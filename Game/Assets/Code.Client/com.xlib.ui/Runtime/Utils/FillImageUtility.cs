using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using XLib.UI.Procedural;

namespace XLib.UI.Utils {

	[Flags]
	public enum AnimDirection {

		None = 0,

		Forward = 1 << 0,
		Back = 1 << 1,

		Both = Forward | Back,

		Default = Forward

	}

	public static class FillImageUtility {

		private const float DefaultDuration = 0.3f;
		private const Ease DefaultEase = Ease.Linear;

		public static Tween FillProgress(float value, RectTransform filled, RectTransform root, AnimDirection animDir = AnimDirection.Default,
			float duration = DefaultDuration, Ease ease = DefaultEase, float minWidth = 0) {
			value = Mathf.Clamp01(value);

			var srcSizeDelta = filled.sizeDelta;

			var endValue = value <= 0.001f ? new Vector2(0, srcSizeDelta.y) : new Vector2(Mathf.Max(minWidth, root.rect.width * value), srcSizeDelta.y);
			if (Mathf.Abs(srcSizeDelta.x - endValue.x) < 2f) {
				filled.sizeDelta = endValue;
				return null;
			}

			var needAnimation = duration > float.Epsilon;
			if (needAnimation) {
				var isForward = endValue.x > srcSizeDelta.x;
				needAnimation = isForward ? animDir.HasFlag(AnimDirection.Forward) : animDir.HasFlag(AnimDirection.Back);
			}

			filled.DOKill();

			if (needAnimation) return filled.DOSizeDelta(endValue, duration).SetEase(ease).SetUpdate(false);
			filled.sizeDelta = endValue;
			return null;
		}

		public static Tween FillProgress(float value, Image filled, AnimDirection animDir = AnimDirection.Default,
			float duration = DefaultDuration, Ease ease = DefaultEase) {
			value = Mathf.Clamp01(value);

			if (Mathf.Abs(filled.fillAmount - value) < 0.01f) return null;

			var needAnimation = duration > float.Epsilon;
			if (needAnimation) {
				var isForward = value > filled.fillAmount;
				needAnimation = isForward ? animDir.HasFlag(AnimDirection.Forward) : animDir.HasFlag(AnimDirection.Back);
			}

			filled.DOKill();

			if (needAnimation) return filled.DOFillAmount(value, duration).SetEase(ease).SetUpdate(false);
			filled.fillAmount = value;
			return null;
		}
		
		public static Tween FillProgress(float value, UICircle filled, AnimDirection animDir = AnimDirection.Default,
			float duration = DefaultDuration, Ease ease = DefaultEase) {
			value = Mathf.Clamp01(value);

			if (Mathf.Abs(filled.FillAmount - value) < 0.01f) return null;

			var needAnimation = duration > float.Epsilon;
			if (needAnimation) {
				var isForward = value > filled.FillAmount;
				needAnimation = isForward ? animDir.HasFlag(AnimDirection.Forward) : animDir.HasFlag(AnimDirection.Back);
			}

			filled.DOKill();

			if (needAnimation) return filled.FillWithAnimation(value, duration, ease);
			filled.FillAmount = value;
			return null;
		}

	}

}