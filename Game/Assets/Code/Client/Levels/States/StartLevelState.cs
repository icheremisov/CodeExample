using Client.Core.Common.Contracts;
using Client.Levels.Contracts;
using Cysharp.Threading.Tasks;

namespace Client.Levels.States
{
    public class StartLevelState : ILevelState
    {
        public ClientLevelState State => ClientLevelState.Start;

        private readonly ILoadingScreen _loadingScreen;

        public StartLevelState(ILoadingScreen loadingScreen) => _loadingScreen = loadingScreen;

        public void OnEnter()
        {
            _loadingScreen.HideAsync().Forget();
        }

        public void OnExit()
        {
        }
    }
}