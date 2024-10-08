using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XLib.BuildSystem;
using XLib.BuildSystem.Types;
using XLib.Unity.Utils;

namespace XLib.UI.Internal {

	public class UIBuildPreProcessor : IBeforeBuildRunner {
		public int Priority => 0;

		[MenuItem("Build/Tools/Sync Screen Scenes With Build", false, 100)]
		public static void SyncScreenScenesWithBuild() {
			var report = new RunnerReport(new Logger("UI"));
			ProcessScenes(report);
			report.ThrowOnError();
		}

		private static void ProcessScenes(RunnerReport report) {
			var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

			for (var index = 0; index < scenes.Count; index++) {
				var scene = scenes[index];
				var path = AssetDatabase.GUIDToAssetPath(scene.guid);
				if (scene.path.IsNullOrEmpty() || !System.IO.File.Exists(System.IO.Path.GetFullPath(scene.path))) {
					report.ReportError($"ERROR: Scene #{index} '{scene.guid}' '{path}' does not exist - remove it!");
				}
			}

			if (report.HasErrors) return;

			foreach (var path in EditorUtils.GetAssetPaths<SceneAsset>("Screen")) {
				var newScene = new EditorBuildSettingsScene(path, true);
				if (scenes.FirstOrDefault(x => x.path == newScene.path) != null) continue;
				scenes.Add(newScene);
			}

			EditorBuildSettings.scenes = scenes.ToArray();

			Debug.Log($"Sync Screen Scenes With Build: finished, {EditorBuildSettings.scenes.Length} scene(s) found");
		}

		public void OnBeforeBuild(BuildRunnerOptions options, RunnerReport report) {
			ProcessScenes(report);
		}
	}

}