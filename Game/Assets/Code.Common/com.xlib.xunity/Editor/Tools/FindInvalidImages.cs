using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace XLib.Unity.Tools {

	public class FindInvalidImages : EditorWindow {

		private static int go_count, components_count, invalid_count;

		public void OnGUI() {
			if (GUILayout.Button("Find Missing Scripts in selected GameObjects")) FindInSelected();
		}

		[MenuItem("Tools/Assets/Find Invalid Images")]
		public static void ShowWindow() {
			GetWindow(typeof(FindInvalidImages));
		}

		private static void FindInSelected() {
			var go = Selection.gameObjects;
			go_count = 0;
			components_count = 0;
			invalid_count = 0;
			foreach (var g in go) FindInGO(g);

			if (invalid_count > 0)
				Debug.LogWarning($"Searched {go_count} GameObjects, {components_count} components, found {invalid_count} invalid images");
			else
				Debug.Log($"Searched {go_count} GameObjects, {components_count} components, found {invalid_count} invalid images");
		}

		private static void FindInGO(GameObject g) {
			go_count++;
			var components = g.GetComponents<Image>();
			for (var i = 0; i < components.Length; i++) {
				if (components[i] == null) continue;
				components_count++;

				var image = components[i];
				
				if (image.sprite == null) continue;

				if (image.type == Image.Type.Simple && image.useSpriteMesh == false) {
					invalid_count++;
					Debug.LogWarning($"{image.GetFullPath()} has useSpriteMesh=false", g);
				}
			}

			foreach (Transform childT in g.transform)
				FindInGO(childT.gameObject);
		}

	}

}