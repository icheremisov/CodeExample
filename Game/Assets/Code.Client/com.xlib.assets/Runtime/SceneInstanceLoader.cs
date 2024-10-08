using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;

namespace XLib.Assets {

	public class SceneInstanceLoader {
		private readonly string _sceneName;
		private readonly ZenjectSceneLoader _zenjectLoader;
		private Scene _scene;
		private SceneInstance? _instance;

		public string Name => _sceneName;
		public Scene Scene => _scene;

		public SceneInstanceLoader(string sceneName, ZenjectSceneLoader zenjectLoader = null) {
			_sceneName = sceneName;
			_zenjectLoader = zenjectLoader;
			_scene = SceneManager.GetSceneByName(_sceneName);
		}

		public async UniTask LoadAsync(LoadSceneMode mode, LoadSceneRelationship containerMode = LoadSceneRelationship.None, IProgress<float> progress = null) {
			if (_instance != null) return;

			if (!_scene.IsValid() || !_scene.isLoaded) {
				if(SceneManagerUtils.IsSceneInBuiltInSettings(_sceneName)){
					Debug.Log($"Load built-in scene: {_sceneName}");
					if (_zenjectLoader != null)
						await _zenjectLoader.LoadSceneAsync(_sceneName, mode, containerMode: containerMode).ToUniTask(progress);
					else
						await SceneManager.LoadSceneAsync(_sceneName, mode).ToUniTask(progress);
					_scene = SceneManager.GetSceneByName(_sceneName);
				}
				else {
					// _zenjectLoader?.PrepareForLoadScene(mode, null, null, containerMode);

					Debug.Log($"Load addressables scene: {_sceneName}");
					var loadSceneAsync = Addressables.LoadSceneAsync(_sceneName, mode);
					if (progress != null) {
						UniTask.WaitUntil(() => {
								if (loadSceneAsync.IsDone) return true;
								progress.Report(loadSceneAsync.PercentComplete);
								return false;
							})
							.Forget();
					}

					_instance = await loadSceneAsync.Task;
					_scene = _instance.Value.Scene;
				}
			}
		}

		public async UniTask UnloadAsync(IProgress<float> progress = null) {
			if (_instance == null && !_scene.isLoaded) return;

			Debug.Log($"Unload scene: {_sceneName}");
			if (_instance.HasValue) {
				var unloadSceneAsync = Addressables.UnloadSceneAsync(_instance.Value);
				if (progress != null) {
					UniTask.WaitUntil(() => {
							if (unloadSceneAsync.IsDone) return true;
							progress.Report(unloadSceneAsync.PercentComplete);
							return false;
						})
						.Forget();
				}

				await unloadSceneAsync.Task;
			}
			else
				await SceneManager.UnloadSceneAsync(_scene).ToUniTask(progress);
		}
	}

}