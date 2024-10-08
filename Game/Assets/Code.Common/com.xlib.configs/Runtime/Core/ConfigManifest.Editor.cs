#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Utils;
using XLib.Core.Utils;
using XLib.Unity.Utils;

namespace XLib.Configs.Core {

	public partial class ConfigManifest : IEditorAssetManifest {
		private class GameItemCoreComparer : System.Collections.Generic.IComparer<GameItemCore> {
			public int Compare(GameItemCore x, GameItemCore y) {
				if (ReferenceEquals(x, y)) return 0;
				if (ReferenceEquals(null, y)) return 1;
				if (ReferenceEquals(null, x)) return -1;
				var xi = x.Id.AsInt();
				var yi = y.Id.AsInt();
				return xi.CompareTo(yi);
			}
		}

		private List<string> GetConfigGuids() {
			var path = AssetDatabase.GUIDToAssetPath(EditorUtils.GetGuid(this));
			var guids = AssetDatabase.FindAssets($"t:{nameof(GameItemCore)}", new[] { Path.GetDirectoryName(path) }).Distinct().ToList();
			guids.Sort();
			return guids;
		}
		
		[Button("Update", ButtonSizes.Gigantic), GUIColor(0, 1, 0)]
		void IEditorAssetManifest.EditorInitialize() {
			var start = DateTime.Now.Ticks;

			var guids = GetConfigGuids();

			var prevGuids = new Dictionary<string, GameItemBase>();
			foreach (var config in _configs) {
				if(config == null) continue;
				var assetPath = AssetDatabase.GetAssetPath(config);
				var guid = AssetDatabase.AssetPathToGUID(assetPath);
				prevGuids[guid] = AssetDatabase.LoadMainAssetAtPath(assetPath) as GameItemBase;
			}
			var dirty = guids.Count != _configs.Count;
			_configs.SetLength(guids.Count);
			for (var i = guids.Count - 1; i >= 0; --i) {
				var obj = _configs[i];
				var guid = obj != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)) : null;
				if (guid != guids[i]) {
					if (prevGuids.TryGetValue(guids[i], out var prev)) _configs[i] = prev;
					else _configs[i] = AssetDatabase.LoadAssetAtPath<GameItemCore>(AssetDatabase.GUIDToAssetPath(guids[i]));
					dirty = true;
				}
			}
			
			var delta = DateTime.Now.Ticks - start;
			Debug.Log($"ConfigManifest.EditorInitialize: {delta/1000000f} ticks");

			if (dirty) {
				this.SetObjectDirty();
				AssetDatabase.SaveAssetIfDirty(this);
			}
		}
		
		[Button("Report", ButtonSizes.Medium), GUIColor(1, 0, 0)]
		private void GenerateReport() {
			var map = _configs.Where(core => core.GetType().Name == "BackgroundDefinition").ToDictionary(core => core.FileName, core => core.Id.AsInt());
			File.WriteAllText(AssetDatabase.GetAssetPath(this).Replace(".asset", ".json"),
				JsonConvert.SerializeObject(map));
			Debug.Log("Report generated");
		}

		public bool NeedUpdateConfigs() {
			var guids = GetConfigGuids();
			if (guids.Count != _configs.Count) return true;
			
			for (var i = guids.Count - 1; i >= 0; --i) {
				var obj = _configs[i];
				var guid = obj != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)) : null;
				if (guid != guids[i]) return true;
			}

			return false;
		}

		[Button("Show Config Hash", ButtonSizes.Medium), GUIColor(0, 0.75f, 0.75f)]
		public void CalculateHash() => Debug.Log($"CONFIG HASH: {CalculateConfigHash(Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) ?? string.Empty)}");

		public string CalculateConfigHash(string configDirectory) {
			var allAssets = Directory.EnumerateFiles(configDirectory, "*.asset", SearchOption.AllDirectories)
				.OrderBy(s => s, StringComparer.Ordinal).ToArray();
			return ConfigUtils.CalculateHash(allAssets);
		}
	}

}
#endif