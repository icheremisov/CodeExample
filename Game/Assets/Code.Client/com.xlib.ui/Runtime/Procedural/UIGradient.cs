using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Procedural {

	public partial class UIGradient : BaseMeshEffect {
		[HorizontalGroup("Top"), HideLabel]
		[SerializeField] private Color _topLeftColor = Color.white;
		[HorizontalGroup("Top"), HideLabel]
		[SerializeField] private Color _topRightColor = Color.white;

		[HorizontalGroup("Bottom"), HideLabel]
		[SerializeField] private Color _botLeftColor = Color.black;
		[HorizontalGroup("Bottom"), HideLabel]
		[SerializeField] private Color _botRightColor = Color.black;

		public void SetColorVertical(Color top, Color bottom) {
			_topLeftColor = _topRightColor = top;
			_botLeftColor = _botRightColor = bottom;
			graphic.SetVerticesDirty();
		}

		public void SetColorHorizontal(Color left, Color right) {
			_topLeftColor = _botLeftColor = left;
			_topRightColor = _botRightColor = right;
			graphic.SetVerticesDirty();
		}

		public Color TopLeft {
			get => _topRightColor;
			set {
				_topRightColor = value;
				graphic.SetVerticesDirty();
			}
		}

		public Color TopRight {
			get => _topLeftColor;
			set {
				_topLeftColor = value;
				graphic.SetVerticesDirty();
			}
		}

		public Color BottomLeft {
			get => _botRightColor;
			set {
				_botRightColor = value;
				graphic.SetVerticesDirty();
			}
		}

		public Color BottomRight {
			get => _botLeftColor;
			set {
				_botLeftColor = value;
				graphic.SetVerticesDirty();
			}
		}

		private readonly List<UIVertex> _vertices = new(64);

		public override void ModifyMesh(VertexHelper vertexHelper) {
			if (!IsActive()) return;

			vertexHelper.GetUIVertexStream(_vertices);

			ModifyVertices(_vertices);

			vertexHelper.Clear();
			vertexHelper.AddUIVertexTriangleStream(_vertices);
		}

		private void ModifyVertices(List<UIVertex> vertices) {
			if (!IsActive() || vertices.Count == 0) return;

			var count = vertices.Count;
			var bottomX = vertices[0].position.x;
			var bottomY = vertices[0].position.y;
			var topX = vertices[0].position.x;
			var topY = vertices[0].position.y;

			GetTopAndBottomPosition(vertices, count, ref topX, ref bottomX, ref topY, ref bottomY);

			var width = topX - bottomX;
			var height = topY - bottomY;

			for (var i = 0; i < count; i++) {
				var uiVertex = vertices[i];

				var xDelta = (uiVertex.position.x - bottomX) / width;
				var yDelta = (uiVertex.position.y - bottomY) / height;

				var botColor = Color.Lerp(_topLeftColor, _topRightColor, xDelta);
				var topColor = Color.Lerp(_botLeftColor, _botRightColor, xDelta);
				var resultColor = Color.Lerp(topColor, botColor, yDelta);

				uiVertex.color *= resultColor;

				vertices[i] = uiVertex;
			}
		}

		private static void GetTopAndBottomPosition(List<UIVertex> vertices, int count, ref float topX, ref float bottomX, ref float topY, ref float bottomY) {
			for (var i = 1; i < count; i++) {
				var x = vertices[i].position.x;
				var y = vertices[i].position.y;

				if (x > topX)
					topX = x;
				else if (x < bottomX) bottomX = x;

				if (y > topY)
					topY = y;
				else if (y < bottomY) bottomY = y;
			}
		}
	}

}