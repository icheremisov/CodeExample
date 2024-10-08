using TMPro;
using UnityEngine;

namespace XLib.UI.Utils {

	public static class RectTransformExtensions {
		public static void SetTextWithResize(this TMP_Text obj, string text, float? maxWidth = null) {
			obj.SetText(text);
			if (maxWidth.HasValue) {
				obj.rectTransform.sizeDelta = new Vector2(maxWidth.Value, 0);
				obj.enableWordWrapping = true;
			}

			obj.ForceMeshUpdate();
			var size = obj.GetRenderedValues(false);

			if (maxWidth.HasValue) {
				size = new Vector2(Mathf.Min(size.x, maxWidth.Value), size.y);
			}

			obj.rectTransform.sizeDelta = size;
		}
		
		/// <summary> Makes the RectTransform match its parent size </summary>
		/// <param name="target"> Target RectTransform </param>
		/// <param name="resetScaleToOne"> Reset LocalScale to Vector3.one </param>
		public static void Dock(this RectTransform target, bool resetScaleToOne) {
			if (resetScaleToOne) target.ResetLocalScaleToOne();
			target.AnchorMinToZero();
			target.AnchorMaxToOne();
			target.CenterPivot();
			target.SizeDeltaToZero();
			target.ResetAnchoredPosition3D();
			target.ResetLocalPosition();
		}

		/// <summary> Moves the RectTransform pivot settings to its center </summary>
		/// <param name="target"> Target RectTransform </param>
		/// <param name="resetScaleToOne"> Reset LocalScale to Vector3.one </param>
		public static void Center(this RectTransform target, bool resetScaleToOne) {
			if (resetScaleToOne) target.ResetLocalScaleToOne();
			target.AnchorMinToCenter();
			target.AnchorMaxToCenter();
			target.CenterPivot();
			target.SizeDeltaToZero();
		}

		/// <summary> Resets the target's anchoredPosition3D to Vector3.zero </summary>
		/// <param name="target"> Target RectTransform </param>
		public static void ResetAnchoredPosition3D(this RectTransform target) {
			target.anchoredPosition3D = Vector3.zero;
		}

		/// <summary> Resets the target's localPosition to Vector3.zero </summary>
		/// <param name="target"> Target RectTransform </param>
		public static void ResetLocalPosition(this RectTransform target) {
			target.localPosition = Vector3.zero;
		}

		/// <summary> Resets the target's localScale to Vector3.one </summary>
		/// <param name="target"> Target RectTransform </param>
		public static void ResetLocalScaleToOne(this RectTransform target) {
			target.localScale = Vector3.one;
		}

		/// <summary> Resets the target's anchorMin to Vector2.zero </summary>
		/// <param name="target"> Target RectTransform </param>
		public static void AnchorMinToZero(this RectTransform target) {
			target.anchorMin = Vector2.zero;
		}

		/// <summary> Sets the target's anchorMin to Vector2(0.5f, 0.5f) </summary>
		/// <param name="target"> Target RectTransform </param>
		public static void AnchorMinToCenter(this RectTransform target) {
			target.anchorMin = new Vector2(0.5f, 0.5f);
		}

		/// <summary> Resets the target's anchorMax to Vector2.one </summary>
		/// <param name="target"> Target RectTransform </param>
		public static void AnchorMaxToOne(this RectTransform target) {
			target.anchorMax = Vector2.one;
		}

		/// <summary> Sets the target's anchorMax to Vector2(0.5f, 0.5f) </summary>
		/// <param name="target"> Target RectTransform </param>
		public static void AnchorMaxToCenter(this RectTransform target) {
			target.anchorMax = new Vector2(0.5f, 0.5f);
		}

		/// <summary> Sets the target's pivot to Vector2(0.5f, 0.5f) </summary>
		/// <param name="target"> Target RectTransform </param>
		public static void CenterPivot(this RectTransform target) {
			target.pivot = new Vector2(0.5f, 0.5f);
		}

		/// <summary> Resets the target's sizeDelta to Vector2.zero </summary>
		/// <param name="target"> Target RectTransform </param>
		public static void SizeDeltaToZero(this RectTransform target) {
			target.sizeDelta = Vector2.zero;
		}
		
	}

}