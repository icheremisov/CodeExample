#if UNITY_EDITOR
using UnityEditor;

namespace XLib.Unity.Utils {

	[InitializeOnLoad]
	public static class EditorCycle {
		public static bool IsPlaying;

		static EditorCycle() {
			EditorApplication.playModeStateChanged += _ => IsPlaying = EditorApplication.isPlaying;
		}
	}

}
#endif