using System;
using System.Collections.Generic;
using System.Threading;
using Client.Core;
using Client.Core.GameFlow.Contracts;
using Client.Definitions;
using Client.Levels.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.Assets;
using XLib.Core.AsyncEx;
using XLib.UI.Contracts;

namespace Client.Levels.Internal
{
    public class LevelFlowController : FlowController, ILevelFlowController
    {
        private static readonly Logger Logger = new(nameof(LevelFlowController));

        private readonly GlobalContext _globalContext;
        private readonly IScreenManager _screenManager;
        private List<SceneInstanceLoader> _sceneInstances;
        private readonly AsyncLock _switchLock = new();

        public LevelFlowController(GlobalContext globalContext, IScreenManager screenManager)
        {
            _globalContext = globalContext;
            _screenManager = screenManager;
        }

        async UniTask ILevelFlowController.EnterLevel(LevelArgumentData argumentData, CancellationToken ct)
        {
            Logger.Log("Enter Level");

            await MetaFlowController.ExitMeta(ct);

            ct.ThrowIfCancellationRequested();

            await LoadingScreen.ShowAsync(false, ct);
            await GameStateMachine.EnterAsync<EmptyState>(ct);

            try {
                LoadingScreen.Report(0.3f);

                Debug.Assert(_sceneInstances.IsNullOrEmpty());
                // load battle scene
                _sceneInstances = await SceneLoader.LoadScenesAsync(argumentData.LevelDefinition.Scenes, LoadingScreen.Remap(0.2f, 0.4f), ct);

                LoadingScreen.Report(0.4f);

                GameStateMachine.EnterAsync<PlayLevelState>(ct).Forget();
            }
            catch (Exception e) {
                LoadingScreen.ShowAsync(false, CancellationToken.None).Forget();
                throw;
            }

            Logger.Log("Enter Level OK");
        }

        async UniTask ILevelFlowController.ExitLevel(CancellationToken ct)
        {
            if (_switchLock.Taken) return;
            using var _ = await _switchLock.LockAsync(ct);

            Logger.Log("Exit Level");

            await LoadingScreen.ShowAsync(false, ct);

            ct.ThrowIfCancellationRequested();

            await GameStateMachine.EnterAsync<EmptyState>(ct);

            var levelScreen = _screenManager.GetScreenOrDefault<LevelScreen>();
            if (levelScreen != null) 
                await levelScreen.CloseAsync();
            
            LoadingScreen.Report(0.1f);
			
            await SceneLoader.UnloadScenesAsync(_sceneInstances, LoadingScreen.Remap(0.0f, 0.5f), ct);
            _sceneInstances = null;

            LoadingScreen.Report(0.75f);

            Time.timeScale = 1;

            ct.ThrowIfCancellationRequested();

            await MetaFlowController.EnterMeta(ct);
            Logger.Log("Exit Level OK");
        }
    }
}