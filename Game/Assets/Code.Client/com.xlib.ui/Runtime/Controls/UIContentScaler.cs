using UnityEngine;

namespace XLib.UI.Controls {

	public class UIContentScaler : MonoBehaviour {

		[SerializeField] private Transform _scalingTransform;

		[Space, SerializeField] private float _referenceSizeWidth = 1125;
		[SerializeField] private float _referenceSizeHeight = 2436;

		private bool _isEnabled;
		private Canvas _rootCanvas;

		private Transform ScalingTransform => _scalingTransform != null ? _scalingTransform : _scalingTransform = transform;
		private Canvas RootCanvas => _rootCanvas != null ? _rootCanvas : _rootCanvas = ScalingTransform.GetTopmostCanvas();

		private void LateUpdate() {
			if (_isEnabled) SetCurrentScale();
		}

		private void OnEnable() {
			_isEnabled = true;
			SetCurrentScale();
		}

		private void OnDisable() {
			_isEnabled = false;
			_rootCanvas = null;
		}

		private void SetCurrentScale() {
			var rootCanvas = RootCanvas;

			Debug.Assert(rootCanvas != null, "Root canvas wasn't found!");

			var rootRT = (RectTransform)rootCanvas.transform;
			var sizeDelta = rootRT.sizeDelta;

			var sW = sizeDelta.x / _referenceSizeWidth;
			var sH = sizeDelta.y / _referenceSizeHeight;

			ScalingTransform.localScale = new Vector3(sW, sH, 1);
		}

	}

}