using UnityEditor;
using UnityEngine;

namespace XLib.Configs {

	public static class AssetDatabaseUserData {
		public static T LoadUserData<T>(Object obj) {
			var asset = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
			var userDataString = asset != null ? asset.userData : null;
			return string.IsNullOrEmpty(userDataString) ? default : JsonUtility.FromJson<T>(userDataString);
		}

		public static void SaveUserData<T>(T userData, Object obj) {
			var assetPath = AssetDatabase.GetAssetPath(obj);
			AssetImporter.GetAtPath(assetPath).userData = userData != null ? JsonUtility.ToJson(userData) : "";
			AssetDatabase.WriteImportSettingsIfDirty(assetPath);
		}
	}

}