using Client.Levels.Contracts;

namespace Client.Levels.States
{
    public class ActiveLevelState : ILevelState
    {
        public ClientLevelState State => ClientLevelState.Active;
        
        public void OnEnter() {}

        public void OnExit() {}
    }
}