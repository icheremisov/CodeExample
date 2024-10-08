using System.Threading;
using Cysharp.Threading.Tasks;

namespace XLib.States.Contracts {

	/// <summary>
	///     call OnExit function before state deactivated
	/// </summary>
	public interface IExitState {

		UniTask OnExitAsync(CancellationToken ct);

	}

}