using System;
using UnityEngine;

namespace XLib.Unity.Scene {

	public abstract class DisableAbleSingleton<T> : MonoBehaviour where T : MonoBehaviour {
		[SerializeField] private bool _disableAble = true;

		private static Action<T> _onAfterCreated;

		public static T S { get; private set; }

		protected virtual void Awake() {
			Debug.Assert(S == null, $"Instance of {typeof(T).FullName} already exists in scene. Instance path: '{S.GetFullPath()}'");
			S = this as T;

			_onAfterCreated?.Invoke(S);
			_onAfterCreated = null;
		}

		protected virtual void OnDestroy() {
			if (S == this) S = null;
		}

		protected virtual void OnEnable() {
			if (!_disableAble || S == this) return;

			Debug.Assert(S == null, $"Instance of {typeof(T).FullName} already exists in scene. Instance path: '{S.GetFullPath()}'");
			S = this as T;
		}

		protected virtual void OnDisable() {
			if (_disableAble && S == this) S = null;
		}

		public void UpdateVisible(bool visible) {
			if (visible)
				OnEnable();
			else
				OnDisable();
		}

		public static void OnCreated(Action<T> callback) {
			if (callback == null) return;

			if (S != null)
				callback(S);
			else
				_onAfterCreated += callback;
		}
	}

}