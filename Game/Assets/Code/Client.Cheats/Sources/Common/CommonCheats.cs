// using System.Globalization;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Client.App.ClientLogin.Contracts;
// using Client.App.ClientLogin.Internal;
// using Client.App.Contracts;
// using Client.Cheats.Contracts;
// using Client.Cheats.Internal;
// using Client.Cheats.Tools;
// using Client.Core.DebugLogCollector.Contracts;
// using Client.Core.DebugLogCollector.UI;
// using Client.Core.DeviceNotifications.Contracts;
// using Client.Core.Dialogs.UI;
// using Client.Core.Sounds;
// using Client.Core.Utils;
// using Client.Meta.RateUs.Contracts;
// using Client.Meta.UI.MetaGame;
// using Client.Meta.UI.TimeScaleDebug;
// using Cysharp.Threading.Tasks;
// using JetBrains.Annotations;
// using Shared.Client.Core.Contracts;
// using Shared.Client.Network.Contracts;
// using Shared.Client.ServerCommunication;
// using Shared.Definitions.Profile;
// using Shared.Logic.Battle.Contracts;
// using Shared.Logic.Meta.Modules;
// using UnityEngine;
// using XLib.Core.CommonTypes;
// using XLib.Core.Utils;
// using XLib.Localization.Public;
// using XLib.UI.Contracts;
// using XLib.UI.Screens;
// using XLib.UI.Views;
// using XLib.Unity.Utils;
// using Zenject;
// using Time = Shared.Logic.Meta.Modules.Time;
//
// namespace Client.Cheats.Common {
//
// 	[CheatCategory("Common"), PublicAPI]
// 	public static partial class CommonCheats {
// 		[CheatMethod("Common/Send Log To Discord")]
// 		private static void SendLogsToDiscord(IDebugLogCollector logCollector, IBattleLogCollector battleLogCollector, ISharedLogicService logicService,
// 			INetworkController networkController) {
// 			new GameObject().AddComponent<DebugMessageWindow>().Show(logCollector, battleLogCollector, logicService, networkController);
// 		}
//
// #if FEATURE_ARTANDTESTING
//
// 		[CheatToggle("Preview/Allow Duplicate Chars")]
// 		private static bool AllowDuplicateChars { get; set; } = false;
//
// 		[CheatMethod("Preview/Single Mission")]
// 		private static void DevSingleMission(IScreenManager screenManager) => GotoDebugBattle(screenManager, MetaGameScreen.DebugBattleMode.DevSingle);
//
// 		[CheatMethod("Preview/Graph Preview")]
// 		private static void GraphPreview(IScreenManager screenManager) => GotoDebugBattle(screenManager, MetaGameScreen.DebugBattleMode.GraphPreview);
//
// 		private static void GotoDebugBattle(IScreenManager screenManager, MetaGameScreen.DebugBattleMode mode) {
// 			if (screenManager.TopVisibleScreen is UIScreen screen && screen.CanPerform(UIBuiltInAction.HomeScreen))
// 				screenManager.PerformAction(screen, UIBuiltInAction.HomeScreen, false).Forget();
//
// 			if (screenManager.TopVisibleScreen is not MetaGameScreen metaGameScreen) {
// 				Debug.LogError("MetaGameScreen not found");
// 				return;
// 			}
//
// 			metaGameScreen.ShowDebugPanel();
// 			metaGameScreen.DebugBattleClick(mode, AllowDuplicateChars);
// 			Cheat.Minimize();
// 		}
// #endif
//
// 		[CheatPluginGUI("Common/Localize")]
// 		public static void LocalizeCheat() {
// 			GUILayout.BeginHorizontal();
//
// 			var enable = GUI.enabled;
// 			foreach (var locale in Localize.Instance.CheatLocales) {
// 				GUI.enabled = enable & (Localize.Instance.CurrentLocale != locale);
// 				if (GUILayout.Button(locale.Id)) Localize.Instance.CurrentLocale = locale;
// 			}
//
// 			GUI.enabled = enable;
// 			GUILayout.EndHorizontal();
// 			
// 			GUILayout.BeginHorizontal();
//
// 			enable = GUI.enabled;
// 			foreach (var locale in Localize.Instance.Locales) {
// 				GUI.enabled = enable & (Localize.Instance.CurrentLocale != locale);
// 				if (GUILayout.Button(locale.Id)) Localize.Instance.CurrentLocale = locale;
// 			}
//
// 			GUI.enabled = enable;
// 			GUILayout.EndHorizontal();
// 		}
//
// 		private static bool _useLocalTime;
//
// 		[CheatPluginGUI("Common/Time")]
// 		public static void Time(Time time, ILockable locker, IUnityApplication unityApplication) {
// 			var zone = Shared.Logic.Meta.Modules.Time.ServerTimeZone;
//
// 			var currentTime = time.GetCurrentTime();
// 			var currentDay = currentTime.Day(zone);
//
// 			if (time.Offset == Duration.Zero)
// 				GUILayout.Label($"[Srv] Server Time: {Format(currentTime)}");
// 			else
// 				GUILayout.Label($"[Srv] Server Time: {Format(currentTime)} (Debug Offset={time.Offset.ToTimeSpan:g})");
//
// 			GUILayout.Label(
// 				$"[Srv] Day Time: Passed={(currentTime - Timestamp.DayStart(currentDay, zone)).ToTimeSpan:g} Left={(Timestamp.DayStart(currentDay + 1, zone) - currentTime).ToTimeSpan:g} (TimeZone={zone.ToTimeSpan:g})");
// 			GUILayout.Space(10);
// 			GUILayout.Label($"[Local] Device Time: {Format(Timestamp.DeviceTime)}");
// 			GUILayout.Label($"[Local] Logic Will Reset At: {Format(Timestamp.DeviceTime + (Timestamp.DayStart(currentDay + 1, zone) - currentTime))}");
//
// 			using (var _ = new GUILayout.HorizontalScope()) {
// 				if (GUILayout.Button("10s")) AdvanceTime(Duration.Seconds(10));
// 				if (GUILayout.Button("1m")) AdvanceTime(Duration.Minutes(1));
// 				if (GUILayout.Button("10m")) AdvanceTime(Duration.Minutes(10));
// 				if (GUILayout.Button("1h")) AdvanceTime(Duration.Hours(1));
// 				if (GUILayout.Button("6h")) AdvanceTime(Duration.Hours(6));
// 				if (GUILayout.Button("1d")) AdvanceTime(Duration.Days(1));
// 				if (GUILayout.Button("6d")) AdvanceTime(Duration.Days(6));
// 				if (GUILayout.Button("30d")) AdvanceTime(Duration.Days(30));
// 				if (GUILayout.Button("Next day")) {
// 					var offset = Timestamp.DayStart(currentDay + 1, zone) - currentTime - Duration.Seconds(10);
// 					if (offset.Value > 0) AdvanceTime(offset);
// 				}
//
// 				if (GUILayout.Button("Reset")) new Time.SetOffset() { Offset = null }.Send(locker);
// 			}
//
// 			using (new GUILayout.HorizontalScope()) {
// 				GUILayout.Label("TimeOffline");
// 				GUILayout.Label($"{unityApplication.AddOfflineTimeOffset(Duration.Zero)}");
// 				if (GUILayout.Button("Restart game")) ToolsCheats.RestartGame(unityApplication);
// 			}
//
// 			using (var _ = new GUILayout.HorizontalScope()) {
// 				if (GUILayout.Button("10s")) AdvanceTime(Duration.Seconds(10), true);
// 				if (GUILayout.Button("1m")) AdvanceTime(Duration.Minutes(1), true);
// 				if (GUILayout.Button("10m")) AdvanceTime(Duration.Minutes(10), true);
// 				if (GUILayout.Button("1h")) AdvanceTime(Duration.Hours(1), true);
// 				if (GUILayout.Button("6h")) AdvanceTime(Duration.Hours(6), true);
// 				if (GUILayout.Button("1d")) AdvanceTime(Duration.Days(1), true);
// 				if (GUILayout.Button("6d")) AdvanceTime(Duration.Days(6), true);
// 				if (GUILayout.Button("30d")) AdvanceTime(Duration.Days(30), true);
// 				if (GUILayout.Button("Next day")) {
// 					var offset = Timestamp.DayStart(currentDay + 1, zone) - currentTime - Duration.Seconds(10);
// 					if (offset.Value > 0) AdvanceTime(offset, true);
// 				}
//
// 				if (GUILayout.Button("Reset")) unityApplication.ResetOfflineTimeOffset();
// 			}
//
// 			GUILayout.Space(10);
// 			using (var _ = new GUILayout.HorizontalScope()) {
// 				var c = GUI.color;
// 				GUI.color = !_useLocalTime ? Color.green : c;
// 				if (GUILayout.Button("GMT", GUILayout.Width(80))) _useLocalTime = false;
// 				GUI.color = _useLocalTime ? Color.green : c;
// 				if (GUILayout.Button("Local", GUILayout.Width(80))) _useLocalTime = true;
// 				GUI.color = c;
// 			}
//
// 			string Format(Timestamp t) =>
// 				_useLocalTime ? t.ToLocalDateTime.ToString(CultureInfo.InvariantCulture) + " (Device)" : t.ToDateTime.ToString(CultureInfo.InvariantCulture) + " GMT";
//
// 			void AdvanceTime(Duration deltaTime, bool isOffline = false) {
// 				if (isOffline) {
// 					unityApplication.AddOfflineTimeOffset(deltaTime);
// 					return;
// 				}
//
// 				new Time.SetOffset() { Offset = time.Offset + deltaTime }.Send(locker);
// 			}
// 		}
//
// 		[Inject] private static TimeScaleDebugView _timeScaleDebugView;
//
// 		[CheatToggle("Common/Time scale panel")]
// 		private static bool TimeScalePanel { get => _timeScaleDebugView.IsVisible(); set => _timeScaleDebugView.SetVisible(!_timeScaleDebugView.IsVisible()); }
//
// 		[CheatPluginGUI("Common/Player Avatar")]
// 		private static void PlayerProfile(PlayerProfile profile, ILockable locker) {
// 			if (CheatGui.Input("Avatar", profile.PlayerAvatar.FileName, out var avatarName)) {
// 				var asset = GameData.All<ProfileAvatarDefinition>().FirstOrDefault(def => def.FileName == avatarName);
// 				if (asset != null)
// 					new PlayerProfile.SetAvatar() { AvatarDef = asset }.Send(locker);
// 				else
// 					Debug.LogError($"No avatar definition with assetName: {avatarName}");
// 			}
//
// 			if (CheatGui.Input("Name", profile.PlayerName, out var playerName)) {
// 				new PlayerProfile.SetName() { PlayerName = playerName }.Send(locker);
// 			}
// 		}
//
// 		[CheatMethod("Common/Delete Save")]
// 		private static async Task DeleteProfile(IUnityApplication unityApplication, IServerCheats serverCheats) {
// 			await PlayerCheatHelper.DeleteAllProfiles(unityApplication.DeviceId, serverCheats, CancellationToken.None);
// 			PlayerCheatHelper.StopApplication();
// 		}
//
// 		[CheatMethod("Common/Enable Notifications")]
// 		private static async Task EnableNotifications(IDeviceNotificationsController deviceNotificationsController) {
// 			await new Status.EnableNotifications() { };
// 			deviceNotificationsController.SetEnabled(true);
// 			await deviceNotificationsController.RequestNotificationPermissions();
// 		}
//
// 		[CheatMethod("Common/Rate Us")]
// 		private static async Task RateUs(IRateUsController rateUsController) {
// 			await rateUsController.TryShow(true);
// 		}
//
// 		[CheatToggle("Logs/Sound play errors")]
// 		private static bool SoundPlayErrors { get => SoundsFlags.LogSoundPlayErrors; set => SoundsFlags.LogSoundPlayErrors = !SoundsFlags.LogSoundPlayErrors; }
//
// #if !FEATURE_DISABLELOGIN
// 		[CheatToggle("Login/Stub")]
// 		private static bool LoginStub { get => SocialController.StubEnabled.Value; set => SocialController.StubEnabled.Value = value; }
//
// 		[CheatPluginGUI("Login/Info")]
// 		private static void LoginInfo(ISocialController socialController, IUnityApplication unityApplication, ILockable lockable, IClientLoginController clientLoginController,
// 			GlobalContext context) {
// 			using (CheatGui.HorizontalGroup()) {
// 				if (context.hasMetaHost && (~context.metaHost).GetModule<Status>(true) != null) {
// 					var status = (~context.metaHost).GetModule<Status>(true);
// 					GUILayout.Label($"InnerId: {status.InnerId} ProfileId: {(~context.metaHost).Id}\nLastToken:{socialController.DefaultPlatform?.LastToken?.ToCropString()}");
// 				}
//
// 				GUILayout.FlexibleSpace();
// 				if (GuiEx.Button(LoginStub ? "Stub" : "Real", LoginStub ? Color.green : Color.red)) {
// 					LoginStub = !LoginStub;
// 					var lockInstance = lockable.Lock();
// 					unityApplication.Reset(true).Forget(lockInstance.Unlock);
// 				}
//
// 				if (GuiEx.Button("Restart", Color.cyan)) {
// 					var lockInstance = lockable.Lock();
// 					unityApplication.Reset(true).Forget(lockInstance.Unlock);
// 				}
//
// 				if (GuiEx.Button("Delete", Color.red)) {
// 					PlayerCheatHelper.DeleteStorageKeys();
// 					PlayerCheatHelper.StopApplication();
// 				}
// 			}
//
// 			if (LoginStub) {
// 				var flags = SocialController.SimulateErrorFlags.Value;
// 				using (CheatGui.HorizontalGroup()) {
// 					Flag("Init", ref flags, SimulateError.InitFailed, SimulateError.InitCanceled);
// 					Flag("Silent", ref flags, SimulateError.SilentLoginFailed, SimulateError.SilentLoginCanceled);
// 					Flag("Login", ref flags, SimulateError.LoginFailed, SimulateError.LoginCanceled);
// 					Flag("Logout", ref flags, SimulateError.LogoutFailed, SimulateError.LogoutCanceled);
// 					if (GuiEx.Button(flags.Has(SimulateError.LongWait) ? "Wait: ON" : "Wait: OFF", flags.Has(SimulateError.LongWait) ? Color.red : Color.green))
// 						flags = flags.With(SimulateError.LongWait, !flags.Has(SimulateError.LongWait));
// 				}
//
// 				SocialController.SimulateErrorFlags.Value = flags;
// 			}
//
// 			foreach (var authToken in socialController.InternalTokens) {
// 				using (CheatGui.HorizontalGroup($"[{authToken.Type}]")) {
// 					GUILayout.Label(authToken.Token);
// 					GUILayout.FlexibleSpace();
// 					if (GuiEx.Button("Unlink", Color.red)) {
// 						socialController.UnlinkToken(authToken).Forget();
// 						break;
// 					}
// 				}
// 			}
//
// 			foreach (var platform in socialController.AllPlatforms) {
// 				var authToken = platform.Type.GetAuthToken();
// 				using (CheatGui.HorizontalGroup($"[{authToken}]")) {
// 					GUILayout.Label(platform.IsLoggedIn ? "[Logged]" : "[Not Logged]");
// 					GuiEx.Tooltip(GetTooltip(platform));
// 					GUILayout.FlexibleSpace();
//
// 					if (!platform.IsLoggedIn && GuiEx.Button("Login", Color.green)) {
// 						var lockInstance = lockable.Lock();
// 						platform.LogIn(CancellationToken.None).Forget(_ => lockInstance.Unlock());
// 					}
//
// 					if (platform.IsLinked) {
// 						if (GuiEx.Button("Unlink", Color.red)) {
// 							platform.Unlink().Forget();
// 						}
// 					}
// 					else {
// 						if (platform.IsLoggedIn && GuiEx.Button("Link", Color.green)) {
// 							platform.Link(null).Forget();
// 						}
// 					}
//
// 					if (GuiEx.Button("R", Color.blue)) {
// 						SocialPlatformStub.RemoveAuthToken(authToken);
// 						var token = SocialPlatformStub.GetTokenByType(authToken, platform.Type.GetShortName());
// 						Debug.Log($"Set Platform Token: {token}");
// 					}
//
// 					for (int i = 0; i < 5; i++) {
// 						var tk = i.ToString();
// 						if (GuiEx.Button(tk, Color.blue)) {
// 							SocialPlatformStub.RemoveAuthToken(authToken);
// 							var token = SocialPlatformStub.GetTokenByType(authToken, platform.Type.GetShortName(), tk);
// 							Debug.Log($"Set Platform Token: {token}");
// 						}
// 					}
//
// 					if (platform.ConflictProfile == null) {
// 						// if (GuiEx.Button("Conflict", Color.red)) {
// 						// 	platform.
// 						// }
// 					}
// 					else {
// 						if (GuiEx.Button("Resolve", Color.green)) {
// 							Cheat.Minimize();
// 							var lockInstance = lockable.Lock();
// 							clientLoginController.CheckPlatformConflict()
// 								.Forget(result => {
// 									lockInstance.Unlock();
// 									if (result == SocialPlatformResult.Restart) unityApplication.Reset(false);
// 								});
// 						}
// 					}
//
// 					if (platform.IsLoggedIn && GuiEx.Button("Logout", Color.red)) {
// 						var lockInstance = lockable.Lock();
// 						platform.LogOut()
// 							.Forget(result => {
// 								lockInstance.Unlock();
// 								Debug.Log(result.ToString());
// 							});
// 					}
// 				}
// 			}
// 		}
//
// 		private static string GetTooltip(ISocialPlatform platform) {
// 			return $"{platform.Type.GetAuthToken()}:{(platform.IsInitialized ? "Active" : "Failed")}\n" +
// 				$"[Login] {platform.LoggedToken?.CropToken ?? "<NONE>"}\n" +
// 				$"[Linked] {platform.LinkedToken?.CropToken ?? "<NONE>"}\n" +
// 				$"[Last] {platform.LastToken?.CropToken ?? "<NONE>"}\n" +
// 				$"[Conflict] {platform.ConflictProfile?.ToString() ?? "<NONE>"}";
// 		}
//
// 		private static void Flag(string name, ref SimulateError flags, SimulateError fail, SimulateError cancel) {
// 			var isFail = flags.Has(fail);
// 			var isCancel = flags.Has(cancel);
// 			if (GuiEx.Button($"{name}: " + (isFail ? "Fail" : isCancel ? "Cancel" : "Ok"), isFail ? Color.red : isCancel ? Color.yellow : Color.green)) {
// 				if (isFail)
// 					flags = flags.With(fail, false).With(cancel, false);
// 				else if (isCancel)
// 					flags = flags.With(fail, true).With(cancel, false);
// 				else
// 					flags = flags.With(fail, false).With(cancel, true);
// 			}
// 		}
// #endif
//
// 		[CheatMethod("Common/Restart dialog")]
// 		private static void RestartDialog(IScreenManager screenManager) {
// 			if (!screenManager.IsInStack(TypeOf<CutSceneDialogScreen>.Raw)) return;
// 			screenManager.GetLastFromStack<CutSceneDialogScreen>().RestartDialog();
// 		}
// 	}
//
// }