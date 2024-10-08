using System.Threading;
using Client.App.Contracts;
using Client.Core.Common.Configs;
using Client.Core.Common.Contracts;
using Client.Core.Common.Scene;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.Assets;
using XLib.Assets.Contracts;
using XLib.UI.Contracts;
using XLib.Unity.Core;
using XLib.Unity.Installers;

namespace Client.App.Internal {

	internal class ApplicationLoader : IApplicationLoader {

		private readonly IAssetProvider _assetProvider;
		private readonly CoreConfig _coreConfig;
		private readonly LazyBinding<IUnityApplication> _unityApplication;
		private readonly ILoadingScreen _loadingScreen;
		private readonly ISceneLoader _sceneLoader;
		private readonly IScreenManager _screenManager;
		private SceneInstanceLoader _mainScene;

		public ApplicationLoader(
			ILoadingScreen loadingScreen,
			IAssetProvider assetProvider,
			ISceneLoader sceneLoader,
			CoreConfig coreConfig,
			LazyBinding<IUnityApplication> unityApplication,
			IScreenManager screenManager) {
			_loadingScreen = loadingScreen;
			_assetProvider = assetProvider;
			_sceneLoader = sceneLoader;
			_coreConfig = coreConfig;
			_unityApplication = unityApplication;
			_screenManager = screenManager;
		}
		
		public async UniTask Play() {
			
#if FEATURE_PRODUCTION
			AppLogger.LogWarning("PROD Game Config");
#elif FEATURE_DEMO
			AppLogger.LogWarning("DEMO Game Config");
#elif FEATURE_STAGING
			AppLogger.LogWarning("STAGING Game Config");
#else
			AppLogger.LogWarning("DEV Game Config");
#endif
			
			// initialize one-time classes
			AppLogger.Log("InitGame");
			await InitGame();

			while (true) {
				
				// load main scene and init other classes
				AppLogger.Log("LoadGame");
				await LoadGame();

				if (!_unityApplication.HasValue) {
					AppLogger.LogError("Game stopped due initialization error - see log for details.");
					break;
				}
				
				// call main loop
				AppLogger.Log("MainLoop");
				await _unityApplication.Value.MainLoop(CancellationToken.None);
							
				// unload main scene
				AppLogger.Log("UnloadGame");
				await UnloadGame();
			}
				
		}

		private async UniTask InitGame() {

			await _loadingScreen.ShowAsync(true);
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			_loadingScreen.Report(0.05f);
			
			// initialize global game classes
			await AsyncInitializableHelper.InitializeAsync();
		}

		private async UniTask LoadGame() {
			await _loadingScreen.ShowAsync();
			_loadingScreen.Report(0.1f);

			// initialize system and preload sprites
			// await _assetProvider.InitializeAsync(CancellationToken.None);

			// skip for progressbar animation
			await UniTask.NextFrame();
			
			// load main scene with all bindings
			_mainScene = await _sceneLoader.LoadSceneAsync(_coreConfig.MainSceneName, _loadingScreen.Remap(0.1f, 0.2f));
			
			// await for binding happened and installers are resolved
			await UniTask.NextFrame();

			// initialize classes from MainScene
			await AsyncInitializableHelper.InitializeAsync();
			
			_loadingScreen.Report(0.3f);
		}
		
		private async UniTask UnloadGame() {
			await _loadingScreen.ShowAsync();
			await _sceneLoader.UnloadSceneAsync(_mainScene, _loadingScreen.Remap(0.0f, 0.1f));
		}

	}

}