using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Procedural {

	public static class GeometryUtils {
		public static UIVertex Vertex(Vector2 pos, Color color) => new() { position = pos, uv0 = Vector4.zero, color = color };
		public static UIVertex Vertex(Vector2 pos, Vector2 uv, Color color) => new() { position = pos, uv0 = uv, color = color };

		public static void AddQuad(List<UIVertex> vertexBuffer, List<int> indexBuffer, Vector2 corner1, Vector2 corner2, Vector2 uvCorner1, Vector2 uvCorner2, Color color) {
			var i = vertexBuffer.Count;

			var vert = new UIVertex();
			vert.color = color;

			vert.position = corner1;
			vert.uv0 = uvCorner1;
			vertexBuffer.Add(vert);

			vert.position = new Vector2(corner2.x, corner1.y);
			vert.uv0 = new Vector2(uvCorner2.x, uvCorner1.y);
			vertexBuffer.Add(vert);

			vert.position = corner2;
			vert.uv0 = uvCorner2;
			vertexBuffer.Add(vert);

			vert.position = new Vector2(corner1.x, corner2.y);
			vert.uv0 = new Vector2(uvCorner1.x, uvCorner2.y);
			vertexBuffer.Add(vert);

			indexBuffer.Add(i + 0);
			indexBuffer.Add(i + 2);
			indexBuffer.Add(i + 1);
			indexBuffer.Add(i + 3);
			indexBuffer.Add(i + 2);
			indexBuffer.Add(i + 0);
		}

		public static void AddQuad(VertexHelper vh, Vector2 corner1, Vector2 corner2, Vector2 uvCorner1, Vector2 uvCorner2, Color color) {
			var i = vh.currentVertCount;

			var vert = new UIVertex();
			vert.color = color;

			vert.position = corner1;
			vert.uv0 = uvCorner1;
			vh.AddVert(vert);

			vert.position = new Vector2(corner2.x, corner1.y);
			vert.uv0 = new Vector2(uvCorner2.x, uvCorner1.y);
			vh.AddVert(vert);

			vert.position = corner2;
			vert.uv0 = uvCorner2;
			vh.AddVert(vert);

			vert.position = new Vector2(corner1.x, corner2.y);
			vert.uv0 = new Vector2(uvCorner1.x, uvCorner2.y);
			vh.AddVert(vert);

			vh.AddTriangle(i + 0, i + 2, i + 1);
			vh.AddTriangle(i + 3, i + 2, i + 0);
		}

		public static void AddQuad(VertexHelper vh, Vector2 lt, Vector2 rb, Vector2 uvLT, Vector2 uvRB, Color cLT, Color cRT, Color cLB, Color cRB) {
			var i = vh.currentVertCount;

			var vert = new UIVertex();

			vert.color = cLT;
			vert.position = lt;
			vert.uv0 = uvLT;
			vh.AddVert(vert);

			vert.color = cRT;
			vert.position = new Vector2(rb.x, lt.y);
			vert.uv0 = new Vector2(uvRB.x, uvLT.y);
			vh.AddVert(vert);

			vert.color = cRB;
			vert.position = rb;
			vert.uv0 = uvRB;
			vh.AddVert(vert);

			vert.color = cLB;
			vert.position = new Vector2(lt.x, rb.y);
			vert.uv0 = new Vector2(uvLT.x, uvRB.y);
			vh.AddVert(vert);

			vh.AddTriangle(i + 0, i + 2, i + 1);
			vh.AddTriangle(i + 3, i + 2, i + 0);
		}

		public static void GenerateGrid(List<UIVertex> vertices, List<int> indices, Vector2 srcPos, float[] columns, float[] rows, bool fillCenter, params Color[] colors) {
			
			var vOffset = vertices.Count;

			var pos = srcPos;
			var colCount = columns.Length;
			var rowCount = rows.Length;

			for (var y = 0; y <= rowCount; y++) {
				pos.x = srcPos.x;
				if (y > 0) pos.y += rows[y - 1];

				for (var x = 0; x < colCount; x++) {
					var colSize = columns[x];
						
					if (x == 0) vertices.Add(Vertex(pos, GetColor(x, y)));
					vertices.Add(Vertex(pos + new Vector2(colSize, 0), GetColor(x + 1, y)));
						
					pos.x += colSize;
				}
			}

			for (var y = 0; y < rowCount; y++) {
				var offsetTop = vOffset + y * (colCount + 1);
				var offsetBottom = vOffset + (y + 1) * (colCount + 1);
					
				for (var x = 0; x < colCount; x++) {
					if (!fillCenter && x == colCount / 2 && y == rowCount / 2) {
						++offsetTop;
						++offsetBottom;
						continue;
					}  
					
					indices.Add(offsetBottom + 0);
					indices.Add(offsetTop + 1);
					indices.Add(offsetTop + 0);
					
					indices.Add(offsetBottom + 0);
					indices.Add(offsetBottom + 1);
					indices.Add(offsetTop + 1);

					++offsetTop;
					++offsetBottom;
				}
			}

			return;

			Color GetColor(int x, int y) => colors.Length == 1 ? colors[0] : colors[y * (colCount + 1) + x];
		}
		
		public static void GenerateGrid(List<UIVertex> vertices, List<int> indices, Rect rect, Vector2Int cellCount, Color color) {

			if (rect.size.x <= 0 || rect.size.y <= 0) return;
			if (cellCount.x <= 0 || cellCount.y <= 0) return;
			
			var vOffset = vertices.Count;

			var pos = rect.min;
			var colCount = cellCount.x;
			var rowCount = cellCount.y;

			var cellSize =  rect.size / cellCount;

			for (var y = 0; y <= rowCount; y++) {
				pos.x = rect.min.x;
				if (y > 0) pos.y += cellSize.y;

				for (var x = 0; x < colCount; x++) {
					var colSize = cellSize.x;
						
					if (x == 0) vertices.Add(Vertex(pos, color));
					vertices.Add(Vertex(pos + new Vector2(colSize, 0), color));
						
					pos.x += colSize;
				}
			}

			for (var y = 0; y < rowCount; y++) {
				var offsetTop = vOffset + y * (colCount + 1);
				var offsetBottom = vOffset + (y + 1) * (colCount + 1);
					
				for (var x = 0; x < colCount; x++) {
					
					indices.Add(offsetBottom + 0);
					indices.Add(offsetTop + 1);
					indices.Add(offsetTop + 0);
					
					indices.Add(offsetBottom + 0);
					indices.Add(offsetBottom + 1);
					indices.Add(offsetTop + 1);

					++offsetTop;
					++offsetBottom;
				}
			}

			return;
		}
	}

}