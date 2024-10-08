using System.Threading;
using Client.Core.Common.Contracts;
using Client.Core.GameFlow.Contracts;
using Client.Core.GameStates.Contracts;
using Client.Ecs.Core;
using Client.Ecs.Core.Contracts;
using Client.Levels.Contracts;
using Client.Levels.UI;
using Client.Levels.View.Factory;
using Cysharp.Threading.Tasks;
using XLib.Core.Utils;
using XLib.States.Contracts;
using XLib.UI.Contracts;
using XLib.Unity.Core;

namespace Client.Levels.Internal
{
    public class PlayLevelState : IGameState, IEnterState
    {
        private readonly ILoadingScreen _loadingScreen;
        private readonly LevelContext _levelContext;
        private readonly ILevelFlowController _levelFlowController;
        private readonly ILevelViewFactory _levelViewFactory;
        private readonly IScreenManager _screenManager;
        private readonly IEcsRunner _ecsRunner;

        public PlayLevelState(
            ILoadingScreen loadingScreen,
            LevelContext levelContext, 
            ILevelFlowController levelFlowController, 
            ILevelViewFactory levelViewFactory,
            IScreenManager screenManager, 
            IEcsRunner ecsRunner)
        {
            _loadingScreen = loadingScreen;
            _levelContext = levelContext;
            _levelFlowController = levelFlowController;
            _levelViewFactory = levelViewFactory;
            _screenManager = screenManager;
            _ecsRunner = ecsRunner;
        }

        async UniTask IEnterState.OnEnterAsync(CancellationToken ct)
        {
            using (TraceLog.Usage(nameof(AsyncInitializableHelper))) {
                // initialize game classes
                await AsyncInitializableHelper.InitializeAsync(ct);
            }

            using (TraceLog.Usage("ILevelViewFactory")) {
                await _levelViewFactory.InitializeAsync();
            }
            
            _loadingScreen.Report(0.6f);

            // var room = _levelContext.battleHost.GetModule<BattleRoom>();
            //
            // _environmentController.Initialize(room.Arena.MainScene);

            _loadingScreen.Report(0.8f);

            await _screenManager.TryPreloadScreen<LevelScreen>();

            _ecsRunner.IsPaused = false;

            _ecsRunner.Start(FeaturesEcs.Feature.Global);
            _ecsRunner.Start(FeaturesEcs.Feature.Level);
            // _ecsRunner.Start(FeaturesEcs.Feature.Animation);
            
            _loadingScreen.Report(0.9f);
            
            await _screenManager.Screen<LevelScreen>().Open();

            _levelContext.SafeReplaceBattleState(ClientLevelState.Start);
        }
        
        async UniTask IExitState.OnExitAsync(CancellationToken ct)
        {
  
        }

    }
}