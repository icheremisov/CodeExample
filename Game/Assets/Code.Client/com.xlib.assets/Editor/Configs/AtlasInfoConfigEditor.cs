using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using XLib.Unity.Utils;

namespace XLib.Assets.Configs {

	[CustomEditor(typeof(AtlasInfoConfig))]
	public class AtlasInfoConfigEditor : Editor {

		[MenuItem("CONTEXT/AtlasInfoConfig/Update Info")]
		public static void UpdateAtlasInfoMenu() {
			var config = Selection.activeObject as AtlasInfoConfig;

			if (config == null) config = EditorUtils.LoadExistingAsset<AtlasInfoConfig>();

			if (config == null) {
				Debug.LogError($"Cannot find {nameof(AtlasInfoConfig)} config!");
				return;
			}

			UpdateAtlasInfo(config);

			EditorUtility.SetDirty(config);
		}

		
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			if (GUILayout.Button("Update AtlasInfo")) {
				
			}
		}
		
		private static void UpdateAtlasInfo(AtlasInfoConfig config) {
			var result = new List<AtlasInfoConfig.AtlasDesc>(128);

			result.Clear();
			var groups = EditorUtils.LoadAssets<AddressableAssetGroup>();
			foreach (var assetGroup in groups) {
				foreach (var entry in assetGroup.entries) {
					if (entry.AssetPath.EndsWith("spriteatlas")) ImportUnityAtlas(result, entry);
				}
			}

			config._spriteInfo = result.ToArray();

			Debug.Log($"AtlasInfoConfig Updated. Found {config.SpriteInfo.Length} atlases and {config.SpriteInfo.Select(x => x.sprites?.Length ?? 0).DefaultIfEmpty().Sum()} sprites");
		}
		
		private static void ImportUnityAtlas(List<AtlasInfoConfig.AtlasDesc> result, AddressableAssetEntry entry) {
			var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(entry.AssetPath);
			SpriteAtlasUtility.PackAtlases(new[] { asset }, EditorUserBuildSettings.activeBuildTarget);

			Debug.Log($"Processing {entry.AssetPath}");

			var atlasName = entry.address;
			var desc = new AtlasInfoConfig.AtlasDesc { name = atlasName };
			result.Add(desc);
			var items = new List<string>(128);

			var sprites = new Sprite[asset.spriteCount];
			asset.GetSprites(sprites);
			foreach (var sprite in sprites) {
				sprite.name = sprite.name.Replace("(Clone)", string.Empty);

				if (items.Contains(sprite.name)) {
					Debug.LogError($"Duplicate Image '{sprite.name}' in atlas {atlasName}, key not added.");
					continue;
				}

				var otherAtlas = result.FirstOrDefault(x => x.sprites?.Contains(sprite.name) == true);
				if (otherAtlas != null) {
					Debug.LogError($"Duplicate Image '{sprite.name}' in atlas {atlasName} and {otherAtlas.name}, key not added.");
					continue;
				}

				items.Add(sprite.name);
			}

			desc.sprites = items.ToArray();
		}
		

	}

}