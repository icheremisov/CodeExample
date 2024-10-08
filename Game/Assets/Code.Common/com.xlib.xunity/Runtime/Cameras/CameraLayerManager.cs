using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LeTai.Asset.TranslucentImage;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using XLib.Core.AsyncEx;
using XLib.Core.Utils;
using XLib.Unity.Scene;
using XLib.Unity.Scene.ScreenTransition;

namespace XLib.Unity.Cameras {

	public partial class CameraLayerManager : Singleton<CameraLayerManager> {
		[SerializeField, Required] private Camera _baseCamera;
		[SerializeField, Required] private Material _blurMaterial;
		[SerializeField, Required] private Material _transitionMaterial;

		private UniversalAdditionalCameraData _baseCameraData;

		private readonly Dictionary<CameraLayerName, List<LayerData>> _layers = new(8);
		private int _addedOrderIndex;

		public event Action LayersChanged;

		private readonly AsyncLock _locker = new();
		private bool _updating;

		private readonly struct LayerData {
			public int Order { get; }
			public ICameraLayer Layer { get; }

			public LayerData(ICameraLayer layer, int order) {
				Order = order;
				Layer = layer;
			}
		}

		public IEnumerable<ICameraLayer> GetLayers() => Enums.Values<CameraLayerName>().SelectMany(GetLayers);

		public IEnumerable<ICameraLayer> GetLayers(CameraLayerName layer) =>
			_layers.TryGetValue(layer, out var result) ? result.Select(x => x.Layer) : Enumerable.Empty<ICameraLayer>();

		protected override void Awake() {
			base.Awake();

			Debug.Assert(_baseCamera != null);
			Debug.Assert(_blurMaterial != null);

			_baseCameraData = _baseCamera.GetExistingComponent<UniversalAdditionalCameraData>();
		}

		public async UniTask RegisterLayer(ICameraLayer layer) {
			if (layer == null) return;

			layer.Camera.SetEnabled(false);
			
			var layers = _layers.GetOrAddValue(layer.Layer, () => new List<LayerData>(4));
			if (layers.Any(data => data.Layer == layer)) return;
			
			layers.AddOnce(new LayerData(layer, ++_addedOrderIndex));
			layers.Sort(LayerOrderSort);

			await UpdateStack();
		}

		public async UniTask UnregisterLayer(ICameraLayer layer) {
			if (layer == null) return;

			await DisableLayer(layer);

			var layers = _layers.FirstOrDefault(layer.Layer);
			if (layers?.RemoveAll(x => x.Layer == layer) > 0) await UpdateStack();
		}

		private static int LayerOrderSort(LayerData a, LayerData b) {
			// ReSharper disable once JoinDeclarationAndInitializer
			int order;

			order = a.Layer.Layer.CompareTo(b.Layer.Layer);
			if (order != 0) return order;

			order = a.Layer.Priority.CompareTo(b.Layer.Priority);
			if (order != 0) return order;

			order = a.Order.CompareTo(b.Order);
			return order;
		}

		public void BeginUpdate() {
			_updating = true;
		}

		public async UniTask EndUpdate() {
			if (_updating) {
				_updating = false;
				await UpdateStack();
			}
		}

		[Button("Update Stack")]
		private void UpdateStackButton() => UpdateStack().Forget();
		
		public async UniTask UpdateStack() {
			if (_updating) return;

			using var _ = await _locker.LockAsync();
			
			foreach (var layerName in Enums.Values<CameraLayerName>()) _layers.FirstOrDefault(layerName)?.Sort(LayerOrderSort);

			var ctx = new BuildContext(_baseCameraData, _blurMaterial, _transitionMaterial);
			await ctx.Build(GetLayers());

#if UNITY_EDITOR
			UpdateDebugInfo();
#endif

			LayersChanged?.Invoke();
		}

		private class BuildContext {
			private readonly UniversalAdditionalCameraData _baseCameraData;
			private readonly Material _blurMaterial;
			private readonly Material _transitionMaterial;
			private bool _camerasEnabled;
			private int _depth = 1;

