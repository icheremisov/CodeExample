using System;
using System.Diagnostics.CodeAnalysis;
using DG.Tweening;
using RectEx;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using XLib.UI.Procedural.Brushes;

namespace XLib.UI.Procedural {

	public class UICircle : UIProceduralEffect {
		public enum FillOrigin {
			Top,
			Right,
			Bottom,
			Left,
		}

		[SerializeField] private int _segments = 64;
		[SerializeField] private bool _preserveAspect = false;
		[SerializeField] private float _softness = 1;
		[SerializeField] private float _sizePadding = 0;

		[Space]
		[SerializeField] private bool _sector = false;
		[SerializeField, ShowIf(nameof(_sector))] private FillOrigin _fillOrigin = FillOrigin.Top;
		[SerializeField, ShowIf(nameof(_sector)), Range(0, 1)] private float _fillAmount = 1;
		[SerializeField, ShowIf(nameof(_sector))] private bool _clockwise;

		[Space]
		[SerializeField] private bool _fill = true;
		[BoxGroup("Color")]
		[SerializeField, ShowIf(nameof(_fill)), InlineProperty, HideLabel] private UIProceduralBrush _color;

		[Space]
		[SerializeField] private bool _border = false;
		[SerializeField, ShowIf(nameof(_border))] private float _borderSize = 3.0f;
		[BoxGroup("Border Color")]
		[SerializeField, ShowIf(nameof(_border)), InlineProperty, HideLabel] private UIProceduralBrush _borderColor;

		private Tween _animation;

		public bool FilledSector {
			get => _sector;
			set => _sector = value;
		}

		public float FillAmount {
			get => _fillAmount;
			set {
				if (Math.Abs(_fillAmount - value) < 0.00001f) return;
				_animation?.Kill();
				_fillAmount = value;
				graphic.SetVerticesDirty();
			}
		}
		public Color ImageColor {
			get => ((Image)graphic).color;
			set => ((Image)graphic).color = value;
		}

		public Tween FillWithAnimation(float value, float duration, Ease ease) {
			value = Mathf.Clamp01(value);
			_animation?.Kill();
			
			if (Math.Abs(_fillAmount - value) < 0.00001f) return null;
			
			_animation = DOVirtual.Float(_fillAmount, value, duration, v => {
				_fillAmount = v;
				graphic.SetVerticesDirty();
			}).SetEase(ease).SetUpdate(false);
			return _animation;
		}
		
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		private class DrawContext {
			public Vector2 center;
			public Rect rect;
			public bool softness;
			public bool sector;
			public float angleStart;
			public float arcLen;

			public bool centerVertex;

			public bool soft;

			public Vector2 axisX;
			public Vector2 axisY;
			public Vector2 axisX1;
			public Vector2 axisY1;
			public Vector2 axisX2;
			public Vector2 axisY2;
			public Vector2 axisX3;
			public Vector2 axisY3;
		}

		private DrawContext _ctx;

		protected override void OnDisable() {
			_animation?.Kill(true);
			
			base.OnDisable();
		}

		protected override void GenerateGraphics() {
			if (_segments <= 2 || _segments > 1024) return;
			if (_sector && _fillAmount <= 0.0001f) return;

			var owner = this.graphic;
			var rect = owner.rectTransform.rect;
			if (_sizePadding != 0) rect = rect.Extend(_sizePadding); 

			var sizeX = rect.size.x * 0.5f;
			var sizeY = rect.size.y * 0.5f;

			if (_preserveAspect && Math.Abs(sizeX - sizeY) > 0.01f) sizeX = sizeY = Mathf.Min(sizeX, sizeY);

			_ctx ??= new DrawContext();
			_ctx.rect = rect;
			_ctx.center = rect.center;
			_ctx.angleStart = _fillOrigin switch {
				FillOrigin.Top    => Mathf.PI * 0.5f,
				FillOrigin.Right  => Mathf.PI,
				FillOrigin.Bottom => Mathf.PI * 1.5f,
				FillOrigin.Left   => 0,
				_                 => throw new ArgumentOutOfRangeException()
			};

			if (_sector && _fillAmount < 1) {
				_ctx.sector = true;
				_ctx.arcLen = Mathf.PI * 2 * _fillAmount;
			}
			else {
				_ctx.sector = false;
				_ctx.arcLen = Mathf.PI * 2;
			}

			_ctx.soft = _softness >= 0.01f;

			if (_border && _fill) {
				Build_Fill(_ctx, sizeX - _borderSize + _softness, sizeY - _borderSize + _softness);
				GenerateCircle(_ctx, Vertex_Fill, Poly_Fill);
				Build_Border(_ctx, sizeX, sizeY);
				GenerateCircle(_ctx, Vertex_Border, Poly_Border);
			}
			else if (_border) {
				Build_Border(_ctx, sizeX, sizeY);
				GenerateCircle(_ctx, Vertex_Border, Poly_Border);
			}
			else if (_fill) {
				Build_Fill(_ctx, sizeX, sizeY);
				GenerateCircle(_ctx, Vertex_Fill, Poly_Fill);
			}
		}

