using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Client.Core.Common.Types;
using Cysharp.Threading.Tasks;
using XLib.Assets;

namespace Client.Core.Common.Contracts {

	/// <summary>
	///     load/unload scenes with Zenject binding support
	/// </summary>
	public interface ISceneLoader {

		/// <summary>
		///     load scene additive
		/// </summary>
		Task<SceneInstanceLoader> LoadSceneAsync(string sceneName, IProgress<float> progressReporter = null, CancellationToken ct = default,
			SceneLoaderOptions options = SceneLoaderOptions.Default);

		/// <summary>
		///     unload specific scene from memory
		/// </summary>
		UniTask UnloadSceneAsync(SceneInstanceLoader sceneInstance, IProgress<float> progressReporter = null, CancellationToken ct = default);

		/// <summary>
		///     load specific scenes
		/// </summary>
		Task<List<SceneInstanceLoader>> LoadScenesAsync(string[] sceneNames, IProgress<float> progressReporter = null, CancellationToken ct = default,
			SceneLoaderOptions options = SceneLoaderOptions.Default);

		/// <summary>
		///     unload scenes in reverse order
		/// </summary>
		UniTask UnloadScenesAsync(List<SceneInstanceLoader> sceneInstances, IProgress<float> progressReporter = null, CancellationToken ct = default);
		
	}

}