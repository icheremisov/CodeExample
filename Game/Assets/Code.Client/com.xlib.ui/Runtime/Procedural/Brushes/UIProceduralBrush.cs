using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.UI.Procedural.Brushes {

	[Serializable]
	public partial class UIProceduralBrush {
		private enum BrushType {
			Color,
			GradientVertical,
			GradientHorizontal,
			GradientCorners,
		}

		[SerializeField] private BrushType _type = BrushType.Color;

		[SerializeField, ShowIf(nameof(ColorBrush))] private Color _color = Color.white;

		[SerializeField, ShowIf(nameof(GradientBrush))]
		private Gradient _gradient = new Gradient();

		[HorizontalGroup("Top"), HideLabel]
		[SerializeField, ShowIf(nameof(CornersGradientBrush))] private Color _leftTop = Color.white;
		[HorizontalGroup("Top"), HideLabel]
		[SerializeField, ShowIf(nameof(CornersGradientBrush))] private Color _rightTop = Color.white;
		[HorizontalGroup("Bottom"), HideLabel]
		[SerializeField, ShowIf(nameof(CornersGradientBrush))] private Color _leftBottom = Color.black;
		[HorizontalGroup("Bottom"), HideLabel]
		[SerializeField, ShowIf(nameof(CornersGradientBrush))] private Color _rightBottom = Color.black;

		private bool ColorBrush => _type is BrushType.Color;
		private bool GradientBrush => _type is BrushType.GradientVertical or BrushType.GradientHorizontal;
		private bool CornersGradientBrush => _type is BrushType.GradientCorners;
		
		public Color Get(Vector2 vertex, Rect rect) {
			return _type switch {
				BrushType.Color              => _color,
				BrushType.GradientVertical   => GetVertical(vertex, rect),
				BrushType.GradientHorizontal => GetHorizontal(vertex, rect),
				BrushType.GradientCorners    => GetCorners(vertex, rect),
				_                            => throw new ArgumentOutOfRangeException()
			};
		}

		private Color GetHorizontal(Vector2 vertex, Rect rect) {
			var size = rect.size.x;
			if (size <= 0) return _gradient.Evaluate(0);

			return _gradient.Evaluate(Mathf.Clamp01((vertex.x - rect.x) / size));
		}

		private Color GetVertical(Vector2 vertex, Rect rect) {
			var size = rect.size.y;
			if (size <= 0) return _gradient.Evaluate(0);

			return _gradient.Evaluate(1.0f - Mathf.Clamp01((vertex.y - rect.y) / size));
		}

		private Color GetCorners(Vector2 vertex, Rect rect) {
			var size = rect.size;
			if (size.x <= 0 || size.y <= 0) return _leftTop;

			var u = Mathf.Clamp01((vertex.x - rect.x) / size.x);
			var v = Mathf.Clamp01((size.y - (vertex.y - rect.y)) / size.y);

			var top = Color.Lerp(_leftTop, _rightTop, u);
			var bottom = Color.Lerp(_leftBottom, _rightBottom, u);
			return Color.Lerp(top, bottom, v);
		}
	}

}