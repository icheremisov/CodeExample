using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Procedural {

	public abstract class UIVertexModifier : BaseMeshEffect {
		
		private readonly List<UIVertex> _vertices = new(64);

		public override void ModifyMesh(VertexHelper vertexHelper) {
			if (!IsActive()) return;

			vertexHelper.GetUIVertexStream(_vertices);

			vertexHelper.Clear();
			vertexHelper.AddUIVertexTriangleStream(_vertices);
		}

		protected abstract bool ModifyVertices();
	}
	

}