		private void Build_Fill(DrawContext data, float sx, float sy) {
			if (data.soft) {
				data.axisX1 = new Vector2(sx - _softness, 0);
				data.axisY1 = new Vector2(0, sy - _softness);
			}

			data.centerVertex = true;
			data.axisX = new Vector2(sx, 0);
			data.axisY = new Vector2(0, sy);
		}

		private void Vertex_Fill(DrawContext data, float x, float y) {
			if (data.soft) {
				_vertices.Add(Vertex(data.center + x * data.axisX1 + y * data.axisY1, _color, data.rect));
				_vertices.Add(Transparent(data.center + x * data.axisX + y * data.axisY, _color, data.rect));
			}
			else {
				_vertices.Add(Vertex(data.center + x * data.axisX + y * data.axisY, _color, data.rect));
			}
		}

		private void Poly_Fill(DrawContext data, int polygonSteps, int vOffset) {
			Action<int, int, int> addSegment;
			int offsetDelta;
			if (data.soft) {
				addSegment = _clockwise ? AddSegment2_AB : AddSegment2_BA;
				offsetDelta = 2;
			}
			else {
				addSegment = _clockwise ? AddSegment1_AB : AddSegment1_BA;
				offsetDelta = 1;
			}

			var offset = 1;
			for (var i = 0; i < polygonSteps; i++) {
				addSegment(vOffset, offset, offset + offsetDelta);
				offset += offsetDelta;
			}

			if (!data.sector) addSegment(vOffset, offset, 1);
		}

		private void Build_Border(DrawContext data, float sx, float sy) {
			if (data.soft) {
				data.axisX1 = new Vector2(sx - _softness, 0);
				data.axisY1 = new Vector2(0, sy - _softness);

				data.axisX2 = new Vector2(sx - _borderSize + _softness, 0);
				data.axisY2 = new Vector2(0, sy - _borderSize + _softness);

				data.axisX3 = new Vector2(sx - _borderSize, 0);
				data.axisY3 = new Vector2(0, sy - _borderSize);
			}
			else {
				data.axisX1 = new Vector2(sx - _borderSize, 0);
				data.axisY1 = new Vector2(0, sy - _borderSize);
			}

			data.centerVertex = false;
			data.axisX = new Vector2(sx, 0);
			data.axisY = new Vector2(0, sy);
		}

		private void Vertex_Border(DrawContext data, float x, float y) {
			if (data.soft) {
				_vertices.Add(Transparent(data.center + x * data.axisX3 + y * data.axisY3, _borderColor, data.rect));
				_vertices.Add(Vertex(data.center + x * data.axisX2 + y * data.axisY2, _borderColor, data.rect));
				_vertices.Add(Vertex(data.center + x * data.axisX1 + y * data.axisY1, _borderColor, data.rect));
				_vertices.Add(Transparent(data.center + x * data.axisX + y * data.axisY, _borderColor, data.rect));
			}
			else {
				_vertices.Add(Vertex(data.center + x * data.axisX1 + y * data.axisY1, _borderColor, data.rect));
				_vertices.Add(Vertex(data.center + x * data.axisX + y * data.axisY, _borderColor, data.rect));
			}
		}

