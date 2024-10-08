using UnityEditor;

namespace XLib.UI.Controls {

	[CustomEditor(typeof(ContentSizeFitterEx))]
	public class ContentSizeFitterExEditor : Editor {

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			//var contentSizeFitter = (CustomContentSizeFitter)target;
		}
	}

}