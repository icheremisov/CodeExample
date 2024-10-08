using UnityEditor;
using UnityEngine;
using XLib.BuildSystem.GameDefines;
using XLib.BuildSystem.Types;

namespace XLib.BuildSystem {

	public class CustomConfigWindow : EditorWindow {

		private static BuildRunnerOptions _options;
		private static string _configName;

		private void OnGUI() {
			
			if (_options == null) return;
			
			EditorGUILayout.LabelField($"{_configName}");
			GUILayout.Space(5);
			
			EditorGUILayout.LabelField($"Version: {VersionService.FullVersionString.Replace("\n", "; ")}");
			EditorGUILayout.LabelField($"Platform: {_options.Target}");
			GUILayout.Space(30);
			
			EditorGUILayout.Space();
			_options.BundleId = EditorGUILayout.TextField("Bundle ID", _options.BundleId);
			if (GUILayout.Button("Detect Bundle ID")) _options.BundleId = PlayerSettings.applicationIdentifier;

			EditorGUILayout.Space();
			_options.DevelopmentBuild = EditorGUILayout.Toggle("Development Build", _options.DevelopmentBuild);
			
			EditorGUILayout.Space();
			_options.UseDevGoogleServices = EditorGUILayout.Toggle("Use Dev GoogleServices", _options.UseDevGoogleServices);
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Game Features");

			var features = GameFeatureConfig.LoadConfig();
			
			foreach (var configEntry in features) {
				var enabled = EditorGUILayout.Toggle($"   {configEntry.Name}", _options.HasFeature(configEntry.Name));
				_options.SetFeature(configEntry.Name, enabled);
			}

			EditorGUILayout.Space();

			switch (_options.Target) {
				case BuildTarget.Android: {
					_options.AppBundle = EditorGUILayout.Toggle("Build app bundle", _options.AppBundle);
					_options.ExportAndroidStudio = EditorGUILayout.Toggle("AndroidStudio project", _options.ExportAndroidStudio);
					_options.AndroidCreateSymbols = (AndroidCreateSymbols)EditorGUILayout.EnumPopup("Create symbols", _options.AndroidCreateSymbols);

					_options.KeystorePath = EditorGUILayout.TextField("Keystore path", _options.KeystorePath);
					_options.KeystorePass = EditorGUILayout.TextField("Keystore password", _options.KeystorePass);
					_options.KeyaliasName = EditorGUILayout.TextField("Keyalias name", _options.KeyaliasName);
					_options.KeyaliasPass = EditorGUILayout.TextField("Keyalias password", _options.KeyaliasPass);
					break;
				}

				case BuildTarget.iOS: {
					// iOSUploadToITunes = EditorGUILayout.Toggle("Upload to iTunes", iOSUploadToITunes);
					break;
				}
				
				case BuildTarget.StandaloneWindows64: {
					// iOSUploadToITunes = EditorGUILayout.Toggle("Upload to iTunes", iOSUploadToITunes);
					break;
				}
			}

			if (GUILayout.Button("Build!")) {
				var options = _options.Clone();
				Close();
				BuildRunner.Build(options);
			}
		}

		public static void ShowWindow(RunBuildWindow.OptionsDesc options) {
			_options = options.Options;
			_configName = options.Title;
			GetWindow(typeof(CustomConfigWindow), false, "Custom Build Config");
		}

	}

}