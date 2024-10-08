using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XLib.Unity.FX {

	[RequireComponent(typeof(CanvasRenderer))]
	public class UITrailRenderer : MaskableGraphic {

		private const float Multiplier = 0.5f;
		[Header("Emitter"), SerializeField] public bool _emitting = true;
		[SerializeField] public float _lifetime = 1f;
		[SerializeField] public Gradient _colorOverDistance = new();

		[Header("Size"), SerializeField] public AnimationCurve _width = new(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
		[SerializeField] public float _widthScale = 1;
		[SerializeField] public float _minVertexDistance = 0.1f;

		[Range(0, 90), SerializeField] public int _endCapVertices;

		[Header("Texture"), SerializeField] private Texture _texture;
		[Range(0f, 1f), SerializeField] public float _textureScale;

		private readonly List<Point> _points = new(64);

		public Texture Texture {
			get => _texture;
			set {
				if (_texture == value) return;

				_texture = value;
				SetVerticesDirty();
				SetMaterialDirty();
			}
		}

		public override Texture mainTexture => _texture == null ? s_WhiteTexture : _texture;

		private void Update() {
			var pointToAdd = new Point(transform.position, _lifetime);

			if (_points.Count <= 1) {
				_points.Clear();
				for (var i = 0; i < 2; i++) _points.Add(pointToAdd);
			}

			if (_emitting) {
				if (_points[^1].Distance(_points[^2]) > _minVertexDistance)
					_points.Add(pointToAdd);

				else {
					var pnt = _points[^1];
					pnt.Position = pointToAdd.Position;
					_points[^1] = pnt;
				}
			}

			for (var i = 0; i < _points.Count; i++) {
				var point = _points[i];
				point.LifeTime -= Time.deltaTime;
				_points[i] = point;
			}

			for (var i = _points.Count - 1; i >= 0; i--) {
				var point = _points[i];
				if (!point.IsAlive) _points.Remove(point);
			}

			SetVerticesDirty();
		}

		protected override void OnEnable() {
			base.OnEnable();
			_points.Clear();
		}

		protected override void OnRectTransformDimensionsChange() {
			base.OnRectTransformDimensionsChange();
			SetVerticesDirty();
			SetMaterialDirty();
		}

		private void AddSymmetricalVerts(VertexHelper vh, UIVertex vert, Vector2 pos, Vector2 normal) {
			vert.uv0.y = 1;

			vert.position = transform.InverseTransformPoint(pos + normal);
			vh.AddVert(vert);

			vert.uv0.y = 0;

			vert.position = transform.InverseTransformPoint(pos - normal);
			vh.AddVert(vert);
		}

		private void AddCapVerts(VertexHelper vh, UIVertex vert, Vector2 capVertPos, Vector2 capVertNormal,
			bool head = true) {
			var mult = head ? 1f : -1f;

			var n = capVertNormal * mult;
			var uvn = Vector2.up * 0.5f * mult;

			var theta = 180f / (_endCapVertices + 1);

			var cos = Mathf.Cos(theta * Mathf.Deg2Rad);
			var sin = Mathf.Sin(theta * Mathf.Deg2Rad);

			vert.uv0.y = 0.5f;
			Vector2 b = vert.uv0;

			for (var i = 0; i < _endCapVertices; i++) {
				uvn = new Vector2(cos * uvn.x - sin * uvn.y, sin * uvn.x + cos * uvn.y);
				n = new Vector2(cos * n.x - sin * n.y, sin * n.x + cos * n.y);

				vert.uv0 = b + uvn;
				vert.position = transform.InverseTransformPoint(capVertPos + n);

				vh.AddVert(vert);
			}

			vert.uv0 = b;

			vert.position = transform.InverseTransformPoint(capVertPos);
			vh.AddVert(vert);
		}

		private void EndCapTail(VertexHelper vh, UIVertex vert, Vector2 headPos, Vector2 headNormal) {
			if (_endCapVertices == 0) return;

			AddCapVerts(vh, vert, headPos, headNormal);

			vh.AddTriangle(0, vh.currentVertCount - 1, 2);

			for (var i = 0; i < _endCapVertices - 1; i++) vh.AddTriangle(i + 2, vh.currentVertCount - 1, i + 3);

			vh.AddTriangle(vh.currentVertCount - 1, 1, vh.currentVertCount - 2);
		}

		private void EndCapHead(VertexHelper vh, UIVertex vert, Vector2 headPos, Vector2 headNormal) {
			if (_endCapVertices == 0) return;

			var vc = vh.currentVertCount;

			AddCapVerts(vh, vert, headPos, headNormal, false);

			vh.AddTriangle(vc - 2, vc + _endCapVertices - 1, vh.currentVertCount - 1);

			for (var i = 0; i < _endCapVertices - 1; i++) vh.AddTriangle(vc + i, vh.currentVertCount - 1, vc + i + 1);

			vh.AddTriangle(vh.currentVertCount - 1, vc, vc - 1);
		}

		protected override void OnPopulateMesh(VertexHelper vh) {
			if (_points.Count < 2) {
				vh.Clear();
				return;
			}

			vh.Clear();
			var point = _points[0];

			var vert = new UIVertex { color = _colorOverDistance.Evaluate(1f), uv0 = Vector2.zero };
			var nCur = point.GetNormal(_points[1]) * Multiplier * GetWidth(1);
			AddSymmetricalVerts(vh, vert, point.Position, nCur);

			if (_endCapVertices > 0) EndCapTail(vh, vert, _points[0].Position, nCur);

			var nBC = nCur;

			for (var pointIndex = 1; pointIndex < _points.Count; pointIndex++) {
				point = _points[pointIndex];
				var nAB = nBC;

				if (pointIndex < _points.Count - 1) {
					nBC = point.GetNormal(_points[pointIndex + 1]) * Multiplier;
					nCur = ((nAB + nBC) * 0.5f).normalized * Multiplier;
				}

				else
					nCur = nBC;

				var value = 1f - (float)(pointIndex + 1) / _points.Count;

				vert.color = _colorOverDistance.Evaluate(value);

				nCur *= GetWidth(value);

				vert.uv0 = new Vector2(_textureScale * pointIndex, 0);

				AddSymmetricalVerts(vh, vert, point.Position, nCur);

				var mp2 = _endCapVertices > 0 ? 1 : 0;
				var mp = pointIndex > 1 && mp2 == 1 ? 1 : 0;

				var shift = _endCapVertices + 1;
				var vIndex = 2 * pointIndex;
				var vPrevIndex = vIndex - 2;

				vh.AddTriangle(mp * shift + vPrevIndex, mp2 * shift + vIndex, mp * shift + vIndex - 1);
				vh.AddTriangle(mp * shift + vIndex - 1, mp2 * shift + vIndex, mp2 * shift + vIndex + 1);
			}

			if (_endCapVertices <= 0) return;

			if (nCur == Vector2.zero && _points.Count > 2) nCur = _points[_points.Count - 3].GetNormal(_points[_points.Count - 1]);

			nCur = nCur.normalized * Multiplier * GetWidth(0f);
			vert.color = _colorOverDistance.Evaluate(0f);

			EndCapHead(vh, vert, _points[_points.Count - 1].Position, nCur);
		}

		private float GetWidth(float t) => _width.Evaluate(t) * _widthScale;

		public struct Point {

			public Vector2 Position;
			public float LifeTime;

			public Point(Vector3 position, float lifeTime) {
				Position = position;
				LifeTime = lifeTime;
			}

			public float Distance(Point b) => (Position - b.Position).magnitude;

			public bool IsAlive => LifeTime > 0;

			public Vector2 GetNormal(Point b) {
				var v = b.Position - Position;
				return new Vector2(-v.y, v.x).normalized;
			}

		}

	}

}