		private void Poly_Border(DrawContext data, int polygonSteps, int vOffset) {
			Action<int, int, int> addSegment;
			int offsetDelta;
			if (data.soft) {
				addSegment = _clockwise ? AddSegment3Quads_AB : AddSegment3Quads_BA;
				offsetDelta = 4;
			}
			else {
				addSegment = _clockwise ? AddQuad_AB : AddQuad_BA;
				offsetDelta = 2;
			}

			var offset = 0;
			for (var i = 0; i < polygonSteps; i++) {
				addSegment(vOffset, offset, offset + offsetDelta);
				offset += offsetDelta;
			}

			if (!data.sector) addSegment(vOffset, offset, 0);
		}

		private void GenerateCircle(DrawContext data, Action<DrawContext, float, float> renderVertex, Action<DrawContext, int, int> renderSegments) {
			var angleStep = 2 * Mathf.PI / _segments;

			var a = data.angleStart;

			var vOffset = _vertices.Count;
			if (data.centerVertex) _vertices.Add(Vertex(data.center, _color, data.rect));

			var vertexSteps = _segments;
			var polygonSteps = vertexSteps - 1;
			var lastAngleStep = angleStep;

			if (data.sector) {
				vertexSteps = Mathf.CeilToInt(data.arcLen / angleStep) + 1;
				polygonSteps = vertexSteps - 1;

				var delta = data.arcLen % angleStep;
				if (delta > 0.0001f) lastAngleStep = delta;
			}

			if (_clockwise) {
				angleStep = -angleStep;
				lastAngleStep = -lastAngleStep;
			}

			for (var i = 0; i < vertexSteps; i++) {
				if (i > 0) a += i == vertexSteps - 1 ? lastAngleStep : angleStep;

				var x = Mathf.Cos(a);
				var y = Mathf.Sin(a);

				renderVertex(data, x, y);
			}

			renderSegments(data, polygonSteps, vOffset);
		}

		private void AddQuad_AB(int vOffset, int offsetA, int offsetB) {
			_indices.Add(vOffset + offsetA + 0);
			_indices.Add(vOffset + offsetA + 1);
			_indices.Add(vOffset + offsetB + 1);

			_indices.Add(vOffset + offsetA + 0);
			_indices.Add(vOffset + offsetB + 1);
			_indices.Add(vOffset + offsetB + 0);
		}

		private void AddQuad_BA(int vOffset, int offsetA, int offsetB) {
			_indices.Add(vOffset + offsetB + 0);
			_indices.Add(vOffset + offsetB + 1);
			_indices.Add(vOffset + offsetA + 1);

			_indices.Add(vOffset + offsetB + 0);
			_indices.Add(vOffset + offsetA + 1);
			_indices.Add(vOffset + offsetA + 0);
		}

		private void AddSegment1_AB(int vOffset, int offsetA, int offsetB) {
			_indices.Add(vOffset);
			_indices.Add(vOffset + offsetA + 0);
			_indices.Add(vOffset + offsetB + 0);
		}

		private void AddSegment1_BA(int vOffset, int offsetA, int offsetB) {
			_indices.Add(vOffset);
			_indices.Add(vOffset + offsetB + 0);
			_indices.Add(vOffset + offsetA + 0);
		}

		private void AddSegment2_AB(int vOffset, int offsetA, int offsetB) {
			_indices.Add(vOffset);
			_indices.Add(vOffset + offsetA + 0);
			_indices.Add(vOffset + offsetB + 0);

			AddQuad_AB(vOffset, offsetA, offsetB);
		}

		private void AddSegment2_BA(int vOffset, int offsetA, int offsetB) {
			_indices.Add(vOffset);
			_indices.Add(vOffset + offsetB + 0);
			_indices.Add(vOffset + offsetA + 0);

			AddQuad_BA(vOffset, offsetA, offsetB);
		}

		private void AddSegment3Quads_AB(int vOffset, int offsetA, int offsetB) {
			AddQuad_AB(vOffset, offsetA, offsetB);
			AddQuad_AB(vOffset, offsetA + 1, offsetB + 1);
			AddQuad_AB(vOffset, offsetA + 2, offsetB + 2);
		}
		
		private void AddSegment3Quads_BA(int vOffset, int offsetA, int offsetB) {
			AddQuad_BA(vOffset, offsetA, offsetB);
			AddQuad_BA(vOffset, offsetA + 1, offsetB + 1);
			AddQuad_BA(vOffset, offsetA + 2, offsetB + 2);
		}

	}

}