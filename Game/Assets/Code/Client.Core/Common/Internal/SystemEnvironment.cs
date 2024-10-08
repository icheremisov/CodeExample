using DG.Tweening;
using UnityEngine;
using XLib.BuildSystem;

namespace Client.Core.Common.Internal {

	/// <summary>
	///     system-wide subscribers and etc. setup. Called once at game startup
	/// </summary>
	public static class SystemEnvironment {

		public static string PersistentDataPath { get; private set; }

		public static void Initialize() {
			VersionService.Initialize();
			PersistentDataPath = Application.persistentDataPath;

			DOTween.onWillLog += OnWillDoTweenLog;
			DOTween.SetTweensCapacity(512, 256);
			
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
			Debug.LogWarning("DEVELOPMENT_BUILD=ON");
#elif !DEVELOPMENT_BUILD && !UNITY_EDITOR && !FEATURE_CONSOLE
			Debug.unityLogger.filterLogType = LogType.Warning;
#endif
		}

		/// <summary>
		///     Intercept DOTween's logs
		/// </summary>
		/// <param name="logType"></param>
		/// <param name="message"></param>
		/// <returns>Return TRUE if you want DOTween to proceed with the log, FALSE otherwise.</returns>
		private static bool OnWillDoTweenLog(LogType logType, object message) {
			if (logType == LogType.Warning) {
				Debug.LogError(message);
				return false;
			}

			return true;
		}

	}

}