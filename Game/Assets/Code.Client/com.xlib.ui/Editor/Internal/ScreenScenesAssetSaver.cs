using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using XLib.Unity.Utils;

namespace XLib.UI.Internal {

	[InitializeOnLoad]
	internal static class ScreenScenesAssetSaver {
		private const string DefaultLightSettings = "Common Lighting Settings";

		static ScreenScenesAssetSaver() {
			EditorSceneManager.sceneSaving += CheckAndSaveScene;
			EditorSceneManager.sceneOpened += CheckAndSaveScene;
		}

		private static void CheckAndSaveScene(Scene scene, OpenSceneMode mode) {
			if (!Process(scene, scene.path)) return;
			EditorSceneManager.SaveScene(scene);
		}

		private static void CheckAndSaveScene(Scene scene, string path) {
			Process(scene, path);
		}

		public static void SetupLightSettings(Scene scene) {
			RenderSettings.skybox = null;
			RenderSettings.sun = null;
			RenderSettings.subtractiveShadowColor = new Color(0.42f, 0.478f, 0.627f); //realtime shadow color
			RenderSettings.ambientMode = AmbientMode.Flat; //Environment lightning ambient source
			RenderSettings.ambientSkyColor = new Color(0.212f, 0.227f, 0.259f); //Environment lightning ambient color
			RenderSettings.ambientIntensity = 1.0f; //Environment lightning ambient color intensity
			RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom; //Environment reflections Source
			RenderSettings.customReflectionTexture = null; //Environment reflections Cubemap
			//LightmapEditorSettings.reflectionCubemapCompression = ReflectionCubemapCompression.Auto; //Environment reflections Compression
			RenderSettings.reflectionIntensity = 1.0f; //Environment reflections Intensity multiplier
			RenderSettings.reflectionBounces = 1; //Environment reflections Bounces
			RenderSettings.fog = false;
			RenderSettings.haloStrength = 0.5f;
			RenderSettings.flareFadeSpeed = 3.0f;
			RenderSettings.flareStrength = 1.0f;
		}

		private static bool Process(Scene scene, string path) {
			if (!path.Contains("Assets/UI/")) return false;
			
			Lightmapping.TryGetLightingSettings(out var lightingSettings);
			if (lightingSettings != null && lightingSettings.name == DefaultLightSettings) return false;

			if (lightingSettings != null && lightingSettings.name != DefaultLightSettings) {
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(lightingSettings));
				Object.DestroyImmediate(lightingSettings);
			}

			Lightmapping.lightingSettings = EditorUtils.LoadExistingAsset<LightingSettings>(DefaultLightSettings);
			return true;
		}
	}

}