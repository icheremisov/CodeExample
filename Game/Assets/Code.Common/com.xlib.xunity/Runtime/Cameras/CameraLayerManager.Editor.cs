#if UNITY_EDITOR

using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Unity.Cameras {

	public partial class CameraLayerManager {
	
		[SerializeField, ShowIf("@UnityEngine.Application.isPlaying"), TextArea(10, 20)] private string _stack;
		[SerializeField, ShowIf("@UnityEngine.Application.isPlaying"), ListDrawerSettings(DefaultExpandedState = true, IsReadOnly = true)] private Camera[] _cameras;

		private void UpdateDebugInfo() {

			_stack = GetLayers().Select(GetDesc).JoinToString('\n');
			_cameras = _baseCameraData.cameraStack.ToArray();
		}
		
		private static string GetName(ICameraLayer layer) {
			if (layer == null) return "[<null>]";

			var fs = layer.Fullscreen ? " <FULLSCREEN>" : "";
			
			if (layer is MonoBehaviour mono) return $"[{mono.GetFullPath()} {layer.Layer}@{layer.Priority}{fs}]";
			
			var cam = layer.Camera.FirstOrDefault();
			if (cam) return $"[cam={cam.GetFullPath()} {layer.Layer}@{layer.Priority}{fs}]";
			
			return $"[{layer.GetType().FullName} {layer.Layer}@{layer.Priority}{fs}]";
		}

		private static string GetDesc(ICameraLayer layer) {
			if (layer == null)
				return string.Empty;

			var off = layer.Camera.Any(x => x && x.enabled) ? "" : "<OFF> ";
			return $"{off}{GetName(layer)}";
		}

		
		
	}

}


#endif