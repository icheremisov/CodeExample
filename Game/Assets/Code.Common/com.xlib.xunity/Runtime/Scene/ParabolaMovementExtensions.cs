using DG.Tweening;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.Unity.Scene {

	public static class ParabolaMovementExtensions {

		public static Tween DOParabola2DMove(this GameObject obj, Vector2 startPos, Vector2 endPos, float height, float duration, float? middleScale = null) =>
			DOParabola2DMove(obj.transform, startPos, endPos, height, duration, middleScale);

		public static Tween DOParabola2DMove(this Transform obj, Vector2 startPos, Vector2 endPos, float height, float duration, float? middleScale = null) {
			var transform = obj.transform;
			transform.position = startPos;

			var s = transform.localScale;

			return DOVirtual.Float(0, 1, duration, t => {
				if (!transform) return;

				transform.position = MathEx.Parabola2D(startPos, endPos, height, t);
				if (middleScale.HasValue) {
					var k = Mathf.Lerp(1, middleScale.Value, MathEx.Parabola1D(height, t) / height);
					transform.localScale = s * k;
				}
			});
		}

	}

}