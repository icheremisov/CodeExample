using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Controls {

	[RequireComponent(typeof(CanvasRenderer))]
	public class UIEmptyGraphic : Graphic {

		protected override void OnPopulateMesh(VertexHelper vh) {
			vh.Clear();
		}

	}

}