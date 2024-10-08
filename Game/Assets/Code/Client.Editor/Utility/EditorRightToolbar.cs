using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLib.Unity.Cameras;
using XLib.Unity.Core;
using XLib.Unity.Tools;

namespace Client.Utility {

	[InitializeOnLoad]
	public static class EditorRightToolbar {
		static EditorRightToolbar() {
			ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
		}

		private static UnityEngine.SceneManagement.Scene? GetEnvScene() {
			for (var i = 0; i < SceneManager.sceneCount; i++) {
				var scene = SceneManager.GetSceneAt(i);
				if (scene.IsValid() && scene.isLoaded && scene.path.Contains("/Environment/")) return scene;
			}

			return null;
		}

		private static void OnToolbarGUI() {
			if (Application.isPlaying) return;

			// GUILayout.FlexibleSpace();
			if (GUILayout.Button(EditorGUIUtility.IconContent("IN foldout focus", "|Play game from Start"), (GUIStyle)"toolbarbutton", GUILayout.Width(30),
					GUILayout.Height(ToolbarExtender.defaultHeight-5))) {
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
					GameLoader.PlayGameFromStart();
					// GUIUtility.ExitGUI();
					return;
				}
			}
			
			var scene = GetEnvScene();
			if (scene != null) DrawEnvSceneButtons(scene.Value);
		}

		private static void DrawEnvSceneButtons(UnityEngine.SceneManagement.Scene scene) {
			GUILayout.Space(ToolbarExtender.space);

			if (GUILayout.Button(EditorGUIUtility.IconContent("d_SceneViewCamera", "|View to Game Camera"), GUILayout.Width(30), GUILayout.Height(20))) {
				var cameras = Object.FindObjectsByType<CameraLayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

				var camera = cameras.FirstOrDefault();
				if (camera) {
					var view = SceneView.lastActiveSceneView;

					if (view) {
						view.AlignViewToObject(camera.transform);
						view.Repaint();
					}
				}
				else {
					Debug.LogError($"Cannot find world camera");
				}
			}

			GUILayout.Space(ToolbarExtender.space);

			if (GUILayout.Button(EditorGUIUtility.IconContent("d_BuildSettings.Standalone.Small", "|Play with Fake Screen"), GUILayout.Width(30), GUILayout.Height(20))) {
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
					GameLoader.PlayGameFromCurrentScene(loadAdditiveScenes: new[] { "BattleScreen" });
					return;
				}
			}
		}
	}

}