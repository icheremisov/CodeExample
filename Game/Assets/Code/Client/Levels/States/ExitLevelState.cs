using Client.Levels.Contracts;

namespace Client.Levels.States
{
    public class ExitLevelState : ILevelState
    {
        public ClientLevelState State => ClientLevelState.Exit;
        
        public void OnEnter() {}

        public void OnExit() {}
    }
}