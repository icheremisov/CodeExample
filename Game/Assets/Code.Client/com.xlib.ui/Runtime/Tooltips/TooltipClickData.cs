using UnityEngine;

namespace XLib.UI.Tooltips {

	public class TooltipClickData {
		public GameObject Target { get; }
		private readonly RectTransform _rectTransform;

		private readonly Camera _worldCam;
		private readonly Transform _worldTransform;
		private readonly Camera _screenCam;

		public TooltipClickData(RectTransform rectTransform) {
			_rectTransform = rectTransform;
			Target = _rectTransform.gameObject;
		}

		public TooltipClickData(Transform worldTransform, Camera screenCam, Camera worldCam) {
			_worldCam = worldCam;
			_worldTransform = worldTransform;
			_screenCam = screenCam;
			Target = _worldTransform.gameObject;
		}

		public Rect GetTransformedRect(RectTransform tooltipParentRT) {
			if (_rectTransform == null) {
				var screenPos = RectTransformUtility.WorldToScreenPoint(_worldCam, _worldTransform.position);
				RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipParentRT, screenPos, _screenCam, out var localPoint);
				return new Rect(localPoint, Vector2.zero);
			}

			var targetRect = _rectTransform.rect;
			var pos = tooltipParentRT.InverseTransformPoint(_rectTransform.TransformPoint(targetRect.position));
			return new Rect(pos, targetRect.size);
		}
	}

}