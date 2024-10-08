using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLib.UI.Procedural.Brushes;

namespace XLib.UI.Procedural {

	public abstract class UIProceduralEffect : BaseMeshEffect {
		
		// ReSharper disable once InconsistentNaming
		protected readonly List<UIVertex> _vertices = new(64);
		// ReSharper disable once InconsistentNaming
		protected readonly List<int> _indices = new(64);

		public override void ModifyMesh(VertexHelper vertexHelper) {
			if (!IsActive()) return;

			_vertices.Clear();
			_indices.Clear();
			GenerateGraphics();

			vertexHelper.Clear();
			vertexHelper.AddUIVertexStream(_vertices, _indices);
		}

		protected abstract void GenerateGraphics();
		
		protected UIVertex Vertex(Vector2 pos, UIProceduralBrush brush, Rect rect) {
			var color = brush.Get(pos, rect) * graphic.color;
			return GeometryUtils.Vertex(pos, color);
		}

		protected UIVertex Transparent(Vector2 pos, UIProceduralBrush brush, Rect rect) {
			var color = brush.Get(pos, rect) * graphic.color;
			return GeometryUtils.Vertex(pos, color.ReplaceA(0));
		}

		protected void ApplyColor(UIProceduralBrush brush, Rect rect, int vOffset, int verticesCount) {

			for (var i = 0; i < verticesCount; i++) {
				var v = _vertices[vOffset + i];
				v.color = v.color * brush.Get(v.position, rect); 
				_vertices[vOffset + i] = v;
			}
		}
	}

}