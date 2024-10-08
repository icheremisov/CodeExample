using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Unity.Scene {

	[ExecuteInEditMode]
	public class EditorPreview : MonoBehaviour {

		private void Awake() {
			if (Application.isPlaying) Destroy(gameObject);
		}

#if UNITY_EDITOR
		[SerializeField, AssetsOnly, OnValueChanged(nameof(UpdateModel))] private GameObject _prefab;

		private GameObject _instancePrefab;
		private GameObject _instance;

		public void OnEnable() {
			UpdateModel();
		}

		public void OnDisable() {
			Clear();
		}

		private void UpdateModel() {
			if (_instance != null && _prefab != null)
				if (_instancePrefab == _prefab)
					return;

			Clear();

			_instancePrefab = _prefab;

			if (_prefab != null) {
				_instance = Instantiate(_prefab, transform);
				_instance.hideFlags = HideFlags.HideAndDontSave;
				_instance.ResetPRS();
			}
		}

		private void Clear() {
			if (_instance != null) {
				DestroyImmediate(_instance);
				_instance = null;
				_instancePrefab = null;
			}
		}
#endif

	}

}