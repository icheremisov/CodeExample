using Client.Levels.Contracts;

namespace Client.Levels.States
{
    public class NoneLevelState : ILevelState
    {
        public ClientLevelState State => ClientLevelState.None;
        
        public void OnEnter() {}

        public void OnExit() {}
    }
}