using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Controls {

	[RequireComponent(typeof(RawImage))]
	public class UIRawImageScaler : MonoBehaviour {

		[SerializeField] private bool _scaleWidth = true;
		[SerializeField, ShowIf(nameof(_scaleWidth))] private float _textureWidth = 10f;

		[SerializeField] private bool _scaleHeight = true;
		[SerializeField, ShowIf(nameof(_scaleHeight))] private float _textureHeight = 10f;

		private RawImage _image;
		private float? _prevHeight;

		private float? _prevWidth;
		private RectTransform _tm;

		private RawImage Image => _image ? _image : _image = this.GetExistingComponent<RawImage>();
		private RectTransform Tm => _tm ? _tm : _tm = this.GetExistingComponent<RectTransform>();

		private void LateUpdate() {
			UpdatePosition();
		}

		private void OnEnable() {
			_prevWidth = null;
			UpdatePosition();
		}

		[Button]
		private void UpdatePosition() {
			if ((!_scaleWidth && !_scaleHeight) || !NeedUpdate()) return;

			var image = Image;

			var tm = Tm;

			var r = image.uvRect;
			if (_scaleWidth) {
				var width = tm.rect.size.x;
				_prevWidth = width;

				r.width = width / _textureWidth;
			}

			if (_scaleHeight) {
				var height = tm.rect.size.y;
				_prevHeight = height;

				r.height = height / _textureHeight;
			}

			image.uvRect = r;
		}

		private bool NeedUpdate() {
			var tm = Tm;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if ((_scaleWidth && _prevWidth == tm.sizeDelta.x && Application.isPlaying) || _textureWidth <= 0.0001f) return false;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if ((_scaleHeight && _prevHeight == tm.sizeDelta.y && Application.isPlaying) || _textureHeight <= 0.0001f) return false;

			return true;
		}

	}

}