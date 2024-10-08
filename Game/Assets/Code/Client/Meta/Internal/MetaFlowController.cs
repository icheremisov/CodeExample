using System.Threading;
using Client.Core;
using Client.Core.Contracts;
using Client.Core.GameFlow.Contracts;
using Client.Meta.Internal;
using Cysharp.Threading.Tasks;
using XLib.Core.AsyncEx;
using Zenject;

namespace Client.Meta
{
    public class MetaFlowController : FlowController, IMetaFlowController
    {
        private static readonly Logger Logger = new(nameof(MetaFlowController));

        private readonly IConsoleCheatsInitializer _cheatsInitializer;
        private readonly AsyncLock _switchLock = new();

        public MetaFlowController([Inject(Optional = true)] IConsoleCheatsInitializer cheatsInitializer) => _cheatsInitializer = cheatsInitializer;

        async UniTask IMetaFlowController.EnterMeta(CancellationToken ct)
        {
            if (_switchLock.Taken) return;
            using var _ = await _switchLock.LockAsync(ct);

            Logger.Log("EnterMeta");

            _cheatsInitializer?.InitializeCheatVars();

            await LoadingScreen.ShowAsync(false, ct);
            await GameStateMachine.EnterAsync<EmptyState>(ct);

            LoadingScreen.Report(1.0f);

            GameStateMachine.EnterAsync<PlayMetaState>(ct).Forget();
            Logger.Log("EnterMeta OK");
        }

        async UniTask IMetaFlowController.ExitMeta(CancellationToken ct)
        {
            if (_switchLock.Taken) return;
            using var _ = await _switchLock.LockAsync(ct);

            Logger.Log("ExitMeta");
            await LoadingScreen.ShowAsync(false, ct);

            LoadingScreen.Report(0.1f);
            await GameStateMachine.EnterAsync<EmptyState>(ct);

            LoadingScreen.Report(0.5f);
            Logger.Log("ExitMeta OK");
        }
    }
}