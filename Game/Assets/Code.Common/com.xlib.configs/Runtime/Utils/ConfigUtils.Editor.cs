#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Unity.Utils;

namespace XLib.Configs.Utils {

	public partial class ConfigUtils {
		public static void UpdateItemId(GameItemCore item) {
			var guid = EditorUtils.GetGuid(item);
			if(guid.IsNullOrEmpty()) {
				if (item.Id == ItemId.None)
					guid = GUID.Generate().ToString();
				else return;
			}
			
			var id = GameItemUtils.GuidToItemId(guid);
			if (item.Id != ItemId.None && item.Id != id) {
				Debug.LogWarning($"\"{item.FileName}\" asset id has been changed ({item.Id.ToKeyString()} => {id.ToKeyString()})!", item);
				if (AssetDatabase.IsMainAsset(item)) {
					foreach (var gameItemCore in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(item))
								 .Where(o => o != item)
								 .OfType<GameItemCore>()) {
						((IGameItemEditor)gameItemCore).SetId(ItemId.None);
						UpdateItemId(gameItemCore);
					}
				} 
			}

			if (id != item.Id && (item is IGameItemEditor itemEditor)) {
				itemEditor.SetId(id);
				item.SetObjectDirty();
			}
		}
	}

}
#endif