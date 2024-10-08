using System;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace XLib.Unity.Scene {

	public static class SceneManagerHelper {
		private class SceneUnloader : IDisposable {
			private (string path, bool loaded)[] _loadedScenes;
			private UnityEngine.SceneManagement.Scene _activeScene;

			public SceneUnloader() {
				_activeScene = SceneManager.GetActiveScene();
				var countLoaded = SceneManager.loadedSceneCount;
				_loadedScenes = new (string path, bool loaded)[countLoaded];
				for (var i = 0; i < countLoaded; i++) {
					_loadedScenes[i].path = SceneManager.GetSceneAt(i).path;
					_loadedScenes[i].loaded = SceneManager.GetSceneAt(i).isLoaded;
				}

				_loadedScenes = _loadedScenes.Where(x => x.path.IsNotNullOrEmpty()).ToArray();
			}

			public void Dispose() {
				for (var index = 0; index < _loadedScenes.Length; index++) {
					var info = _loadedScenes[index];
					EditorSceneManager.OpenScene(info.path, index == 0 ? OpenSceneMode.Single : (info.loaded ? OpenSceneMode.Additive : OpenSceneMode.AdditiveWithoutLoading));
				}

				if (_activeScene.path.IsNotNullOrEmpty() && _activeScene.isLoaded) SceneManager.SetActiveScene(_activeScene);
			}
		}

		public static IDisposable UnloadAllScenes() => new SceneUnloader();
	}

}