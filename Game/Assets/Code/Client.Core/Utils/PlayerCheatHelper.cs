using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using XLib.Core.Utils;
using XLib.Unity.LocalStorage;

namespace Client.Core.Utils {

	public static class PlayerCheatHelper {
		private static void DeleteLocalProfiles() {
			// Debug.Log($"DeleteLocalProfiles: {NetConstants.ProfileDirectory}");
			// if (Directory.Exists(NetConstants.ProfileDirectory)) Directory.Delete(NetConstants.ProfileDirectory, true);
			// var idMapper = Path.Combine(UnityAppConstants.persistentDataPath, "id.mapper");
			// if (File.Exists(idMapper)) File.Delete(idMapper);
			DeleteStorageKeys();
		}

		public static void DeleteStorageKeys() {
#if UNITY3D
#if FEATURE_DEMO
			LocalProfileStorage.DeleteAllKeys($"{LocalProfileStorage.ProfileKey}@demo");
#else
			LocalProfileStorage.DeleteAllKeys();
#endif
			DeleteStoredValues();
#endif
		}

		public static void StopApplication() {
			if (UnityAppConstants.isPlaying && !UnityAppConstants.isEditor) Application.Quit(0);
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}

#if UNITY3D
		private static void DeleteStoredValues() {
			PlayerPrefs.DeleteKey("PlayerDeviceId");
			PlayerPrefs.DeleteKey("Login.TermsAgree");
			PlayerPrefs.DeleteKey("Login.FirstRun");
			PlayerPrefs.DeleteKey("Login.Stub");
			PlayerPrefs.DeleteKey("Login.Stub.Enabled");
			PlayerPrefs.DeleteKey("Login.Stub.Error");
			PlayerPrefs.DeleteKey("Login.LoginData");
			PlayerPrefs.DeleteKey("Login.GameCenter");

			PlayerPrefs.DeleteKey("Settings.Volume");
			PlayerPrefs.DeleteKey("Localize_CurrLocale");
			PlayerPrefs.DeleteKey("OffersShown");
			PlayerPrefs.DeleteKey("UnityApplication_PrevNetworkError");
			PlayerPrefs.DeleteKey("Tutorial.debug");
			PlayerPrefs.DeleteKey("IsDeveloper");
		
		}

#endif

#if UNITY_EDITOR

		[UnityEditor.MenuItem("Game/Clear Device Player Data", priority = 1000)]
		private static void EditorDeleteLocalData() {
			if (UnityEditor.EditorUtility.DisplayDialog("Clear Device Player Data", "Clear local storage of Player Profile data (Simulate restoring old profile from server)",
					"Clear", "Cancel")) {
				LocalProfileStorage.DeleteAllKeys();
				DeleteStoredValues();
			}
		}

		[UnityEditor.MenuItem("Game/Delete All Profiles", priority = 1001)]
		public static void DeleteAllProfiles() {
			DeleteLocalProfiles();
			StopApplication();
		}
#else
		public static void DeleteAllProfiles() {}
#endif
	}

}