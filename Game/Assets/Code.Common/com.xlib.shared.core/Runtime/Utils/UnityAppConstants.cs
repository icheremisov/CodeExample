using System.Diagnostics.CodeAnalysis;

#if UNITY3D
using UnityEngine;
#endif

namespace XLib.Core.Utils {

	/// <summary>
	/// UnityEngine.Application constants for access from another threads
	/// </summary>
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public static class UnityAppConstants {
#if !UNITY3D
		public static bool isEditor => false;
		public static bool isPlaying => true;
		public static string persistentDataPath => ".";
		public static string dataPath => persistentDataPath;

#else
		
#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		public static void InitializeEditor() {
			UnityEditor.EditorApplication.playModeStateChanged += _ => Initialize();
			Initialize();
		}
#endif		

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		public static void Initialize() {
			
			isEditor = Application.isEditor;
			isPlaying = Application.isPlaying;
			persistentDataPath = Application.persistentDataPath;
			dataPath = Application.dataPath;
		}

		public static bool isEditor { get; private set; }
		public static bool isPlaying { get; private set; }
		public static string persistentDataPath { get; private set; }
		public static string dataPath { get; private set; }

#endif
	}

}