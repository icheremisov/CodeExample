using System.Linq;
using UnityEngine;
using XLib.Unity.Utils;

namespace XLib.Unity.FX {

	public static class ScreenCapture {
		public static Texture2D MakeScreenShot(Texture2D result, params Camera[] ignoreCameras) {
			MakeScreenShot(ref result);
			return result;
		}

		public static void MakeScreenShot(ref Texture2D result, params Camera[] ignoreCameras) {
			var sz = GfxUtils.ScreenSize;
			var textureSizeW = sz.x;
			var textureSizeH = sz.y;

			if (result == null || result.width != textureSizeW || result.height != textureSizeH) {
				result = new Texture2D(textureSizeW, textureSizeH, TextureFormat.RGB24, false)
				{
					filterMode = FilterMode.Bilinear
				};
			}

			var rt = RenderTexture.GetTemporary(textureSizeW, textureSizeH);

			RenderCameras(rt, ignoreCameras);

			RenderTexture.active = rt;
			result.ReadPixels(new Rect(0, 0, textureSizeW, textureSizeH), 0, 0);
			result.Apply();

			RenderTexture.active = null;

			RenderTexture.ReleaseTemporary(rt);
		}

		private static void RenderCameras(RenderTexture rt, params Camera[] ignoreCameras) {
			rt.DiscardContents();

			var cameras = Camera.allCameras
				.Where(x => x.enabled && x.targetTexture == null && !ignoreCameras.Contains(x))
				.OrderByDescending(x => x.depth)
				.ToArray();

			foreach (var c in cameras) {
				c.targetTexture = rt;
				c.Render();
				c.targetTexture = null;
			}
		}
	}

}