using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LeTai.Asset.TranslucentImage;
using UnityEngine;
using XLib.Unity.Scene.ScreenTransition;

namespace XLib.Unity.Cameras {

	public abstract class CameraLayerBase : MonoBehaviour, ICameraLayer {
		
		[SerializeField] private CameraLayerName _layer = CameraLayerName.World;
		[SerializeField] private int _priority;
		[SerializeField] private bool _fullscreen;
		
		public CameraLayerName Layer => _layer;
		public bool Fullscreen => _fullscreen;
		public int Priority => _priority;
		
		public abstract IEnumerable<Camera> Camera { get; }
		public abstract TranslucentImageSource BlurSource { get; }
		public abstract IEnumerable<TranslucentImage> BlurTarget { get; }
		public abstract ScreenTransitionSource TransitionSource { get; }
		public abstract IEnumerable<ScreenTransitionImage> TransitionTarget { get; }

		private void OnEnable() {
			Debug.Assert(CameraLayerManager.S != null, $"{this.GetFullPath()} - {nameof(CameraLayerManager)} required!");
			CameraLayerManager.S.RegisterLayer(this).Forget();
		}

		private void OnDisable() {
			if (CameraLayerManager.S) CameraLayerManager.S.UnregisterLayer(this).Forget();
		}

		public void SetFullScreen(bool fullscreen) {
			_fullscreen = fullscreen;
			if (CameraLayerManager.S && isActiveAndEnabled) CameraLayerManager.S.UpdateStack().Forget();
		}
	}

}