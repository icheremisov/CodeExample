#if UNITY_EDITOR

using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using XLib.Core.Utils;


namespace XLib.UI.Procedural {

	public partial class UIGradient {
		[HorizontalGroup("Rotate")]
		[Button]
		private void RotateCCW() {
			
			Undo.RecordObject(this, "Change Gradient");
			
			var t = _topLeftColor;
			_topLeftColor = _topRightColor;
			_topRightColor = _botRightColor;
			_botRightColor = _botLeftColor;
			_botLeftColor = t;

			graphic.SetVerticesDirty();
		}

		[HorizontalGroup("Rotate")]
		[Button]
		private void RotateCW() {
			Undo.RecordObject(this, "Change Gradient");
			
			var t = _topLeftColor;
			_topLeftColor = _botLeftColor;
			_botLeftColor = _botRightColor;
			_botRightColor = _topRightColor;
			_topRightColor = t;

			graphic.SetVerticesDirty();
		}

		[HorizontalGroup("Color")]
		[Button]
		private void TopToDown() {
			Undo.RecordObject(this, "Change Gradient");
			
			_botRightColor = Color.black;
			_botLeftColor = Color.black;
			_topRightColor = Color.white;
			_topLeftColor = Color.white;
			graphic.SetVerticesDirty();
		}

		[HorizontalGroup("Color")]
		[Button]
		private void RightToLeft() {
			Undo.RecordObject(this, "Change Gradient");
			
			_botRightColor = Color.white;
			_botLeftColor = Color.black;
			_topRightColor = Color.white;
			_topLeftColor = Color.black;
			graphic.SetVerticesDirty();
		}

		[HorizontalGroup("Alpha")]
		[Button]
		private void TopToDownAlpha() {
			Undo.RecordObject(this, "Change Gradient");
			
			_botRightColor = Color.white.SetAlpha(0);
			_botLeftColor = Color.white.SetAlpha(0);
			_topRightColor = Color.white;
			_topLeftColor = Color.white;
			graphic.SetVerticesDirty();
		}

		[HorizontalGroup("Alpha")]
		[Button]
		private void RightToLeftAlpha() {
			Undo.RecordObject(this, "Change Gradient");
			
			_botRightColor = Color.white;
			_botLeftColor = Color.white.SetAlpha(0);
			_topRightColor = Color.white;
			_topLeftColor = Color.white.SetAlpha(0);
			graphic.SetVerticesDirty();
		}

		[HorizontalGroup("Mirror")]
		[Button]
		private void FlipVertical() {
			Undo.RecordObject(this, "Change Gradient");
			
			MathEx.Swap(ref _topRightColor, ref _botRightColor);
			MathEx.Swap(ref _topLeftColor, ref _botLeftColor);

			graphic.SetVerticesDirty();
		}

		[HorizontalGroup("Mirror")]
		[Button]
		private void FlipHorizontal() {
			Undo.RecordObject(this, "Change Gradient");
			
			MathEx.Swap(ref _topRightColor, ref _topLeftColor);
			MathEx.Swap(ref _botRightColor, ref _botLeftColor);

			graphic.SetVerticesDirty();
		}
	}

}

#endif