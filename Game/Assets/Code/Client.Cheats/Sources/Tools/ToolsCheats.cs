using System.Diagnostics.CodeAnalysis;
using System.IO;
using Client.App.Contracts;
using Client.Cheats.Contracts;
using Client.Core;
using Client.Core.Common.Contracts;
using Client.Core.Common.UI.SystemDialog;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using XLib.Assets.Contracts;

namespace Client.Cheats.Tools {

	[SuppressMessage("ReSharper", "UnusedType.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	[CheatCategory("Tools"), PublicAPI]
	public static class ToolsCheats {
		// [CheatPluginGUI("Tools/Version")]
		// private static void Info(IMetaHost metaHost, IUnityApplication app, INetworkController networkController) {
		// 	if (metaHost != null) CheatGui.LazyLabel("Profile", () => $"ProfileId: \n{metaHost.Id}\n{app.Network} (Version {VersionService.FullVersionString})");
		// 	if (networkController != null) CheatGui.LazyLabel("Network", networkController.DumpDebugInfo);
		// 	if (metaHost != null) {
		// 		CheatGui.LazyLabel("Status", () => {
		// 			var status = metaHost.GetModule<Status>();
		// 			return $"Created: {status.Created}\nLastAuth: {status.LastAuth}\nSession: {status.Session}\n\n";
		// 		});
		// 	}
		// }

		// private static class CheatHostDumper<T> where T : LogicHost<T> {
		// 	private static readonly string[] Names = LogicHostMetaData.Get<T>().Names.Select(s => s.Replace("Module", "")).ToArray();
		// 	private static int _selectMetaModule = 0;
		// 	private static Vector2 _menuScrollPosition;
		// 	private static Vector2 _scrollPosition;
		//
		// 	public static void DrawDump(IHost host) {
		// 		if (host == null) {
		// 			GUILayout.Label("Host is null");
		// 			return;
		// 		}
		// 		
		// 		using (new GUILayout.HorizontalScope()) {
		// 			_menuScrollPosition = GUILayout.BeginScrollView(_menuScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(120));
		// 			var id = GUILayout.SelectionGrid(_selectMetaModule, Names, 1, GUI.skin.button, GUILayout.Width(100));
		// 			GUILayout.EndScrollView();
		// 		
		// 			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);
		// 			var name = LogicHostMetaData.Get<T>().Names.ElementAt(id);
		// 			var module = host.GetModule(name);
		// 			if (module != null) 
		// 				CheatGui.LazyLabel(Names[id], module.ExportDebugInfo, id != _selectMetaModule);
		//
		// 			_selectMetaModule = id;
		// 			GUILayout.EndScrollView();
		// 		}
		// 	}
		// }

		[CheatMethod("Tools/Toggle Cheats Button")]
		private static void ToggleCheatsButton() {
			ConsoleInstaller.InGameCheatsButton.BoolValue = !ConsoleInstaller.InGameCheatsButton.BoolValue;
			ConsoleInstaller.SaveVariables();
		}

		[CheatMethod("Tools/Restart Game")]
		public static void RestartGame(IUnityApplication unityApplication) {
			unityApplication.Reset(true).Forget();
		}

		[CheatMethod("Tools/Clear Bundles Cache")]
		private static void ClearBundlesCache(IAssetProvider assetProvider) {
			 assetProvider.SetNeedClearCacheAsync();
#if UNITY_EDITOR
			 UnityEditor.EditorApplication.isPlaying = false;
#else
			 UnityEngine.Application.Quit();
#endif
		}

#if UNITY_EDITOR
		// [CheatMethod("Tools/Save Profile For Tests")]
		// private static async Task SaveTestProfile(ISharedLogicService logicService) {
		// 	var metaHost = logicService.GetHost<IMetaHost>();
		// 	var dump = metaHost.Dump();
		// 	var status = metaHost.GetModule<Status>();
		// 	var innerId = status.InnerId;
		// 	var fileName = $"{innerId}.{DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.profile";
		//
		// 	var profilesFolder = $"{Directory.GetCurrentDirectory()}/Assets/TestProfiles/";
		//
		// 	var path = Path.Combine(profilesFolder, fileName);
		//
		// 	await File.WriteAllBytesAsync(GetProfilePath(path), NetHelpers.SerializeSharedRawData(dump));
		// }
#endif

		private static string GetProfilePath(string initialProfilePath, int index = 0) {
			if (!File.Exists(initialProfilePath)) return initialProfilePath;
			if (index == 0) return GetProfilePath(initialProfilePath, ++index);

			if (!File.Exists($"{initialProfilePath}-{index}")) return $"{initialProfilePath}-{index}";
			return GetProfilePath(initialProfilePath, ++index);
		}

		[CheatMethod("Tools/Game Dialog")]
		private static void ShowGameDialog(ISystemDialog systemDialog) =>
			systemDialog.ShowAsync("Game Dialog", "From cheats", "Close").Forget();

		[CheatMethod("Tools/System Dialog")]
		private static void ShowSystemDialog(ISystemDialog systemDialog) =>
			systemDialog.ShowAsync("System Dialog", "From cheats", "Close", DialogType.SystemOverlay).Forget();

		// [CheatMethod("Tools/Fullscreen Dialog")]
		// private static void ShowFullscreenDialog(IScreenManager screenManager) => screenManager.ShowDialogFullscreen(GlobalKeys.Reward, GlobalKeys.Reward).Forget();
	}

}