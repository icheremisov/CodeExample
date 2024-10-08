using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XLib.BuildSystem.GameDefines;
using XLib.BuildSystem.Types;

namespace XLib.BuildSystem {

	public class RunBuildWindow : EditorWindow {
		
		public struct OptionsDesc {
			public OptionsDesc(string title, BuildRunnerOptions options) {
				Title = title;
				Options = options;
			}
			public string Title { get; }
			public BuildRunnerOptions Options { get; }
		}

		private OptionsDesc[] _optionsList;

		private OptionsDesc[] OptionsList => _optionsList ??= LoadOptionsList();

		private static string _buildConfigFolder;
		private static string BuildConfigFolder => _buildConfigFolder ??= Path.Join(Application.dataPath, "../../build/config");  

		private static OptionsDesc[] LoadOptionsList() => 
			Directory.GetFiles(BuildConfigFolder, "*.json", SearchOption.TopDirectoryOnly).Where(FileSelector)
			.SelectToArray(fn => {
				var options = new BuildRunnerOptions();
				foreach (var feature in GameFeatureConfig.LoadConfig()) options.SetFeature(feature.Name, feature.DefaultOn);
				options.OverrideFrom(BuildRunnerOptions.FromJsonFile(fn));
					
				return new OptionsDesc(Path.GetFileNameWithoutExtension(fn), options);
			});

		private static bool FileSelector(string x) {
			x = Path.GetFileNameWithoutExtension(x);
			if (x.StartsWith("_")) return false;
			if (x.StartsWith("shared")) return false;
			return true;
		}

		private void OnGUI() {
			
			GUILayout.BeginVertical();

			EditorGUILayout.LabelField($"Version: {VersionService.FullVersionString}");
			GUILayout.Space(30);
			
			foreach (var runnerOptions in OptionsList) {
				GUILayout.BeginHorizontal();
				var oldColor = GUI.backgroundColor;
				GUI.backgroundColor = GetColor(runnerOptions.Title);
				
				if (GUILayout.Button($"Build: {runnerOptions.Title}")) {
					
					if (EditorUtility.DisplayDialog("Confirmation", $"Do You want to start building '{runnerOptions.Title}'?", "Build", "Cancel")) {
						GUILayout.EndHorizontal();
						GUILayout.EndVertical();
						
						var options = runnerOptions.Options.Clone();
						BuildRunner.Build(options);
						return;
					}
				}
				GUI.backgroundColor = oldColor;
				
				if (GUILayout.Button("Custom...", GUILayout.Width(65))) {
					CustomConfigWindow.ShowWindow(runnerOptions);
				}
				GUILayout.EndHorizontal();
			}
			
			GUILayout.EndVertical();
		}

		private static Color GetColor(string title) {
			if (title.Contains("internal")) return Color.green;
			if (title.Contains("public")) return Color.red;
			if (title.Contains("rc")) return Color.yellow;
			return Color.white;
		}

		public static void ShowWindow() {
			GetWindow(typeof(RunBuildWindow), false, "Run Build");
		}

	}

}