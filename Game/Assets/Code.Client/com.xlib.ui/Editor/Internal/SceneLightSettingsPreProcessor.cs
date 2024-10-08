using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using XLib.BuildSystem;
using XLib.BuildSystem.Types;
using XLib.Unity.Scene;

namespace XLib.UI.Internal {

	public class SceneLightSettingsPreProcessor : IBeforeBuildRunner {
		public int Priority => 1;

		[MenuItem("Build/Tools/Set Scenes Light Settings", false, 110)]
		public static void SetScenesLightSettings() {
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
				ProcessScenes();
			}
		}

		public static void ProcessScenes() {
			var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
			using var _ = SceneManagerHelper.UnloadAllScenes();
			try {
				var allScenes = scenes.Select(x => x.path).ToArray();
				for (var index = 0; index < allScenes.Length; index++) {
					EditorUtility.DisplayProgressBar("Set Scenes Light Settings", $"Processing scenes {(index + 1)} of {allScenes.Length}", (float)index / allScenes.Length);
					var path = allScenes[index];
					var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
					ScreenScenesAssetSaver.SetupLightSettings(scene);
					EditorSceneManager.SaveScene(scene);
				}
			}
			finally {
				EditorUtility.ClearProgressBar();
			}
		}

		public void OnBeforeBuild(BuildRunnerOptions options, RunnerReport report) {
			ProcessScenes();
		}
	}

}