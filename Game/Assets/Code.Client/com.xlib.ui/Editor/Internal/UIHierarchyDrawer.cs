using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLib.Core.Utils;
using XLib.UI.Views;

namespace XLib.UI.Internal {

	[InitializeOnLoad]
	public class UIHierarchyDrawer {
		private static readonly GUIContent IconWarn;
		private static readonly HashSet<int> MarkedObjects = new();
		private static readonly List<UIView> Screens = new(10);

		static UIHierarchyDrawer() {
			EditorApplication.update += MarkScreensNoCamera;
			EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;

			IconWarn = EditorGUIUtility.IconContent("d_console.erroricon.sml");
		}

		private static void MarkScreensNoCamera() {
			MarkedObjects.Clear();
			Screens.Clear();
			for (var i = 0; i < SceneManager.sceneCount; i++) {
				var scene = SceneManager.GetSceneAt(i);
				Screens.AddRange(scene.GetRootGameObjects().Select(x => x.GetComponent<UIView>()).Where(x => x != null));
			}

			if (Screens.IsNullOrEmpty()) return;

			foreach (var screen in Screens.Where(screen => screen.GetCanvasCamera() == null)) MarkedObjects.Add(screen.gameObject.GetInstanceID());
		}

		private static void DrawHierarchyItem(int instanceID, Rect rect) {
			if (!MarkedObjects.Contains(instanceID)) return;

			using (new GUILayout.AreaScope(rect)) {
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label(IconWarn);
				GUILayout.EndHorizontal();
			}
		}
	}

}