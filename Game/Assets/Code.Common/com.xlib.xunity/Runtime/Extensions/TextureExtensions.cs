using UnityEngine;
using UnityEngine.Experimental.Rendering;

// ReSharper disable once CheckNamespace
public static class TextureExtensions {

	public static byte[] ExportScreenToPNG(this Camera camera, Vector3 cameraPosition) {
		var p = camera.transform.position;
		camera.transform.position = cameraPosition;

		try {
			return ExportScreenToPNG(camera);
		}
		finally {
			camera.transform.position = p;
		}
	}

	public static byte[] ExportScreenToPNG(this Camera camera) {
		camera.Render();

		var rt = camera.targetTexture;

		RenderTexture.active = rt;
		var result = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
		result.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

		RenderTexture.active = null;

		return result.EncodeToPNG();
	}

	public static Texture2D ExportToTexture(this RenderTexture rt) {
		var result = new Texture2D(rt.width, rt.height, rt.graphicsFormat, TextureCreationFlags.None);

		var prevRt = RenderTexture.active;
		RenderTexture.active = rt;
		result.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
		RenderTexture.active = prevRt;

		result.Apply();

		return result;
	}

	public static RenderTexture ExportToTempRT(this Texture2D tex, int depthBuffer = 24) {
		var result = RenderTexture.GetTemporary(tex.width, tex.height, depthBuffer);
		Graphics.Blit(tex, result);

		return result;
	}

	public static Texture2D GetScaled(this Texture2D tex, int newWidth, int newHeight) {
		var rt = RenderTexture.GetTemporary(newWidth, newHeight, 0);
		Graphics.Blit(tex, rt);

		var result = rt.ExportToTexture();

		RenderTexture.ReleaseTemporary(rt);

		return result;
	}

}