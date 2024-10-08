using System.Threading;
using Client.Core.GameStates.Attributes;
using Client.Core.GameStates.Contracts;
using Cysharp.Threading.Tasks;
using XLib.States.Contracts;

namespace Client.Core {

	[BindToRootInstaller]
	public class EmptyState : IGameState, IEnterState {
		public UniTask OnExitAsync(CancellationToken ct) => UniTask.CompletedTask;

		public UniTask OnEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
	}

}