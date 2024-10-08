using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Client.Scene {

	internal class SceneSetupWindow : EditorWindow {
		private Camera _camera;
		private float _backgroundHeight = 1432;
		private float _viewportHeight = 1170;

		public void OnGUI() {
			if (Selection.gameObjects.Length == 0) {
				GUILayout.BeginVertical();
				GUILayout.Label("Select objects in scene");
				GUILayout.EndVertical();
				return;
			}

			GUILayout.BeginVertical();
			GUILayout.Label("Arrange To Camera");
			_camera = EditorGUILayout.ObjectField(_camera, typeof(Camera), true) as Camera;
			_backgroundHeight = EditorGUILayout.FloatField("Background Height", _backgroundHeight);
			_viewportHeight = EditorGUILayout.FloatField("Viewport Height", _viewportHeight);
			if (GUILayout.Button("Align To Camera")) AlignToCamera(false);

			if (GUILayout.Button("Align and scale To Camera")) AlignToCamera(true);

			GUILayout.EndVertical();
		}

		public void OnSelectionChange() {
			Repaint();
		}

		[MenuItem("Tools/Scene Setup", false, 1000)]
		public static void SceneSetup() {
			GetWindow(typeof(SceneSetupWindow), false, "Scene Setup");
		}

		private void AlignToCamera(bool scaleToFrustum) {
			var sceneObjects = Selection.gameObjects.Where(x => x.scene.IsValid()).ToArray();
			if (sceneObjects.Length == 0) return;

			var camera = _camera;
			Debug.Assert(camera != null);
			var angle = camera.transform.eulerAngles.x;
			var camRay = new Ray(camera.transform.position, camera.transform.forward);

			foreach (var obj in sceneObjects) {
				var plane = new Plane(Vector3.forward, obj.transform.position);

				if (!plane.Raycast(camRay, out Vector3 pos)) continue;

				Undo.RecordObject(obj, "AlignToCamera");

				obj.transform.position = pos;
				obj.transform.rotation = Quaternion.Euler(angle, 0, 0);

				if (scaleToFrustum) {
					var sprite = obj.GetComponent<SpriteRenderer>();
					if (sprite) {
						var frustumHeight = 2.0f * (pos - camRay.origin).magnitude * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

						var pixelsPerUnit = sprite.sprite.pixelsPerUnit;
						var scaleCoef = _backgroundHeight / _viewportHeight;
						
						var scale = frustumHeight / (_backgroundHeight / pixelsPerUnit) * scaleCoef;
						obj.transform.SetLocalScale(scale);
					}
				}

				EditorUtility.SetDirty(obj);
			}

			Undo.FlushUndoRecordObjects();
		}
	}

}