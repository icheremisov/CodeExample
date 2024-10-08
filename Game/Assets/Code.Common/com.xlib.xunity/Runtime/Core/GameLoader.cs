using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XLib.Unity.Core {

	public enum GameLoadingMode {
		/// <summary>
		/// default mode - play game from start 
		/// </summary>
		GameFromStart,
		
		/// <summary>
		/// play from specific scene (no game initialized)  
		/// </summary>
		PlayFromScene,
	}
	
	public interface IEditorStartupParams { }
	
	public static partial class GameLoader {

		private const string InitializeSceneName = "Initialize";

		public static GameLoadingMode Mode { get; private set; } = GameLoadingMode.GameFromStart;

		public static IEditorStartupParams Params { get; private set; }
		
		public static T GetParams<T>() where T : IEditorStartupParams => (T)Params;
		public static bool HasParams<T>() where T : IEditorStartupParams => Params is T;
		

		static GameLoader() {
#if UNITY_EDITOR
			InitEditor();
#endif
		}
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void OnLoadBeforeAwake() {

			Mode = SceneManager.GetActiveScene().name == InitializeSceneName ? GameLoadingMode.GameFromStart : GameLoadingMode.PlayFromScene;
			
#if UNITY_EDITOR			
			StartInEditor().Forget();
#else
			Debug.Log($"{nameof(GameLoader)}: Mode={Mode}");
#endif
		}
		
		
	}

}