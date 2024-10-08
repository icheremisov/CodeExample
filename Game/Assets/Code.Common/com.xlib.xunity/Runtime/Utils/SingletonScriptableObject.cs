using System;
using UnityEngine;
using XLib.Core.Utils;
using Zenject;

namespace XLib.Unity.Utils {

	public class SingletonScriptableObject<T> : ScriptableObject, IInitializable, IDisposable where T : SingletonScriptableObject<T> {
		private static T _instance;

		private static T TryGetInstance() {
			if (_instance != null) return _instance;
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				_instance = EditorUtils.LoadSingleAsset<T>();
				return _instance;
			}
#endif

			throw new SystemException($"The {TypeOf<T>.Name} object has not yet been created or initialized.");
		}

		public static T Instance => _instance != null ? _instance : TryGetInstance();

		private void SetAsSingleton() {
			if (_instance != null) Debug.Assert(_instance == null, $"Instance '{name}' of {TypeOf<T>.Name} already exists '{_instance.name}'.");
			_instance = this as T;
		}

		public void Initialize() => SetAsSingleton();

		public void Dispose() {
			if (_instance == this) _instance = null;
		}

	}

}