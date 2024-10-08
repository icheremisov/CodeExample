using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using XLib.Utils;

namespace XLib.UI.Controls {

	[AddComponentMenu("UI/Effects/UI Mesh Slicer", 16), RequireComponent(typeof(Image))]
	public class UIMeshSlicer : BaseMeshEffect {

		[SerializeField] private Vector2 _sliceLocalPosition = Vector2.zero;
		[SerializeField, Range(0, 360)] private float _sliceLocalAngle = 90;
		[SerializeField] private bool _invert;

		private readonly UIVertex[] _poly = new UIVertex[3];
		private readonly List<UIVertex> _verticesDst = new(32);

		private readonly List<UIVertex> _verticesSrc = new(32);
		private readonly bool[] _visible = new bool[3];

		/// <summary>
		///     set slice in local-space coordinates
		/// </summary>
		public void SetSliceLocal(Vector2 pos, float angleDeg) {
			if (_sliceLocalPosition != pos || _sliceLocalAngle != angleDeg) {
				_sliceLocalPosition = pos;
				_sliceLocalAngle = angleDeg;
				graphic.SetVerticesDirty();
			}
		}

		public override void ModifyMesh(VertexHelper vh) {
			var image = GetComponent<Image>();
			if (image.type != Image.Type.Simple) return;

			vh.GetUIVertexStream(_verticesSrc);
			_verticesDst.Clear();

			var sliceDir = Vector2.right.GetRotated(_sliceLocalAngle);

			var sliceDir90 = _invert ? Vector2.right.GetRotated(_sliceLocalAngle + 90) : Vector2.right.GetRotated(_sliceLocalAngle - 90);

			var slicePos3d = _sliceLocalPosition.ToXY0();
			var sliceDir3d = sliceDir.ToXY0();

			bool IsVisible(Vector2 v) {
				var dir = v - _sliceLocalPosition;

				return Vector2.Dot(dir, sliceDir90) >= 0;
			}

			for (var polyIndex = 0; polyIndex < _verticesSrc.Count; polyIndex += 3) {
				var numVisible = 0;

				for (var i = 0; i < 3; i++) {
					_poly[i] = _verticesSrc[polyIndex + i];
					_visible[i] = IsVisible(_poly[i].position.ToXY());

					if (_visible[i]) ++numVisible;
				}

				if (numVisible == 0) continue;

				switch (numVisible) {
					case 0:
						// skip triangle
						break;

					case 1:
						// clip triangle with 1 visible vertex
						ClipTriangle1(slicePos3d, sliceDir3d);
						break;

					case 2:
						// clip triangle with 2 visible vertices
						ClipTriangle2(slicePos3d, sliceDir3d);
						break;

					case 3:
						// add triangle
						_verticesDst.AddRange(_poly);
						break;
				}
			}

			vh.Clear();
			vh.AddUIVertexTriangleStream(_verticesDst);
		}

		[Il2CppSetOption(Option.NullChecks, false),
		 Il2CppSetOption(Option.ArrayBoundsChecks, false),
		 Il2CppSetOption(Option.DivideByZeroChecks, false),
		 MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ClipTriangle1(Vector3 slicePos, Vector3 sliceDir) {
			var visible0i = 0;
			var other0i = 0;
			var other1i = 0;

			for (var i = 0; i < 3; i++) {
				if (_visible[i]) {
					visible0i = i;
					other0i = (i + 1) % 3;
					other1i = (i + 2) % 3;
					break;
				}
			}

			var visible0 = _poly[visible0i];
			var other0 = _poly[other0i];
			var other1 = _poly[other1i];

			Math3D.LineLineIntersection(out var slice0pos, visible0.position, other0.position - visible0.position, slicePos, sliceDir);
			Math3D.LineLineIntersection(out var slice1pos, visible0.position, other1.position - visible0.position, slicePos, sliceDir);

			var slice0 = MakeVertex(visible0i, other0i, slice0pos);
			var slice1 = MakeVertex(visible0i, other1i, slice1pos);

			_verticesDst.Add(visible0);
			_verticesDst.Add(slice0);
			_verticesDst.Add(slice1);
		}

		[Il2CppSetOption(Option.NullChecks, false),
		 Il2CppSetOption(Option.ArrayBoundsChecks, false),
		 Il2CppSetOption(Option.DivideByZeroChecks, false),
		 MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ClipTriangle2(Vector3 slicePos, Vector3 sliceDir) {
			var visible0i = 0;
			var visible1i = 0;
			var other0i = 0;

			for (var i = 0; i < 3; i++) {
				if (!_visible[i]) {
					other0i = i;
					visible0i = (i + 1) % 3;
					visible1i = (i + 2) % 3;
					break;
				}
			}

			var visible0 = _poly[visible0i];
			var visible1 = _poly[visible1i];
			var other0 = _poly[other0i];

			Math3D.LineLineIntersection(out var slice0pos, visible0.position, other0.position - visible0.position, slicePos, sliceDir);
			Math3D.LineLineIntersection(out var slice1pos, visible1.position, other0.position - visible1.position, slicePos, sliceDir);

			var slice0 = MakeVertex(visible0i, other0i, slice0pos);
			var slice1 = MakeVertex(visible1i, other0i, slice1pos);

			_verticesDst.Add(visible0);
			_verticesDst.Add(slice0);
			_verticesDst.Add(slice1);

			_verticesDst.Add(visible1);
			_verticesDst.Add(visible0);
			_verticesDst.Add(slice1);
		}

		[Il2CppSetOption(Option.NullChecks, false),
		 Il2CppSetOption(Option.ArrayBoundsChecks, false),
		 Il2CppSetOption(Option.DivideByZeroChecks, false),
		 MethodImpl(MethodImplOptions.AggressiveInlining)]
		private UIVertex MakeVertex(int index0, int index1, Vector3 pos) {
			var v0 = _poly[index0];
			var v1 = _poly[index1];

			var k = (pos - v0.position).magnitude / (v1.position - v0.position).magnitude;

			var result = v0;
			result.position = pos;
			result.color = Color32.Lerp(v0.color, v1.color, k);
			result.uv0 = Vector2.Lerp(v0.uv0, v1.uv0, k);
			return result;
		}

	}

}