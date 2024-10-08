using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace XLib.Unity.Utils {

	/// <summary>
	///     common GFX tools
	/// </summary>
	public static class GfxUtils {

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public enum AspectRatio {

			Aspect_4_3 = 0,
			Aspect_16_10 = 1,
			Aspect_16_9 = 2,
			Aspect_18_9 = 3,
			Aspect_20_9 = 4
		}

		private static readonly float[] AspectValue = {
			4.0f / 3.0f,
			16.0f / 10.0f,
			16.0f / 9.0f,
			18.0f / 9.0f,
			20.0f / 9.0f
		};

		public static Vector2Int ScreenSize {
			get {
#if UNITY_EDITOR
				var screenSize = Handles.GetMainGameViewSize();
				return new Vector2Int((int)screenSize.x, (int)screenSize.y);
#else
				return new Vector2Int(Screen.width, Screen.height);
#endif
			}
		}
		
		public static Texture2D MakeTex(int width, int height, Color col) {
			var pix = new Color[width * height];
			for (var i = 0; i < pix.Length; ++i) {
				pix[i] = col;
			}

			var result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

		private static float CurrentAspectRatio() {
			var sz = ScreenSize;
			return sz.x > sz.y ? (float)sz.x / sz.y : (float)sz.y / sz.x;
		}

		/// <summary>
		///     return 0.0 if aspect is aspect1 or smaller, and 1.0 if aspect is ascpect2 or more
		/// </summary>
		public static float AspectRatioK(AspectRatio aspect1, AspectRatio aspect2) {
			if (aspect1 == aspect2) return 0.0f;

			var v1 = AspectValue[(int)aspect1];
			var v2 = AspectValue[(int)aspect2];

			return Mathf.Clamp01(Mathf.InverseLerp(v1, v2, CurrentAspectRatio()));
		}

	}

}