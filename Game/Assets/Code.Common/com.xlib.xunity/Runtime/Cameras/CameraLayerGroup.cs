using System.Collections.Generic;
using System.Linq;
using LeTai.Asset.TranslucentImage;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Unity.Scene.ScreenTransition;

namespace XLib.Unity.Cameras {

	public class CameraLayerGroup : CameraLayerBase {

		[SerializeField, Required] private Camera[] _cameras;
		[SerializeField, Required] private TranslucentImage[] _blurTargets;
		[SerializeField, Required] private ScreenTransitionImage[] _transitionTargets;
		
		private TranslucentImageSource _blurSource;
		private ScreenTransitionSource _transitionSource;

		public override IEnumerable<Camera> Camera { get => _cameras; }
		public override TranslucentImageSource BlurSource => _blurSource;
		public override IEnumerable<TranslucentImage> BlurTarget => _blurTargets;
		public override ScreenTransitionSource TransitionSource => _transitionSource;
		public override IEnumerable<ScreenTransitionImage> TransitionTarget => _transitionTargets;

		private void Awake() {
			_cameras = _cameras.SelectNotNull().ToArray();
			_blurTargets = _blurTargets.SelectNotNull().ToArray();
			_blurSource = _cameras.SelectNotNull(x => x.GetComponent<TranslucentImageSource>()).LastOrDefault();
			_transitionTargets = _transitionTargets.SelectNotNull().ToArray();
			_transitionSource = _cameras.SelectNotNull(x => x.GetComponent<ScreenTransitionSource>()).LastOrDefault();
		}
	}

}