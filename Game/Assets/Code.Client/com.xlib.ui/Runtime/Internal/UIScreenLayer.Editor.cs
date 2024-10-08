#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using XLib.UI.Views;

namespace XLib.UI.Internal {

	[InitializeOnLoad]
	public partial class UIScreenLayer {

		private const string UICamPrefab = "Assets/Data.Core/Cameras/Camera - UI.prefab";
		private const string UICamAdditionalPrefab = "Assets/Data.Core/Cameras/Camera - UI (Additional).prefab";
		private const string UICamAdditional3DPrefab = "Assets/Data.Core/Cameras/Camera - UI 3D (Additional).prefab";
		private const string EditorCameraName = "EditorPreviewCamera";

		private bool NeedAdditionalCamera => _camera == null;

		static UIScreenLayer()
		{
			EditorApplication.hierarchyChanged += OnHierarchyChanged;
			SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
		}

		private static void SceneManagerOnsceneLoaded(UnityEngine.SceneManagement.Scene arg0, LoadSceneMode arg1) => OnHierarchyChanged();

		private static void OnHierarchyChanged() {
			if (EditorApplication.isPlayingOrWillChangePlaymode) return;
			
			var all = SceneManager.GetActiveScene().GetRootGameObjects();
			var layers = all.SelectNotNull(x => x.GetComponent<UIScreenLayer>()).ToArray();
			
			for (var i = 0; i < layers.Length; i++) {
				var order = i + 1;
				if (layers[i]._sceneOrder != order) {
					layers[i]._sceneOrder = order;
					EditorUtility.SetDirty(layers[i]);
				}
			}	
		}	
		
		[Button, ShowIf(nameof(NeedAdditionalCamera)), PropertySpace(spaceBefore: 30)]
		public void SetupCanvasLayer() {

			SetupAdditional(this, UICamAdditionalPrefab);

			var canvas = this.GetComponentInChildren<Canvas>(true);
			canvas.renderMode = RenderMode.ScreenSpaceCamera;
			canvas.worldCamera = Camera;
			canvas.planeDistance = 0;
			
			ArrangeLayers();
		}

		[Button, ShowIf(nameof(NeedAdditionalCamera))]
		public void SetupWorldLayer() {
			SetupAdditional(this, UICamAdditional3DPrefab);
			ArrangeLayers();
		}
		
		[Button, PropertySpace(spaceBefore: 30)]
		private void ArrangeLayers() {
			if (!gameObject.scene.IsValid()) return;

			ArrangeLayers(gameObject.scene.GetRootGameObjects());
		}

		public static void ArrangeLayers(GameObject[] rootGameObjects) {

			var tm = new List<Transform>(8);
			var cameras = new List<Camera>(8);
			foreach (var rootObj in rootGameObjects) {
				var layer = rootObj.GetComponent<UIScreenLayer>();
				if (layer) {
					tm.Add(layer.transform);
					cameras.Add(layer.Camera);
				}
				
				var screen = rootObj.GetComponent<UIView>();
				if (screen) {
					var cam = screen.GetTopmostCanvas().rootCanvas.worldCamera;
					if (cam && !cam.HasComponent<UIScreenLayer>()) {
						tm.Add(cam.transform);
						cameras.Add(cam);
					}
				}
			}
			
			UpdateLayersPositions(tm);

			var baseCamera = GameObject.Find(EditorCameraName);
			Debug.Assert(baseCamera != null);

			var baseCam = baseCamera.GetExistingComponent<UniversalAdditionalCameraData>();
			baseCam.cameraStack.Clear();
			baseCam.cameraStack.AddRange(cameras);

			OnHierarchyChanged();
		}

		private static void SetupAdditional(UIScreenLayer layer, string prefab) {
			var camPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
			var cam = ((GameObject)PrefabUtility.InstantiatePrefab(camPrefab, layer.transform)).GetComponent<Camera>();
			cam.name = $"{layer.name} - Cam";
			cam.nearClipPlane = -CameraZDepth * 0.5f;
			cam.farClipPlane = CameraZDepth * 0.5f;
			
			cam.ResetPRS();

			layer._camera = cam;
		}

		public static void SetupMainCamera(GameObject screen) {
			var camPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(UICamPrefab);
			var cam = ((GameObject)PrefabUtility.InstantiatePrefab(camPrefab)).GetComponent<Camera>();
			cam.name = $"{screen.name}Cam";
			cam.transform.parent = null;
			cam.nearClipPlane = -CameraZDepth * 0.5f;
			cam.farClipPlane = CameraZDepth * 0.5f;
			var canvas = screen.GetTopmostCanvas().rootCanvas;
			canvas.renderMode = RenderMode.ScreenSpaceCamera;
			canvas.worldCamera = cam;
			canvas.planeDistance = 0;
			
			cam.ResetPRS();

			if (!screen.scene.IsValid()) return;
			ArrangeLayers(screen.scene.GetRootGameObjects());
		}
	}

}

#endif