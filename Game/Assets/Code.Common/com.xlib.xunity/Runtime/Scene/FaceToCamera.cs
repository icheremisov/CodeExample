using UnityEngine;

namespace XLib.Unity.Scene {

	public class FaceToCamera : MonoBehaviour {

		[SerializeField] private Camera _camera;

		private void LateUpdate() {
			if (_camera == null) _camera = Camera.main;

			if (_camera == null) return;

			transform.rotation = _camera.transform.rotation;
		}

	}

}