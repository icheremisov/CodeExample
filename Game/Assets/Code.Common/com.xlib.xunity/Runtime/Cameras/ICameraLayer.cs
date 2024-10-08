using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LeTai.Asset.TranslucentImage;
using UnityEngine;
using XLib.Unity.Scene.ScreenTransition;

namespace XLib.Unity.Cameras {

	public interface ICameraLayer {

		CameraLayerName Layer { get; }
		bool Fullscreen { get; }
		int Priority { get; }

		TranslucentImageSource BlurSourceCampaignMapHack => null;
		ScreenTransitionSource TransitionSourceCampaignMapHack => null;

		IEnumerable<Camera> Camera { get; }
		TranslucentImageSource BlurSource { get; }
		IEnumerable<TranslucentImage> BlurTarget { get; }
		ScreenTransitionSource TransitionSource { get; }
		IEnumerable<ScreenTransitionImage> TransitionTarget { get; }

		UniTask VisibleChanged(bool visible) => UniTask.CompletedTask;
	}

}