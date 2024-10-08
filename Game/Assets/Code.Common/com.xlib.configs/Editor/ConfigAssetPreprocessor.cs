using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;
using XLib.Configs.Utils;
using XLib.Core.Utils;
using XLib.Unity.Utils;

namespace XLib.Configs {

	public class ConfigAssetPreprocessor : AssetPostprocessor {

		[MenuItem("Tools/Database/Fix configs name", false, -102)]
		private static void FixConfigs() {
			AssetDatabase.FindAssets("*", new[] {"Assets/Configs"})
				.Select(AssetDatabase.GUIDToAssetPath)
				.ForEach(path => {
					if (!File.Exists(path)) return;
					Debug.Log("Fixing: " + path);
					File.WriteAllText(path, File.ReadAllText(path).ReplaceAll("_Name: sds_", "_Name: "));
				});
		}
		
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			try {
				using var _ = EditorUtils.FilterLockable.Lock();
				AssetDatabase.StartAssetEditing();

				var manifests = EditorUtils.LoadAssets<ConfigManifest>();

				var sources = manifests
					.Select(manifest => (Path: Path.GetFullPath(Path.GetDirectoryName(AssetDatabase.GetAssetPath(manifest))) + Path.DirectorySeparatorChar, Config: manifest))
					.ToHashSet();

				var gameItemsUpdated = false;
				foreach (var asset in importedAssets.Concat(movedAssets)) {
					var fullPath = Path.GetFullPath(asset);
					var manifestInfo = sources.FirstOrDefault(source => fullPath.StartsWith(source.Path));
					if (manifestInfo.Config == null) continue;

					if (!TypeOf<GameItemCore>.IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(asset)))
						continue;

					var main = AssetDatabase.LoadMainAssetAtPath(asset) as GameItemCore;

					var items = AssetDatabase.LoadAllAssetsAtPath(asset).OfType<GameItemCore>();
					var assetPath = AssetDatabase.GetAssetPath(main);
					foreach (var gameItem in manifestInfo.Config.Configs) {
						if(gameItem == null || gameItem.Id == ItemId.None) continue;
						
						foreach (var core in items) {
							if (core.Id == gameItem.Id && AssetDatabase.GetAssetPath(core) != assetPath)
								((IGameItemEditor)core).SetId(ItemId.None);
						}
					}

					ConfigUtils.UpdateItemId(main);
					foreach (var item in items) ConfigUtils.UpdateItemId(item);

					gameItemsUpdated = true;
				}

				var deleteGameItem = deletedAssets
					.Where(s => !string.IsNullOrEmpty(s))
					.Where(s => sources.Any(source => Path.GetFullPath(s).StartsWith(source.Path)))
					.ToArray();
				if (gameItemsUpdated || deleteGameItem.Length > 0) {
					foreach (var manifest in manifests) ((IEditorAssetManifest)manifest).EditorInitialize();

					AssetDatabase.SaveAssets();
				}
			}
			finally {
				AssetDatabase.StopAssetEditing();
			}
		}
	}

}