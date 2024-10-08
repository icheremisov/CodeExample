using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLib.Unity.Core;

namespace XLib.Unity.Scene {

	/// <summary>
	/// destroy object when game runs from start 
	/// </summary>
	public class ScenePreviewOnly : MonoBehaviour {
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void OnLoadBeforeAwake() {
			SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
		}

		private static void SceneManagerOnsceneLoaded(UnityEngine.SceneManagement.Scene arg0, LoadSceneMode arg1) {
			if (GameLoader.Mode == GameLoadingMode.GameFromStart) {
				foreach (var obj in FindObjectsOfType<ScenePreviewOnly>(true)) obj.gameObject.Destroy();
			}
		}

		private void Awake() {
			if (GameLoader.Mode == GameLoadingMode.GameFromStart) gameObject.Destroy();
		}
#endif
		
		
	}

}