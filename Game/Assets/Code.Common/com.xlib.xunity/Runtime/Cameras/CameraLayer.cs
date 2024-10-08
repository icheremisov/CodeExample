using System.Collections.Generic;
using System.Linq;
using LeTai.Asset.TranslucentImage;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Unity.Scene.ScreenTransition;

namespace XLib.Unity.Cameras {

	[RequireComponent(typeof(Camera))]
	public class CameraLayer : CameraLayerBase {
		[SerializeField, Required] private TranslucentImage[] _blurTargets;
		[SerializeField, Required] private ScreenTransitionImage[] _transitionTargets;
		
		private Camera _camera;
		private TranslucentImageSource _blurSource;
		private ScreenTransitionSource _transitionSource;

		public override IEnumerable<Camera> Camera { get { yield return _camera; } }
		public override TranslucentImageSource BlurSource => _blurSource;
		public override IEnumerable<TranslucentImage> BlurTarget => _blurTargets;
		public override ScreenTransitionSource TransitionSource => _transitionSource;
		public override IEnumerable<ScreenTransitionImage> TransitionTarget => _transitionTargets;

		private void Awake() {
			_camera = this.GetExistingComponent<Camera>();
			_blurSource = GetComponent<TranslucentImageSource>();
			_transitionSource = GetComponent<ScreenTransitionSource>();
			
			_blurTargets = _blurTargets.SelectNotNull().ToArray();
			_transitionTargets = _transitionTargets.SelectNotNull().ToArray();
		}
	}

}