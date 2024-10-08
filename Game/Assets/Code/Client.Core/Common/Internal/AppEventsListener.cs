using System;
using Client.Core.Common.Contracts;
using UnityEngine;
using XLib.Core.Utils;

namespace Client.Core.Common.Internal {

	public class AppEventsListener : MonoBehaviour, IAppEventsListener {

		protected void Awake() {
			DontDestroyOnLoad(gameObject);
		}

		private void OnApplicationPause(bool pauseStatus) {
			ApplicationPause?.Invoke(pauseStatus);
			ApplicationPauseStatic?.Invoke(pauseStatus);
		}

		private void OnApplicationQuit() {
			ApplicationQuit?.Invoke();
			ApplicationQuitStatic?.Invoke();
		}

		public event Action<bool> ApplicationPause;
		public event Action ApplicationQuit;

		public static event Action<bool> ApplicationPauseStatic;
		public static event Action ApplicationQuitStatic;

		internal static AppEventsListener Instantiate() {
			var appEvents = new GameObject("AppEventsListener", TypeOf<AppEventsListener>.Raw);
			DontDestroyOnLoad(appEvents);
			return appEvents.GetExistingComponent<AppEventsListener>();
		}

	}

}