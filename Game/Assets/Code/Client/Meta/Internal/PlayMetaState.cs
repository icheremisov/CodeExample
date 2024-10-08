using System;
using System.Collections.Generic;
using System.Threading;
using Client.Core.Common.Contracts;
using Client.Core.Common.UI.SystemDialog;
using Client.Core.GameStates.Contracts;
using Client.Definitions;
using Client.Ecs.Core;
using Client.Ecs.Core.Contracts;
using Client.Meta.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.Assets;
using XLib.Configs.Contracts;
using XLib.Core.Utils;
using XLib.States.Contracts;
using XLib.UI.Contracts;
using XLib.Unity.Core;

namespace Client.Meta.Internal {

	public class PlayMetaState : IGameState, IEnterState {
		private readonly IEcsRunner _ecsRunner;
		private readonly ILoadingScreen _loadingScreen;
		private readonly ISceneLoader _sceneLoader;
		private readonly IScreenManager _screenManager;
		private readonly IGameStateMachine _gameStateMachine;
		private readonly IGameDatabase _gameDatabase;
		private readonly GlobalContext _globalContext;
		private SceneInstanceLoader _metaSceneInstanceLoader;

		public PlayMetaState(
			ILoadingScreen loadingScreen,
			ISceneLoader sceneLoader,
			IScreenManager screenManager,
			IEcsRunner ecsRunner,
			IGameStateMachine gameStateMachine,
			IGameDatabase gameDatabase,
			GlobalContext globalContext) {
			_loadingScreen = loadingScreen;
			_sceneLoader = sceneLoader;
			_screenManager = screenManager;
			_ecsRunner = ecsRunner;
			_gameStateMachine = gameStateMachine;
			_gameDatabase = gameDatabase;
			_globalContext = globalContext;
		}

		public async UniTask OnEnterAsync(CancellationToken ct) {
			// initialize game classes
			await AsyncInitializableHelper.InitializeAsync(ct);

			_loadingScreen.Report(1.0f);

#if UNITY_EDITOR
			// if (GameLoader.HasParams<MissionStartupParams>()) {
			// 	_gameStateMachine.EnterAsync<Tools.TestMissionState>(ct).Forget();
			// 	return;
			// }
#endif

			_ecsRunner.IsPaused = false;
			_ecsRunner.Start(FeaturesEcs.Feature.Global);

			Debug.Assert(_metaSceneInstanceLoader == null);
			
			_metaSceneInstanceLoader = await _sceneLoader.LoadSceneAsync(GlobalDefinition.Instance.MetaMainScene, _loadingScreen, ct);
			await _screenManager.Screen<MetaGameScreen>().Open();
			var screen = await _screenManager.GetUIScreen<MetaGameScreen>();

			// var startupParamsGraphPreview = GameLoader.HasParams<GraphPreviewStartupParams>();
			// var startupParamsSingleMission = GameLoader.HasParams<SingleMissionStartupParams>();
			
			// await screen.TrySwitchToScreen();
			
			await _loadingScreen.HideAsync(false, ct);

			// if (startupParamsGraphPreview) {
			// 	await UniTask.NextFrame(ct);
			//
			// 	var arena = _gameDatabase.Get<ArenaDefinition>(GameLoader.GetParams<GraphPreviewStartupParams>().ArenaConfigId);
			// 	screen.CreateGraphPreview(arena).Forget();
			// }
			
			// _purchasesController.HandlePendingPurchases(true);
		}

		public async UniTask OnExitAsync(CancellationToken ct) {
			// _purchasesController.HandlePendingPurchases(false);

			_ecsRunner.Stop(FeaturesEcs.Feature.Global);
			_ecsRunner.IsPaused = true;

			if (_metaSceneInstanceLoader != null)
			{
				await _metaSceneInstanceLoader.UnloadAsync();
				_metaSceneInstanceLoader = null;
			}
			
			await _screenManager.UnloadAllScreens(new List<Type>() {
				TypeOf<SystemDialogScreen>.Raw,
				TypeOf<GameDialogScreen>.Raw
			});
		}
	}

}