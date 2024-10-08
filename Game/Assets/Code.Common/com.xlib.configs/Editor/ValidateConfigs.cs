using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Core;
using XLib.Unity.Utils;

namespace XLib.Configs {

	[InitializeOnLoad]
	public static class ValidateConfigs {
		static ValidateConfigs() {
			EditorApplication.playModeStateChanged += PlayModeStateChanged;
		}
		
		private static void PlayModeStateChanged(PlayModeStateChange state) {
			if (state != PlayModeStateChange.EnteredPlayMode) return;
			
			// var typesManifest = EditorUtils.LoadSingleAsset<TypesManifest>();
			//
			// if (typesManifest == null) {
			// 	Debug.LogError("[ValidateConfigs] Can't load TypesManifest for validate.");
			// 	ExitPlaymode().Forget();
			// 	return;
			// }
			//
			// if (typesManifest.NeedUpdateTypes()) {
			// 	Debug.LogError($"[ValidateConfigs] TypesManifest ({typesManifest.name}) invalid. Types list not updated.");
			// 	ExitPlaymode().Forget();
			// 	return;
			// }
			
			var configManifest = EditorUtils.LoadSingleAsset<ConfigManifest>();
			
			if (configManifest == null) {
				Debug.LogError("[ValidateConfigs] Can't load ConfigManifest for validate.");
				ExitPlaymode().Forget();
				return;
			}
			
			if (configManifest.NeedUpdateConfigs()) {
				Debug.LogError($"[ValidateConfigs] ConfigManifest ({configManifest.name}) invalid. Configs list not updated.");
				ExitPlaymode().Forget();
				return;
			}
			
			Debug.Log("[ValidateConfigs] TypesManifest and ConfigManifest valid.");
		}

		private static async UniTask ExitPlaymode() {
			await UniTask.Yield();
			await UniTask.Yield();
			EditorApplication.ExitPlaymode();
		}
	}

}