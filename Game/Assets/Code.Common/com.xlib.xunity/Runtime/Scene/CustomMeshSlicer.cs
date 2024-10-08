using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Unity.Scene {

	[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer)), ExecuteInEditMode]
	public class CustomMeshSlicer : MonoBehaviour {

#if UNITY_EDITOR
		[ValidateInput(nameof(MeshValidator), "_meshAsset must be with read/write flag!"), SerializeField, Required, AssetsOnly, OnValueChanged(nameof(UpdateSlicedMesh))]
#endif
		private Mesh _meshAsset;

		[BoxGroup("Slicing Zones"), SerializeField, Required, Range(0, 1), EnableIf(nameof(_editMode))]
		private float[] _xSlices = { 0.5f };

		[BoxGroup("Slicing Zones"), SerializeField, Required, Range(0, 1), EnableIf(nameof(_editMode))]
		private float[] _ySlices = { 0.5f };

		[BoxGroup("Slicing Zones"), SerializeField, Required, Range(0, 1), EnableIf(nameof(_editMode))]
		private float[] _zSlices = { 0.5f };

		[BoxGroup("Slicing Widths"), SerializeField, Required, DisableIf(nameof(_editMode))]
		private float[] _xWidth = { 0.0f };

		[BoxGroup("Slicing Widths"), SerializeField, Required, DisableIf(nameof(_editMode))]
		private float[] _yWidth = { 0.0f };

		[BoxGroup("Slicing Widths"), SerializeField, Required, DisableIf(nameof(_editMode))]
		private float[] _zWidth = { 0.0f };

		private bool _editMode;

		private MeshFilter _filter;

		private Mesh _mesh;

		private SliceData[] _xSlicesLocal;
		private SliceData[] _ySlicesLocal;
		private SliceData[] _zSlicesLocal;
		private MeshFilter Filter => _filter == null ? _filter = GetComponent<MeshFilter>() : _filter;

		private void OnEnable() {
			_mesh = new Mesh();

			UpdateSlicedMesh();
		}

		private void OnDrawGizmosSelected() {
			if (!_editMode || _xSlicesLocal == null) return;

			var c = _meshAsset.bounds.center;
			var sz = _meshAsset.bounds.extents * 2;

			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.color = Color.red;
			foreach (var t in _xSlicesLocal) Gizmos.DrawCube(c.To0YZ(t.SrcPosition), new Vector3(0, sz.y, sz.z));
			Gizmos.color = Color.green;
			foreach (var t in _ySlicesLocal) Gizmos.DrawCube(c.ToX0Z(t.SrcPosition), new Vector3(sz.x, 0, sz.z));
			Gizmos.color = Color.blue;
			foreach (var t in _zSlicesLocal) Gizmos.DrawCube(c.ToXY0(t.SrcPosition), new Vector3(sz.x, sz.y, 0));

			Gizmos.matrix = Matrix4x4.identity;
		}

		[Button("Toggle Edit Zones"), GUIColor(0, 1, 0)]
		private void ToggleEditor() {
			_editMode = !_editMode;
		}

		private void UpdateSlicedMesh() {
			if (_meshAsset == null) {
				_xSlicesLocal = null;
				_ySlicesLocal = null;
				_zSlicesLocal = null;
				_mesh.Clear();
				Filter.mesh = null;
				return;
			}

			if (_xSlices.IsNullOrEmpty() && _ySlices.IsNullOrEmpty() && _zSlices.IsNullOrEmpty()) {
				_xSlicesLocal = null;
				_ySlicesLocal = null;
				_zSlicesLocal = null;
				_mesh.Clear();
				Filter.mesh = _meshAsset;
				return;
			}

			var bounds = _meshAsset.bounds;

			var min = bounds.min;
			var sz = bounds.extents * 2;

			CalcSlicing(ref _xSlicesLocal, _xSlices, _xWidth, min.x, sz.x);
			CalcSlicing(ref _ySlicesLocal, _ySlices, _yWidth, min.y, sz.y);
			CalcSlicing(ref _zSlicesLocal, _zSlices, _zWidth, min.z, sz.z);

			if (_editMode || !_meshAsset.isReadable)
				Filter.mesh = _meshAsset;
			else {
				GenerateSlicedMesh();
				Filter.mesh = _mesh;
			}
		}

		private static float GetX(Vector3 v) => v.x;

		private static float GetY(Vector3 v) => v.y;

		private static float GetZ(Vector3 v) => v.z;

		private static Vector3 SetX(Vector3 src, float v) => new(v, src.y, src.z);

		private static Vector3 SetY(Vector3 src, float v) => new(src.x, v, src.z);

		private static Vector3 SetZ(Vector3 src, float v) => new(src.x, src.y, v);

		private void GenerateSlicedMesh() {
			_mesh.Clear();
			var vertices = _meshAsset.vertices;
			var normals = _meshAsset.normals;
			var uv = _meshAsset.uv;
			var tangents = _meshAsset.tangents;
			var triangles = _meshAsset.triangles;

			foreach (var t in _xSlicesLocal) TransformVertices(t, vertices, GetX, SetX);
			foreach (var t in _ySlicesLocal) TransformVertices(t, vertices, GetY, SetY);
			foreach (var t in _zSlicesLocal) TransformVertices(t, vertices, GetZ, SetZ);

			_mesh.vertices = vertices;
			_mesh.normals = normals;
			_mesh.uv = uv;
			_mesh.tangents = tangents;
			_mesh.triangles = triangles;
			_mesh.RecalculateBounds();
		}

		private static void TransformVertices(SliceData slice, Vector3[] vertices, Func<Vector3, float> get, Func<Vector3, float, Vector3> set) {
			for (var i = 0; i < vertices.Length; i++) {
				var v = vertices[i];
				var p = get(v);

				var sign = Math.Sign(p);
				if (sign != slice.Sign) continue;

				if (Math.Abs(p) < slice.SrcDistance) continue;

				p += sign * slice.Width;

				vertices[i] = set(v, p);
			}
		}

		private static void CalcSlicing(ref SliceData[] dest, float[] src, float[] width, float offset, float sz) {
			src ??= Array.Empty<float>();

			if (dest?.Length != src.Length) dest = new SliceData[src.Length];

			for (var i = 0; i < dest.Length; i++) {
				var p = src[i] * sz + offset;

				dest[i] = new SliceData(p, Math.Abs(p), Math.Sign(p), width.GetOrDefault(i));
			}
		}

		private readonly struct SliceData {

			public SliceData(float srcPosition, float srcDistance, int sign, float width) {
				SrcPosition = srcPosition;
				SrcDistance = srcDistance;
				Sign = sign;
				Width = width;
			}

			public float SrcPosition { get; }
			public float SrcDistance { get; }
			public int Sign { get; }
			public float Width { get; }

		}

#if UNITY_EDITOR
		private bool MeshValidator(Mesh mesh) => mesh != null && mesh.isReadable;

		private void Update() {
			if (Application.isPlaying) return;

			UpdateSlicedMesh();
		}

#endif

	}

}