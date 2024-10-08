using System.Collections.Generic;
using System.Linq;
using LeTai.Asset.TranslucentImage;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Unity.Cameras;
using XLib.Unity.Scene.ScreenTransition;

namespace XLib.UI.Internal {

	public partial class UIScreenLayer : MonoBehaviour {

		public static bool HideScreensCheat = false;
		
		private const float CameraZDepth = 100.0f;
		private const float ScreensSpacing = 50.0f;

		[SerializeField, Required] private Camera _camera;
		[SerializeField, ReadOnly] private int _sceneOrder;

		[Space]
		[SerializeField] private bool _overrideSorting;
		[SerializeField, ShowIf(nameof(_overrideSorting))] private CameraLayerName _layer = CameraLayerName.UI;
		[SerializeField, ShowIf(nameof(_overrideSorting))] private int _priority;

		public int SceneOrder => _sceneOrder;
		
		public bool OverrideSorting => _overrideSorting;
		public CameraLayerName Layer => _layer;
		public int Priority => _priority;
		
		public Camera Camera { get => _camera; set => _camera = value; }
		public GameObject LinkedObject { get; set; }

		public IEnumerable<TranslucentImage> BlurTarget => GetComponentsInChildren<TranslucentImage>(true);
		public IEnumerable<ScreenTransitionImage> TransitionTarget => GetComponentsInChildren<ScreenTransitionImage>(true);
		
		public void SetVisible(bool v) {
			if (LinkedObject) LinkedObject.SetActive(v);
			else this.SetActive(v);
		}

		public static void UpdateLayersPositions(IEnumerable<UIScreenLayer> layers) => UpdateLayersPositions(layers.Where(x => x != null).Select(x => x.transform));

		public static void UpdateLayersPositions(IEnumerable<Transform> layers) {
			const float delta = ScreensSpacing + CameraZDepth * 0.5f;
			var z = delta;

			foreach (var layer in layers) {
				if (layer == null) continue;
				layer.localPosition = layer.localPosition.ToXY0(-z);
				z += delta;
			}
		}

		
	}

}