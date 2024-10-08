using System;
using UnityEngine;

namespace XLib.Unity.Scene {

	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	public abstract class Singleton<T> : MonoBehaviour
		where T : MonoBehaviour {

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

		public static void OnCreated(Action<T> callback) {
			if (callback == null) return;

			if (S != null)
				callback(S);
			else
				_onAfterCreated += callback;
		}

	}

}