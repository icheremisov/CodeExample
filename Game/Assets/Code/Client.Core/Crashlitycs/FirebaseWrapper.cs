using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Crashlytics;
using UnityEngine;

namespace Client.Core.Crashlitycs {

	public static class FirebaseWrapper {
		private static int _onApplicationResumeCount = 0;
		private static int _onApplicationPauseCount = 0;
		private static DateTime _lastResumeTime;
		private static DateTime _lastPauseTime;
		private static FirebaseApp _app;
		private static bool _isEnable = false;
		private static TaskCompletionSource<FirebaseApp> _taskCompletionSource;
		private static string _userId;
		public static bool IsReady => _isEnable && _app != null;

		public static void LogException(Exception exception) {
			if (!_isEnable) return;
			LogExceptionInternal(exception).Forget();
		}

		private static async Task LogExceptionInternal(Exception exception) {
			try {
				if (!IsReady && _taskCompletionSource != null) await _taskCompletionSource.Task;

				UpdateInternalCustomKeys(exception);
				Crashlytics.LogException(exception);
			}
			catch (Exception e) {
				Debug.LogWarning($"FirebaseWrapper caught exception while trying to log exception. {e.Message}");
			}
		}

		public static void OnApplicationPause(bool isPause) {
			if (!isPause) {
				_onApplicationResumeCount++;
				_lastResumeTime = DateTime.Now;
			}
			else {
				_onApplicationPauseCount++;
				_lastPauseTime = DateTime.Now;
			}
		}

		public static void OnLogCallback(string condition, string stacktrace, LogType type) {
			if (!IsReady) return;

			condition = (type is LogType.Warning or LogType.Log ? condition : $"{condition}\n{stacktrace ?? string.Empty}");
			if (condition.Length > 8192) {
				condition =
					$"{condition[..4096]}\n<SKIP>\n{condition.Substring(condition.Length - 4097, 4096)}";
			}

			Crashlytics.Log(condition);
		}

		public static async Task Init() {
			_isEnable = !Application.isEditor;
#if DEVELOPMENT_BUILD
			FirebaseApp.LogLevel = LogLevel.Debug;
#else
			FirebaseApp.LogLevel = LogLevel.Warning;
#endif
			_taskCompletionSource = new TaskCompletionSource<FirebaseApp>();
			var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

			if (dependencyStatus == DependencyStatus.Available) {
				// Create and hold a reference to your FirebaseApp,
				// where app is a Firebase.FirebaseApp property of your application class.
				// Crashlytics will use the DefaultInstance, as well;
				// this ensures that Crashlytics is initialized.
				_app = FirebaseApp.DefaultInstance;

				// When this property is set to true, Crashlytics will report all
				// uncaught exceptions as fatal events. This is the recommended behavior.
				Crashlytics.ReportUncaughtExceptionsAsFatal = false;

				Crashlytics.IsCrashlyticsCollectionEnabled = true;

				// Set a flag here for indicating that your project is ready to use Firebase.

				Debug.Log("[FirebaseRootInstaller] Initialize complete");
				_taskCompletionSource.SetResult(_app);
			}
			else {
				Debug.LogError($"[FirebaseRootInstaller] Could not resolve all Firebase dependencies: {dependencyStatus}");
				_taskCompletionSource.SetCanceled();
			}
		}

		private static void UpdateInternalCustomKeys(Exception exception = null) {
			if (!IsReady) return;

			if (_userId.IsNotNullOrEmpty()) {
				try {
					Crashlytics.SetUserId(_userId);
				}
				catch (Exception e) {
					Debug.LogWarning($"FirebaseWrapper couldn't set user id ({_userId}).\nException: {e.Message}");
				}
				SetCustomKey("ProfileId", _userId);
			}
			
			SetCustomKey("OnApplicationPause", _onApplicationPauseCount.ToString());
			SetCustomKey("OnApplicationResume", _onApplicationResumeCount.ToString());
			if (_lastPauseTime != default) SetCustomKey("LastPauseTime", (DateTime.Now - _lastPauseTime).ToString());
			if (_lastResumeTime != default) SetCustomKey("LastResumeTime", (DateTime.Now - _lastResumeTime).ToString());

			SetCustomKey("SystemLanguage", Application.systemLanguage.ToString());
			SetCustomKey("InternetType", Application.internetReachability.ToString());
			SetCustomKey("InGameTime", TimeSpan.FromSeconds(Time.time).ToString("g"));
			SetCustomKey("RealtimeSinceStartup", TimeSpan.FromSeconds(Time.realtimeSinceStartup).ToString("g"));
		}

		public static void SetConnectionInfo(string deviceId, string serverEnvironment) {
			if (_isEnable) return;

			SetCustomKey("DeviceId", deviceId);
			SetCustomKey("DeploymentType", serverEnvironment);
			UpdateInternalCustomKeys();
		}

		private static void SetCustomKey(string key, string value) {

			// 1k limit of value
			value = value.ClampWithEllipsis(1023);
			try {
				Crashlytics.SetCustomKey(key, value);
			}
			catch (Exception e) {
				Debug.LogWarning($"FirebaseWrapper couldn't set custom key (key={key}, value={value}).\nException: {e.Message}");
			}
		}

		public static void SetUserId(string userId) {
			_userId = userId;
			if (IsReady) UpdateInternalCustomKeys();
		}
	}

}