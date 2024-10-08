using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

namespace XLib.Assets {
	public static class SceneManagerUtils {
		private static List<string> _builtInSceneNames;
		public static bool IsSceneInBuiltInSettings(string sceneName) {
			if (_builtInSceneNames is null) {
				_builtInSceneNames = new List<string>();
				for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
					var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
					var name = Path.GetFileNameWithoutExtension(scenePath);
					_builtInSceneNames.Add(name);
				}
			}

			return _builtInSceneNames.Contains(sceneName);
		}
	}

}