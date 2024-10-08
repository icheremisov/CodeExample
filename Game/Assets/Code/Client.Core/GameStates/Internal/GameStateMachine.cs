using Client.Core.GameStates.Contracts;
using XLib.States.Contracts;
using XLib.States.Controllers;

// ReSharper disable once ClassNeverInstantiated.Global

namespace Client.Core.GameStates.Internal {

	internal class GameStateMachine : BaseStateMachine<IGameState>, IGameStateMachine {

		public GameStateMachine(IStateFactory<IGameState> stateFactory) : base(stateFactory) { }

	}

}