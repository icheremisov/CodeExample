#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using XLib.Core.Utils;
using XLib.Unity.Utils;

namespace Client.Core.Ecs.Configs {

	public static class EcsEditorMenu {
		public const string EcsDataDir = "Assets/Settings/ECS";
		private const string AssetName = "EcsFeature";

		[MenuItem("ECS/Select Configs")]
		public static void EcsConfig() {
			var config = EditorUtils.LoadFirstAsset<EcsFeatureConfig>();

			if (config == null)
				EcsConfigAdd();
			else {
				EditorUtility.FocusProjectWindow();
				Selection.activeObject = config;
			}
		}

		[MenuItem("ECS/Add Feature", priority = 100)]
		public static void EcsConfigAdd() {
			var asset = ScriptableObject.CreateInstance<EcsFeatureConfig>();

			Directory.CreateDirectory(EcsDataDir);

			var files = Directory.GetFiles(EcsDataDir, $"{AssetName}*.asset", SearchOption.TopDirectoryOnly);
			var newName = $"{AssetName}.asset";
			var nameIndex = 1;
			while (files.Any(x => x.EndsWith(newName, StringComparison.InvariantCultureIgnoreCase))) {
				newName = $"{AssetName}{nameIndex:00}.asset";
				++nameIndex;
			}

			AssetDatabase.CreateAsset(asset, $"{EcsDataDir}/{newName}");

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}


		[DidReloadScripts]
		public static void ScriptsReloaded() {
			if (EditorApplication.isPlayingOrWillChangePlaymode) return;

			using (TraceLog.Usage(nameof(EcsEditorMenu), nameof(ScriptsReloaded))) {
				foreach (var config in EditorUtils.LoadAssets<EcsFeatureConfig>()) if (config) config.UpdateList();
			}
		}
	}

}

#endif