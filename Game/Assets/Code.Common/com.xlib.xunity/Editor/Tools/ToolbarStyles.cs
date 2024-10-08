using UnityEngine;

namespace XLib.Unity.Tools {

	public static class ToolbarStyles {

		public static readonly GUIStyle CommandButtonStyle = new("Command") {
			fontSize = 16,
			alignment = TextAnchor.MiddleCenter,
			imagePosition = ImagePosition.ImageLeft,
			fontStyle = FontStyle.Normal
		};
	}

}