using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Client.Core.Common.Contracts;
using Client.Core.Common.Types;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using XLib.Assets;
using XLib.Assets.Contracts;
using XLib.Core.CommonTypes;
using XLib.Core.Utils;
using Zenject;

namespace Client.Core.Common.Internal {

	internal class SceneLoader : ISceneLoader {

		private static readonly Logger Logger = new(nameof(SceneLoader));

		private readonly IAssetProvider _assetProvider;
		private readonly ZenjectSceneLoader _zenjectSceneLoader;

		public SceneLoader(IAssetProvider assetProvider, ZenjectSceneLoader zenjectSceneLoader) {
			_assetProvider = assetProvider;
			_zenjectSceneLoader = zenjectSceneLoader;
		}

		public async Task<SceneInstanceLoader> LoadSceneAsync(string sceneName, IProgress<float> progressReporter = null, CancellationToken ct = default,
			SceneLoaderOptions options = SceneLoaderOptions.Default) {
			if (sceneName.IsNullOrEmpty()) throw new ArgumentNullException(nameof(sceneName));

			return (await LoadScenesAsync(new[] { sceneName }, progressReporter, ct, options)).First();
		}

		public async Task<List<SceneInstanceLoader>> LoadScenesAsync(string[] sceneNames, IProgress<float> progressReporter = null, CancellationToken ct = default,
			SceneLoaderOptions options = SceneLoaderOptions.Default) {
			var sceneStep = 1.0f / sceneNames.Length;
			var sceneIndex = 0;

			var loadSceneMode = LoadSceneMode.Additive;
			var containerSceneMode = LoadSceneRelationship.Child;
			
			Logger.Log($"Loading: {sceneNames.JoinToString()}");
			var sceneInstances = new List<SceneInstanceLoader>();
			foreach (var sceneName in sceneNames) {
				var progress = new RemapProgress(new RangeF(sceneStep * sceneIndex, sceneStep * (sceneIndex + 1)), progressReporter);
				var sceneInstance = new SceneInstanceLoader(sceneName, _zenjectSceneLoader);
				sceneInstances.Add(sceneInstance);
				await sceneInstance.LoadAsync(loadSceneMode, containerSceneMode, progress);
				await UniTask.NextFrame(ct);
				++sceneIndex;
			}

			return sceneInstances;
		}

		public UniTask UnloadSceneAsync(SceneInstanceLoader sceneInstance, IProgress<float> progressReporter = null, CancellationToken ct = default) {
			return UnloadScenesAsync(new List<SceneInstanceLoader>() { sceneInstance }, progressReporter, ct);
		}
		
		public async UniTask UnloadScenesAsync(List<SceneInstanceLoader> sceneInstances, IProgress<float> progressReporter = null, CancellationToken ct = default) {
			var sceneStep = 1.0f / sceneInstances.Count;
			var sceneIndex = 0;

			Logger.Log($"Unloading: {sceneInstances.Select(loader => loader.Name).JoinToString()}");

			foreach (var sceneInstance in sceneInstances.AsEnumerable().Reverse()) {
				var progress = new RemapProgress(new RangeF(sceneStep * sceneIndex, sceneStep * (sceneIndex + 1)), progressReporter);
				await sceneInstance.UnloadAsync(progress);
				await UniTask.NextFrame(ct);
				++sceneIndex;
			}
		}

	}

}