using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using XLib.Assets;
using XLib.Core.AsyncEx;
using XLib.UI.Types;
using XLib.UI.Views;
using XLib.Unity.Core;
using Zenject;
using Object = UnityEngine.Object;

namespace XLib.UI.Internal {

	internal class UIScreenLoader : IContainerListener {
		private DiContainer FindContainer() => _containers.Last();
		private readonly List<DiContainer> _containers = new(4);

		public Dictionary<Type, UIScreenInstance> LoadedScreens => _loadedScreens;
		private readonly Dictionary<Type, UIScreenInstance> _loadedScreens = new(16);

		private readonly AsyncLock _loadingLock = new();

		public UIScreenInstance GetOrDefault(Type screenType) => _loadedScreens.FirstOrDefault(screenType);

		public async UniTask<UIScreenInstance> GetOrLoadScreenView(Type screenType) {
			await LoadIfNotLoaded(screenType);
			return GetOrDefault(screenType);
		}

		public bool ScreenIsLoaded(Type screenType) => _loadedScreens.ContainsKey(screenType);

		private async UniTask LoadIfNotLoaded(Type screenType) {
			if (_loadedScreens.ContainsKey(screenType)) return;

			using var _ = await _loadingLock.LockAsync();
			if (_loadedScreens.ContainsKey(screenType)) return;

			var sceneName = GetScreenParentSceneName(screenType);
			var sceneLoader = new SceneInstanceLoader(sceneName);
			await sceneLoader.LoadAsync(LoadSceneMode.Additive);
			
			TryRemoveAllUnwantedGos(sceneLoader.Scene);

			(var view, var mainLayer, var allLayers) = UnpackScene(sceneLoader.Scene);
			if (view == null || view.GetType() != screenType) {
				Debug.LogError($"There is no screen of type:{screenType.Name} in scene:{sceneName}");
				return;
			}

			var screenInstanceType = view.GetType();
			if (_loadedScreens.TryGetValue(screenInstanceType, out var oldScreen)) {
				Debug.LogError($"Scene with name:{sceneName} contains screen with whe same type:{screenInstanceType} that already loaded in scene {oldScreen.Scene.name}");
				return;
			}

			var screenInfo = new UIScreenInstance(view, sceneLoader, mainLayer, allLayers);

			_loadedScreens.Add(view.GetType(), screenInfo);
			UpdateScreenPositions();

			InjectToWidgets(sceneLoader.Scene);
		}

		public async UniTask Unload(Type screenType) {
			if (!_loadedScreens.ContainsKey(screenType)) return;

			using var _ = await _loadingLock.LockAsync();
			if (!_loadedScreens.TryGetValue(screenType, out var screenContainerInfo)) return;
			await screenContainerInfo.SceneLoader.UnloadAsync();
			_loadedScreens.Remove(screenType);
		}

		public async UniTask UnloadAll(IList<Type> exceptList) {
			foreach (var type in _loadedScreens.Keys.Where(x => !exceptList.Contains(x)).ToArray()) await Unload(type);
		}

		public void UpdateScreenPositions() => UIScreenLayer.UpdateLayersPositions(LoadedScreens.Values.SelectMany(x => x.ScreenLayers));

		private void InjectToWidgets(Scene scene) {
			var container = FindContainer();
			foreach (var parentGo in scene.GetRootGameObjects()) container.InjectGameObject(parentGo);
		}

		private void TryRemoveAllUnwantedGos(Scene scene) {
			foreach (var rootGameObject in scene.GetRootGameObjects().ToArray()) {
				TryRemoveAllUnwantedGosOfType<Light>(rootGameObject);
				TryRemoveAllUnwantedGosOfType<EventSystem>(rootGameObject);
			}
		}

		private void TryRemoveAllUnwantedGosOfType<T>(GameObject parent) where T : Component {
			if (parent == null) return;

			var gameObjectsToRemove = parent.GetComponentsInChildren<T>(true).SelectToArray(x => x.gameObject).Distinct().ToArray();
			foreach (var gameObject in gameObjectsToRemove) {
				Object.DestroyImmediate(gameObject);
			}
		}

		private static (UIView, UIScreenLayer, UIScreenLayer[]) UnpackScene(Scene scene) {
			var layers = new List<UIScreenLayer>(1);
			UIView view = null;
			var rootGameObjects = scene.GetRootGameObjects();

			foreach (var gObject in rootGameObjects.Where(x => !x.hideFlags.Has(HideFlags.DontSave))) {
				var objectView = gObject.GetComponent<UIView>();
				if (objectView) {
					if (!view)
						view = objectView;
					else
						Debug.LogError($"Only one screen instance supported at this moment in scene. duplicated type:{objectView.GetType().Name} scene:{scene.name}");

					continue;
				}

				var screenPart = gObject.GetComponent<UIScreenLayer>();
				if (!screenPart && !gObject.HasComponent<Camera>() && gObject.activeSelf) {
					Debug.LogError($"Invalid secondary game object found ({gObject.GetFullPath()}) - must contains {nameof(UIScreenLayer)} component on root object - will be deactivated! scene:{scene.name}");
					gObject.SetActive(false);
				}

				if (screenPart) {
					if (screenPart.Camera != null)
						layers.Add(screenPart);
					else
						Debug.LogError($"Camera does not set - gameObject will be ignored {screenPart.GetFullPath()} scene:{scene.name}");
				}
			}

			if (!view) {
				Debug.LogError($"There is no instances of any screen in scene:{scene.name}");
				return (null, null, Array.Empty<UIScreenLayer>());
			}

			var viewCamera = view.GetTopmostCanvas().rootCanvas.worldCamera;

			var mainPart = viewCamera.GetExistingComponent<UIScreenLayer>();
			mainPart.LinkedObject = view.gameObject;

			return (view, mainPart, layers.OrderBy(x => x.SceneOrder).ToArray());
		}

		private string GetScreenParentSceneName(Type screenType) {
			var sceneNameAttr = screenType.GetCustomAttribute<ScreenParentSceneNameAttribute>();
			var sceneName = sceneNameAttr != null ? sceneNameAttr.SceneName : screenType.Name;
			return sceneName;
		}

		public void OnInstall(DiContainer container) {
			if (_containers.Contains(container)) return;

			_containers.Add(container);
		}

		public void OnUninstall(DiContainer container) {
			_containers.Remove(container);
		}

		public GameObject InstantiateUIPrefab(Object prefab, Transform parent) {
			var container = FindContainer();
			return container.InstantiatePrefab(prefab, parent);
		}
	}

}