#if FEATURE_CONSOLE || FEATURE_CHEATS

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Client.Core.DebugLogCollector.Contracts;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using XLib.BuildSystem;
using XLib.Core.Runtime.Discord;
using XLib.Core.Utils;
using XLib.Unity.Utils;
using static UnityEngine.Screen;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Client.Core.DebugLogCollector.UI
{
    public class DebugMessageWindow : MonoBehaviour
    {
        private const string PrefsUserName = "PlayerName";

        private const string WebhookUrl = "";
//            "https://discord.com/api/webhooks/1045635048482807830/Dggjdglm-bnObE0J_CenyovFCB7erv9V7bWAtJFE2mnxXTfLRhPNbYIcrBSh5zCTcxso";

        private const int ToMb = 1048576;
        private string _un;

        private string UserName
        {
            get => _un;
            set
            {
                _un = value;
                PlayerPrefs.SetString(PrefsUserName, value);
            }
        }

        private string _additionalInfo = string.Empty;
        private Texture2D _tex;
        private GUIStyle _currentStyle;
        private Vector2 _vertScroll;

        private IDebugLogCollector _debugLogCollector;
        // [CanBeNull] private IBattleLogCollector _battleLogCollector;
        // [CanBeNull] private ISharedLogicService _sharedLogicService;
        // [CanBeNull] private INetworkController _networkController;

        private bool _sending;
        private bool _sendingEnded;
        private bool _endedWithError;
        private EventSystem _eventSystem;

        private bool _prevEventsEnabled;

        private class EmptyLogCollector : IDebugLogCollector
        {
            public int TotalErrors => 0;
            public string GetLog() => "<<empty log>>";

            public string GetErrorLog() => string.Empty;

            public string FirstError => string.Empty;

            public void ClearLog()
            {
            }
        }

        public void Show(IDebugLogCollector debugLogCollector)
        {
            _debugLogCollector = debugLogCollector ?? new EmptyLogCollector();
            _eventSystem = FindObjectOfType<EventSystem>();

            if (_eventSystem)
            {
                _prevEventsEnabled = _eventSystem.enabled;
                _eventSystem.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_eventSystem) _eventSystem.enabled = _prevEventsEnabled;
        }

        private void Awake()
        {
            UserName = PlayerPrefs.GetString(PrefsUserName) ?? string.Empty;
            _tex = GfxUtils.MakeTex(2, 2, new Color(0.5f, 0.5f, 0.5f, 1f));
        }

        private void OnGUI()
        {
            _currentStyle ??= new GUIStyle(GUI.skin.box)
                { normal = { background = _tex }, wordWrap = true, stretchHeight = true };
            var screenCenter = new Vector2(width / 2.0f, height / 2.0f);
            var popupSize = new Vector2(screenCenter.x / 2.0f, screenCenter.y / 2.0f);
            GUI.skin.label.fontSize = GUI.skin.textField.fontSize = GUI.skin.textArea.fontSize =
                GUI.skin.button.fontSize = _currentStyle.fontSize = (int)popupSize.x / 20;
            var areaRect = new Rect(screenCenter.x - popupSize.x, screenCenter.y - popupSize.y, popupSize.x * 2,
                popupSize.y * 2);

            if (_sending)
            {
                var windSize = new Vector2(screenCenter.x / 4.0f, screenCenter.y / 4.0f);
                GUI.ModalWindow(1,
                    new Rect(screenCenter.x - windSize.x, screenCenter.y - windSize.y, windSize.x * 2, windSize.y * 2),
                    DoOkWindow,
                    _sendingEnded != true ? "Sending..." : _endedWithError ? "Sending failed" : "Sending complete",
                    _currentStyle);
                return;
            }

            GUILayout.BeginArea(areaRect, _currentStyle);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Enter your name:");

            UserName = GUILayout.TextField(UserName, 20, GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(areaRect.width / 2), GUILayout.Width(areaRect.width / 2));

            GUILayout.EndHorizontal();

            GUILayout.Label("Additional Information:");

            _vertScroll = GUILayout.BeginScrollView(_vertScroll, GUIStyle.none, GUIStyle.none);

            var currentStyle = new GUIStyle(GUI.skin.textArea) { wordWrap = true, stretchHeight = false };
            _additionalInfo = GUILayout.TextArea(_additionalInfo, 200, currentStyle, GUILayout.ExpandHeight(true));

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Send"))
            {
                var log = _debugLogCollector.GetErrorLog();
                SendDebugLog(log)
                    .Forget(() => _sendingEnded = true, ex =>
                    {
                        Debug.LogException(ex);
                        _sendingEnded = true;
                        _endedWithError = true;
                    });
            }

            if (GUILayout.Button("Close"))
            {
                Destroy(gameObject);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private async UniTask SendDebugLog(string log)
        {
            enabled = false;

            _sending = true;
            var startFrame = Time.frameCount;

            await UniTask.WaitForEndOfFrame(this);

            log = RichTag.StripTags(log);
            var hook = new DiscordWebhook { Url = WebhookUrl };

            var dt = DateTime.Now;

            var connectionStr = Application.internetReachability switch
            {
                NetworkReachability.NotReachable => "no network",
                NetworkReachability.ReachableViaCarrierDataNetwork => "Mobile",
                NetworkReachability.ReachableViaLocalAreaNetwork => "WiFi or cable",
                _ => throw new ArgumentOutOfRangeException()
            };

            var sb = new StringBuilder();

            var firstErrorStr = RichTag.StripTags(_debugLogCollector.FirstError);
            if (firstErrorStr.Length > 1500) firstErrorStr = firstErrorStr[..1000];

            sb.AppendLine($"{UserName}: {_additionalInfo}");
            if (_debugLogCollector.TotalErrors > 0)
                sb.AppendLine(
                    $"**FirstError**: ```{firstErrorStr}``` and {_debugLogCollector.TotalErrors - 1} more errors");
            sb.AppendLine(
                $"**Platform**: {Application.platform} **TimeSinceStartup**: {TimeSpan.FromSeconds(Time.realtimeSinceStartup):hh\\:mm\\:ss}");
            sb.AppendLine(
                $"**Version**: {VersionService.VersionString}{(VersionService.EnvironmentName.IsNullOrEmpty() ? "" : $" {VersionService.EnvironmentName}")} **Branch**: {GitInfoUtils.GetGitBranch()}");
            sb.Append($"**Device** {SystemInfo.deviceModel} ~ {SystemInfo.deviceName} ~ {SystemInfo.deviceType} ~ ");
            sb.AppendLine(
                $":battery: {(int)(SystemInfo.batteryLevel * 100)}%; RAM Total:{SystemInfo.systemMemorySize:n0}Mb Al.:{Profiler.GetTotalAllocatedMemoryLong() / ToMb:n0}Mb Res.:{Profiler.GetTotalReservedMemoryLong() / ToMb:n0}Mb; graphics {SystemInfo.graphicsMemorySize / 1024.0f:n2}Gb; {connectionStr}");

            // if (_networkController != null) {
            // 	sb.AppendLine();
            // 	sb.AppendLine(_networkController.DumpDebugInfo().Replace("<b>", "**").Replace("</b>", "**"));
            // }


            var message = new DiscordMessage
                { Content = sb.ToString() /*, Thread_name = $"Thread_Test{DateTime.Now}"*/ };

            var screenShot = ScreenCapture.CaptureScreenshotAsTexture();

            await UniTask.WaitWhile(() => Time.frameCount <= startFrame + 2);

            enabled = true;

            var filesData = new List<FileData>();

            if (!log.IsNullOrEmpty())
                filesData.Add(new FileData($"logError-{dt:yyyyMMdd-HHmmss}.txt", Encoding.UTF8.GetBytes(log)));
            filesData.Add(new FileData($"log-{dt:yyyyMMdd-HHmmss}.jpg", screenShot.EncodeToJPG()));

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                {
                    try
                    {
                        var logError = log;
                        if (!logError.IsNullOrEmpty())
                            await AddFileToArchive($"logError-{dt:yyyyMMdd-HHmmss}.txt", archive,
                                Encoding.UTF8.GetBytes(logError));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }

                    try
                    {
                        var logAll = _debugLogCollector.GetLog();
                        if (!logAll.IsNullOrEmpty())
                            await AddFileToArchive($"logAll-{dt:yyyyMMdd-HHmmss}.txt", archive,
                                Encoding.UTF8.GetBytes(logAll));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }

                    try
                    {
                        // var sessions = NetLogger.Sessions.Count > 0 ? NetLogger.Sessions.JoinToString('\n') : "<<no sessions found>>";
                        // await AddFileToArchive($"sessions-{dt:yyyyMMdd-HHmmss}.txt", archive, Encoding.UTF8.GetBytes(sessions));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }


// #if FEATURE_BATTLELOGS
// 					if (_battleLogCollector != null) {
// 						var allText =
//  _battleLogCollector.TakeAll().Select(battleLog => battleLog.Message).JoinToString("\n");
// 						allText = RichTag.StripTags(allText);
// 						if (allText.IsNotNullOrEmpty()) await AddFileToArchive($"battleLog-{dt:yyyyMMdd-HHmmss}.txt", archive, Encoding.UTF8.GetBytes(allText));
// 					}
// #endif

                    await GetProfile(archive);
                }

                filesData.Add(new FileData("attachments.zip", memoryStream.ToArray()));
            }

            await hook.Send(message, filesData.ToArray());
            _debugLogCollector.ClearLog();
            PlayerPrefs.Save();

            if (screenShot) Destroy(screenShot);
        }

        private async UniTask AddFileToArchive(string fileName, ZipArchive archive, byte[] data)
        {
            var file = archive.CreateEntry(fileName, CompressionLevel.Optimal);
            await using var entryStream = file.Open();
            using var fileToCompressStream = new MemoryStream(data);
            await fileToCompressStream.CopyToAsync(entryStream);
        }

        private async UniTask GetProfile(ZipArchive archive)
        {
            // if (_sharedLogicService == null) return;

            try
            {
                // var dumps = _sharedLogicService.GetHostsSerializedDumps();
                // foreach (var dump in dumps) await AddFileToArchive($"{dump.Key.GetType().Name}.profile", archive, dump.Value);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private void DoOkWindow(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (_sendingEnded && GUILayout.Button("OK", GUILayout.ExpandWidth(true))) Destroy(gameObject);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}

#endif