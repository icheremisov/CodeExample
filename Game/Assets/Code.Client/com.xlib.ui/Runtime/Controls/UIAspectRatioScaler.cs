using UnityEngine;
using UnityEngine.UI;
using XLib.Unity.Utils;

namespace XLib.UI.Controls {

	[RequireComponent(typeof(CanvasScaler)), ExecuteInEditMode]
	public class UIAspectRatioScaler : MonoBehaviour {

		[Header("16:9"), SerializeField, Range(0, 1)] private float _matchWidthOrHeight169;
		[Header("16:10"), SerializeField, Range(0, 1)] private float _matchWidthOrHeight1610 = 0.5f;
		[Header("4:3"), SerializeField, Range(0, 1)] private float _matchWidthOrHeight43 = 1;

		private CanvasScaler _canvasScaler;

		private void Awake() {
			UpdateAspectRatio();
		}

#if UNITY_EDITOR
		private void Update() {
			UpdateAspectRatio();
		}
#endif

		private void UpdateAspectRatio() {
			if (!_canvasScaler) _canvasScaler = this.GetExistingComponent<CanvasScaler>();

			var aspectK = GfxUtils.AspectRatioK(GfxUtils.AspectRatio.Aspect_4_3, GfxUtils.AspectRatio.Aspect_16_10);

			if (aspectK < 0.5f)
				_canvasScaler.matchWidthOrHeight = _matchWidthOrHeight43;
			else if (aspectK >= 1.0f) {
				aspectK = GfxUtils.AspectRatioK(GfxUtils.AspectRatio.Aspect_16_10, GfxUtils.AspectRatio.Aspect_16_9);
				_canvasScaler.matchWidthOrHeight = aspectK < 0.5f ? _matchWidthOrHeight1610 : _matchWidthOrHeight169;
			}
			else
				_canvasScaler.matchWidthOrHeight = _matchWidthOrHeight1610;
		}

	}

}