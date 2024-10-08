#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Sheets.Contracts;
using XLib.Configs.Sheets.Core;
using Object = UnityEngine.Object;

namespace XLib.Configs.Sheets.Converters {
	
	public abstract class UnityEngineObjectSheetsConverter<T> : SheetsConverter<T, string> where T : Object {
		private const string Assets = "Assets/";
		public override IEnumerable<object> GetValues(SheetRowProperty property) {
			var type = property.ElementType ?? property.Type;
			var filter = $"t:{type.Name}";
			var searchInFolders = Array.Empty<string>();

			var assetSelector = property.Attributes.OfType<AssetSelectorAttribute>().FirstOrDefault();
			if (assetSelector != null) {
				filter = string.IsNullOrEmpty(assetSelector.Filter) ? filter : assetSelector.Filter;
				searchInFolders = assetSelector.SearchInFolders;
			}
			else {
				Debug.LogWarning("Use AssetSelectorAttribute for property: " + property.Name);
				return Enumerable.Empty<object>();
			}
			
			return AssetDatabase.FindAssets(filter, searchInFolders).Select(s => AssetDatabase.GUIDToAssetPath(s).Substring(Assets.Length));
		}

		public override string To(T obj, Type type) => obj == null ? null : AssetDatabase.GetAssetPath(obj).Substring(Assets.Length);

		public override T From(string value, Type type) => string.IsNullOrEmpty(value) ? null : AssetDatabase.LoadAssetAtPath<T>(Assets + value);
	}

	[SheetsConverter(typeof(GameObject))]
	public class GameObjectSheetsConverter : UnityEngineObjectSheetsConverter<GameObject> {}
	
	[SheetsConverter(typeof(Sprite))]
	public class SpriteSheetsConverter : UnityEngineObjectSheetsConverter<Sprite> {}

	[SheetsConverter(typeof(AudioClip))]
	public class AudioClipSheetsConverter : UnityEngineObjectSheetsConverter<AudioClip> {}

}
#endif