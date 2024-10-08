using Client.Levels.Contracts;

namespace Client.Levels.States
{
    public class EndLevelState : ILevelState
    {
        public ClientLevelState State => ClientLevelState.End;
        
        public void OnEnter() {}

        public void OnExit() {}
    }
}