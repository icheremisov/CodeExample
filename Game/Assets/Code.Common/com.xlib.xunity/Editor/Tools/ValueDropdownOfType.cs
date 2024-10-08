using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using XLib.Unity.Utils;
using Object = UnityEngine.Object;

namespace XLib.Unity.Tools {

	public class ValueDropdownOfType {
		public static IEnumerable All(InspectorProperty property) {
			var type = property.ValueEntry.TypeOfValue;
			var config = property.GetAttribute<ValueDropdownOfAttribute>();
			var name = config?.Name ?? string.Empty;
			var searchInFolders = config?.SearchInFolders ?? Array.Empty<string>();
			return AssetDatabase.FindAssets($"t:{type.Name} {name}", searchInFolders)
				.Select(AssetDatabase.GUIDToAssetPath)
				.SelectMany(AssetDatabase.LoadAllAssetsAtPath)
				.Where(o => type.IsInstanceOfType(o))
				.Select(obj => new ValueDropdownItem(obj.name, obj));
				
		}
	}

}