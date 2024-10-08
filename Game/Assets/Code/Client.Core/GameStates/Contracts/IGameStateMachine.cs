using XLib.States.Contracts;

namespace Client.Core.GameStates.Contracts {

	/// <summary>
	///     state machine for game states
	/// </summary>
	public interface IGameStateMachine : IStateMachine<IGameState> { }

}