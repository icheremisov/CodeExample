namespace Client.Levels.Contracts
{
    public interface ILevelStateController
    {
        
    }
    
    public interface ILevelState
    {
        ClientLevelState State { get; }
		
        void OnEnter();
        void OnExit();
    }
}