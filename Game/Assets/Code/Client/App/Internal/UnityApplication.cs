using System;
using System.Threading;
using Client.App.Contracts;
using Client.Core.Common.Contracts;
using Client.Core.Common.UI.SystemDialog;
using Client.Core.GameFlow.Contracts;
using Client.Core.GameStates.Contracts;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using XLib.Assets.Contracts;
using XLib.BuildSystem;
using XLib.Configs.Contracts;
using XLib.Core.AsyncEx;
using XLib.Core.Utils;
using XLib.UI.ConnectionBlocker;
using XLib.UI.Contracts;
using XLib.UI.Types;
using XLib.Unity.LocalStorage;

namespace Client.App.Internal {

	[UsedImplicitly]
	internal partial class UnityApplication : IUnityApplication {
		private static readonly StoredValue<string> _playerDeviceId = new("PlayerDeviceId", null);

		private string GetPlayerDeviceId() {
			var value = _playerDeviceId.Value.IsNullOrEmpty() ? _playerDeviceId.Value = Guid.NewGuid().ToString() : _playerDeviceId.Value;
			_playerDeviceId.Save();
			return value;
		}

		private static readonly StoredValue<StartupGameParams> StartupParams = new("StartupGameParams", new StartupGameParams());
		public string DeviceId { get; private set; }

		private readonly IMetaFlowController _metaFlowController;
		private readonly ILevelFlowController _levelFlowController;
		private readonly IScreenManager _screenManager;
		private readonly ISystemDialog _systemDialog;
		// private readonly IGameApplication _logicApplication;
		// private readonly ISharedLogicService _sharedLogicService;
		private readonly IBlockerView _blockerView;
		// private readonly ITutorialProcessor _tutorialProcessor;
		private readonly ILoadingScreen _loadingScreen;
		private readonly IDataStorageProvider _dataStorageProvider;
		private readonly IGameStateMachine _gameStateMachine;
		private readonly GlobalContext _globalContext;
		private readonly IClientGameDatabaseProvider _gameDatabaseProvider;

		private readonly AsyncEvent _evRestartGame = new();
		private bool _gameStarted;
		private readonly IScreenLocker _screenLocker;
		// private readonly ITutorialView _tutorialView;
		// private readonly IBundlesCatalogService _bundlesCatalogService;
		private readonly IAssetProvider _assetProvider;

		public UnityApplication(
			IGameStateMachine gameStateMachine,
			IMetaFlowController metaFlowController,
			ILevelFlowController levelFlowController,
			IScreenManager screenManager,
			ISystemDialog systemDialog,
			// IGameApplication logicApplication,
			// ISharedLogicService sharedLogicService,
			IBlockerView blockerView,
			GlobalContext globalContext,
			// ITutorialProcessor tutorialProcessor,
			ILoadingScreen loadingScreen,
			IClientGameDatabaseProvider gameDatabaseProvider,
			IScreenLocker screenLocker,
			// ITutorialView tutorialView,
			IDataStorageProvider dataStorageProvider,
			// IBundlesCatalogService bundlesCatalogService,
			IAssetProvider assetProvider) {
			_gameStateMachine = gameStateMachine;
			_metaFlowController = metaFlowController;
			_levelFlowController = levelFlowController;
			_screenManager = screenManager;
			_systemDialog = systemDialog;
			// _logicApplication = logicApplication;
			// _sharedLogicService = sharedLogicService;
			_blockerView = blockerView;
			_globalContext = globalContext;
			// _tutorialProcessor = tutorialProcessor;
			_loadingScreen = loadingScreen;
			// _loginController = loginController;
			_dataStorageProvider = dataStorageProvider;
			_gameDatabaseProvider = gameDatabaseProvider;
			_screenLocker = screenLocker;
			// _tutorialView = tutorialView;
			// _bundlesCatalogService = bundlesCatalogService;
			_assetProvider = assetProvider;
		}

		public async UniTask MainLoop(CancellationToken ct) {
			_evRestartGame.Reset();
			_gameStarted = false;

			var gameParams = StartupParams.Value;

			// if (!gameParams.UseCustomBundleVersion && !Application.isEditor)
			// 	await _bundlesCatalogService.DownloadCatalog(ct);

			// preload game Database
			await _gameDatabaseProvider.LoadGameDatabase();
			
#if UNITY_IOS
			await HandleATTracking(ct);
#endif
			
			DeviceId = GetPlayerDeviceId();
			Debug.Assert(!DeviceId.IsNullOrEmpty());
			Debug.Log($"DeviceToken={DeviceId} VersionCode={VersionService.VersionCode}");
			
			// init logic App
			// await _logicApplication.Initialize(ct, SharedVersion.Parse(VersionService.ShortVersionString, VersionService.VersionCode));
			
			// make server connection
			// if (!await ConnectToServer(connectionOptions, ct)) {
			// 	await _evRestartGame.WaitAsync(ct);
			// 	await UnloadGame(ct);
			// 	return;
			// }
			
			_loadingScreen.Report(0.5f);

			// enter game: -> Meta
			_gameStarted = true;
			_metaFlowController.EnterMeta(ct).Forget();
			
			// await for signal and unload game
			await _evRestartGame.WaitAsync(ct);
			await UnloadGame(ct);
		}

		public UniTask Reset(bool forceShowConnectionScreen) {
			_evRestartGame.FireEvent();
			return UniTask.CompletedTask;
		}

		private async UniTask UnloadGame(CancellationToken ct) {
			await UniTask.NextFrame(ct);

			if (_gameStarted) {
				// _tutorialProcessor.AbortStage(true);

				while (!ct.IsCancellationRequested) {
					await UniTask.NextFrame(ct);
					// if (!_gameStateMachine.ChangingState && (_gameStateMachine.IsActive<PlayMetaState>() || _gameStateMachine.IsActive<PlayBattleState>())) {
					// 	break;
					// }
				}

				// if (_gameStateMachine.IsActive<PlayMetaState>()) {
				// 	await _metaFlowController.ExitMeta(ct: ct);
				// }
				// else if (_gameStateMachine.IsActive<PlayBattleState>()) {
				// 	await _battleFlowController.ExitBattle(false, ct);
				// }
				// else {
				// 	Debug.LogError($"Unknown state {_gameStateMachine.CurrentState?.GetType().FullName ?? "null"} - cannot exit");
				// }
			}

			await _screenManager.UnloadAllScreens(new[] { TypeOf<SystemDialogScreen>.Raw });

			// _globalContext.ReplaceMetaHost(null);
			LocalProfileStorage.CloseProfile();

			// _blockerView.Close(NetworkLockTag);
			
			_screenLocker.UnlockAll();
			_blockerView.UnlockAll();
			// _tutorialView.Reset();
		}

	}

}