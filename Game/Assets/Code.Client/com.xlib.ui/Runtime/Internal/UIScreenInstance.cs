using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LeTai.Asset.TranslucentImage;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XLib.Assets;
using XLib.UI.Contracts;
using XLib.UI.Screens;
using XLib.UI.Types;
using XLib.UI.Views;
using XLib.Unity.Cameras;
using XLib.Unity.Scene.ScreenTransition;

namespace XLib.UI.Internal {

	internal class UIScreenInstance : ICameraLayer {
		public int VisualOrder { get; set; }

		public IUIScreen Screen { get; }
		public Scene Scene => _sceneLoader.Scene;
		public SceneInstanceLoader SceneLoader => _sceneLoader;

		private readonly Camera[] _persistentCameras;
		private readonly TranslucentImageSource _blurSource;
		private readonly TranslucentImage[] _blurImages;
		private readonly ScreenTransitionSource _transitionSource;
		private readonly ScreenTransitionImage[] _transitionImages;
		private readonly IBeforeScreenDynamicCameraProvider _cameraProvider;
		private readonly GraphicRaycaster[] _raycasters;
		private readonly CanvasGroup[] _groups;

		private readonly CameraLayerName _layer = CameraLayerName.UI;
		private readonly int? _customPriority;
		private readonly UIView _view;
		private readonly SceneInstanceLoader _sceneLoader;
		private readonly UIScreenLayer _mainLayer;

		public UIScreenLayer[] ScreenLayers { get; }

		CameraLayerName ICameraLayer.Layer => _layer;
		bool ICameraLayer.Fullscreen => Screen.Style.Has(ScreenStyle.FullScreen);
		int ICameraLayer.Priority => _customPriority ?? VisualOrder;
		IEnumerable<Camera> ICameraLayer.Camera {
			get {
				var result = _persistentCameras.AsEnumerable();

				if (_cameraProvider != null) result = (_cameraProvider.Cameras ?? Enumerable.Empty<Camera>()).Concat(result);
				if (UIScreenLayer.HideScreensCheat) result = result.Where(x => x != _mainLayer.Camera);
				return result;
			}
		}

		TranslucentImageSource ICameraLayer.BlurSourceCampaignMapHack => _cameraProvider?.BlurSourcePanelHack;
		ScreenTransitionSource ICameraLayer.TransitionSourceCampaignMapHack => _cameraProvider?.TransitionSourcePanelHack;

		TranslucentImageSource ICameraLayer.BlurSource => _blurSource;
		IEnumerable<TranslucentImage> ICameraLayer.BlurTarget => _blurImages;
		ScreenTransitionSource ICameraLayer.TransitionSource => _transitionSource;
		IEnumerable<ScreenTransitionImage> ICameraLayer.TransitionTarget => _transitionImages;

		public UIScreenInstance(UIView view, SceneInstanceLoader sceneLoader, UIScreenLayer mainLayer, UIScreenLayer[] screenLayers) {
			Screen = (IUIScreen)view;
			ScreenLayers = screenLayers;
			_view = view;
			_sceneLoader = sceneLoader;
			_mainLayer = mainLayer;
			_cameraProvider = view as IBeforeScreenDynamicCameraProvider;
			_raycasters = screenLayers.Select(x => x.gameObject)
				.Append(view.gameObject)
				.SelectMany(x => x.GetComponentsInChildren<GraphicRaycaster>(true))
				.Distinct()
				.ToArray();

			_groups = screenLayers.Select(x => x.gameObject)
				.Append(view.gameObject)
				.SelectMany(x => x.GetComponents<CanvasGroup>())
				.Distinct()
				.ToArray();

			if (mainLayer.OverrideSorting) {
				_layer = mainLayer.Layer;
				_customPriority = mainLayer.Priority;
			}

			Debug.Assert(ScreenLayers.Length > 0, $"Screen has no parts: {view.GetType().Name}; scene:{SceneLoader.Name}");
			_persistentCameras = screenLayers.SelectToArray(x => x.Camera);

			Debug.Assert(_persistentCameras.Length > 0, $"Screen has no cameras: {view.GetType().Name}; scene:{SceneLoader.Name}");

			_persistentCameras.SetEnabled(false);

			_blurSource = ScreenLayers.OrderByDescending(x => x.SceneOrder).SelectNotNull(x => x.Camera.GetComponent<TranslucentImageSource>()).FirstOrDefault();
			_blurImages = ScreenLayers.SelectMany(x => x.BlurTarget)
				.Concat(view.transform.GetComponentsInChildren<TranslucentImage>(true))
				.Distinct()
				.ToArray();
			
			_transitionSource = ScreenLayers.OrderByDescending(x => x.SceneOrder).SelectNotNull(x => x.Camera.GetComponent<ScreenTransitionSource>()).FirstOrDefault();
			_transitionImages = ScreenLayers.SelectMany(x => x.TransitionTarget)
				.Concat(view.transform.GetComponentsInChildren<ScreenTransitionImage>(true))
				.Distinct()
				.ToArray();
		}

		public async UniTask VisibleChanged(bool visible) {
			_raycasters.SetEnabled(visible);
			_groups.ForEach(x => {
				if (x) x.blocksRaycasts = visible;
			});
			
			if (!_view) return;
			
			if (visible) _view.SetActive(true);
			await _view.CamerasVisibleChanged(visible);
			if (!visible) _view.SetActive(false);
		}

		public override string ToString() => $"Screen '{Screen.GetType().Name}'";
	}

}