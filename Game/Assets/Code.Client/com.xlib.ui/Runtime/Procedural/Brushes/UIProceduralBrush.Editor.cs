#if UNITY_EDITOR

using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using XLib.Core.Utils;

namespace XLib.UI.Procedural.Brushes {

	public partial class UIProceduralBrush {
		[ShowIf(nameof(CornersGradientBrush))]
		[HorizontalGroup("Rotate")]
		[Button]
		private void RotateCCW() {
			Undo.RecordObject(Selection.activeObject, "Change Gradient");

			var t = _leftTop;
			_leftTop = _rightTop;
			_rightTop = _rightBottom;
			_rightBottom = _leftBottom;
			_leftBottom = t;

			UpdateSelection();
		}

		[ShowIf(nameof(CornersGradientBrush))]
		[HorizontalGroup("Rotate")]
		[Button]
		private void RotateCW() {
			Undo.RecordObject(Selection.activeObject, "Change Gradient");

			var t = _leftTop;
			_leftTop = _leftBottom;
			_leftBottom = _rightBottom;
			_rightBottom = _rightTop;
			_rightTop = t;

			UpdateSelection();
		}

		[ShowIf(nameof(CornersGradientBrush))]
		[HorizontalGroup("Color")]
		[Button]
		private void TopToDown() {
			Undo.RecordObject(Selection.activeObject, "Change Gradient");

			_rightBottom = Color.black;
			_leftBottom = Color.black;
			_rightTop = Color.white;
			_leftTop = Color.white;

			UpdateSelection();
		}

		[ShowIf(nameof(CornersGradientBrush))]
		[HorizontalGroup("Set Color")]
		[Button]
		private void ColorWhite() {
			Undo.RecordObject(Selection.activeObject, "Change Gradient");

			_rightBottom = Color.white;
			_leftBottom = Color.white;
			_rightTop = Color.white;
			_leftTop = Color.white;

			UpdateSelection();
		}

		[ShowIf(nameof(CornersGradientBrush))]
		[HorizontalGroup("Set Color")]
		[Button]
		private void ColorBlack() {
			Undo.RecordObject(Selection.activeObject, "Change Gradient");

			_rightBottom = Color.black;
			_leftBottom = Color.black;
			_rightTop = Color.black;
			_leftTop = Color.black;

			UpdateSelection();
		}

		[ShowIf(nameof(CornersGradientBrush))]
		[HorizontalGroup("Color")]
		[Button]
		private void RightToLeft() {
			Undo.RecordObject(Selection.activeObject, "Change Gradient");

			_rightBottom = Color.white;
			_leftBottom = Color.black;
			_rightTop = Color.white;
			_leftTop = Color.black;

			UpdateSelection();
		}

		[ShowIf(nameof(CornersGradientBrush))]
		[HorizontalGroup("Alpha")]
		[Button]
		private void TopToDownAlpha() {
			Undo.RecordObject(Selection.activeObject, "Change Gradient");

			_rightBottom = _rightBottom.SetAlpha(0);
			_leftBottom = _leftBottom.SetAlpha(0);
			_rightTop = _rightTop.SetAlpha(1);
			_leftTop = _leftTop.SetAlpha(1);

			UpdateSelection();
		}

		[ShowIf(nameof(CornersGradientBrush))]
		[HorizontalGroup("Alpha")]
		[Button]
		private void RightToLeftAlpha() {
			Undo.RecordObject(Selection.activeObject, "Change Gradient");

			_rightBottom = _rightBottom.SetAlpha(1);
			_leftBottom = _leftBottom.SetAlpha(0);
			_rightTop = _rightTop.SetAlpha(1);
			_leftTop = _leftTop.SetAlpha(0);

			UpdateSelection();
		}

		[ShowIf(nameof(CornersGradientBrush))]
		[HorizontalGroup("Mirror")]
		[Button]
		private void FlipVertical() {
			Undo.RecordObject(Selection.activeObject, "Change Gradient");

			MathEx.Swap(ref _rightTop, ref _rightBottom);
			MathEx.Swap(ref _leftTop, ref _leftBottom);

			UpdateSelection();
		}

		[ShowIf(nameof(CornersGradientBrush))]
		[HorizontalGroup("Mirror")]
		[Button]
		private void FlipHorizontal() {
			Undo.RecordObject(Selection.activeObject, "Change Gradient");

			MathEx.Swap(ref _rightTop, ref _leftTop);
			MathEx.Swap(ref _rightBottom, ref _leftBottom);

			UpdateSelection();
		}

		private void UpdateSelection() {
			var go = Selection.activeObject as GameObject;
			if (!go) return;

			var g = go.GetComponent<Graphic>();
			if (!g) return;
			
			g.SetVerticesDirty();
		}
	}

}

#endif