			private TranslucentImageSource _lastBlurSource;
			private ScreenTransitionSource _lastTransitionSource;

			public BuildContext(UniversalAdditionalCameraData baseCameraData, Material blurMaterial, Material transitionMaterial) {
				_baseCameraData = baseCameraData;
				_blurMaterial = blurMaterial;
				_transitionMaterial = transitionMaterial;
			}

			public async UniTask Build(IEnumerable<ICameraLayer> layers) {
				var layersList = layers.OrderBy(x => x.Priority).ToArray();
				
				var cameras = layersList.SelectMany(layer => layer.Camera);
				_baseCameraData.cameraStack.RemoveAll(c => !cameras.Contains(c));
				
				var fullscreenLayer = layersList.LastOrDefault(x => x.Fullscreen);
				_camerasEnabled = fullscreenLayer == null;

				var tasks = new List<UniTask>();
				
				foreach (var layer in layersList) {
					if (!_camerasEnabled) {
						if (layer != fullscreenLayer) {
							tasks.Add(DisableLayer(layer));
							continue;
						}

						await UniTask.WhenAll(tasks);
						tasks.Clear();
						
						_camerasEnabled = true;
					}

					if (layer != null) {
						if (layer.BlurSourceCampaignMapHack) _lastBlurSource = layer.BlurSourceCampaignMapHack;
						if (layer.TransitionSourceCampaignMapHack) _lastTransitionSource = layer.TransitionSourceCampaignMapHack;
					}

					tasks.Add(EnableLayer(layer, _lastBlurSource, _lastTransitionSource));

					if (layer == null) continue;
					
					if (layer.BlurSource) _lastBlurSource = layer.BlurSource;
					if (layer.TransitionSource) _lastTransitionSource = layer.TransitionSource;
				}
				
				await UniTask.WhenAll(tasks);
			}

			private async UniTask EnableLayer(ICameraLayer layer, TranslucentImageSource blurSource, ScreenTransitionSource transitionSource) {
				foreach (var cam in layer.Camera) {
					if (!cam) continue;

					cam.stereoTargetEye = StereoTargetEyeMask.Both;

					var camData = cam.GetExistingComponent<UniversalAdditionalCameraData>();
					cam.depth = ++_depth;
					camData.renderType = CameraRenderType.Overlay;

					if (_baseCameraData.cameraStack.Contains(cam)) _baseCameraData.cameraStack.Remove(cam);
					_baseCameraData.cameraStack.Add(cam);
					cam.enabled = true;
				}

				Material blurMaterialInstance = null;
				foreach (var blur in layer.BlurTarget.Where(x => x != null)) {
					blur.SetSource(blurSource);
					if (blurSource == null) continue;

					if (blur.material != null && blur.material.name == blurSource.name) continue;

					if (!blurMaterialInstance) blurMaterialInstance = new Material(_blurMaterial) { name = blurSource.name };
					blur.material = blurMaterialInstance;
				}
				
				Material transitionMaterialInstance = null;
				foreach (var transition in layer.TransitionTarget.Where(x => x != null)) {
					transition.SetSource(transitionSource);
					if (transitionSource == null) continue;

					if (transition.material != null && transition.material.name == transitionSource.name) continue;

					if (!transitionMaterialInstance) transitionMaterialInstance = new Material(_transitionMaterial) { name = transitionSource.name };
					transition.material = transitionMaterialInstance;
				}

				await layer.VisibleChanged(true);
			}
			
			private UniTask DisableLayer(ICameraLayer layer) => DisableLayerInternal(_baseCameraData, layer);
		}

		private UniTask DisableLayer(ICameraLayer layer) => DisableLayerInternal(_baseCameraData, layer);
		
		private static async UniTask DisableLayerInternal(UniversalAdditionalCameraData baseCameraData, ICameraLayer layer) {
			await layer.VisibleChanged(false);
			var cameras = layer.Camera.ToArray();
			cameras.ForEach(c => baseCameraData.cameraStack.Remove(c));
			cameras.SetEnabled(false);
		}
	}

}