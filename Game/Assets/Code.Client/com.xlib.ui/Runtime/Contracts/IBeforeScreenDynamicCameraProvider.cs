using System;
using System.Collections.Generic;
using LeTai.Asset.TranslucentImage;
using UnityEngine;
using XLib.Unity.Scene.ScreenTransition;

namespace XLib.UI.Contracts {

	public interface IBeforeScreenDynamicCameraProvider {
		
		event Action CameraChanged;
		
		TranslucentImageSource BlurSourcePanelHack => null;
		ScreenTransitionSource TransitionSourcePanelHack => null;

		IEnumerable<Camera> Cameras { get; }
	}

}