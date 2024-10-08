using System.Threading;
using Cysharp.Threading.Tasks;

namespace XLib.States.Contracts {

	/// <summary>
	///     call OnEnter function right after state activated
	/// </summary>
	public interface IEnterState {

		UniTask OnEnterAsync(CancellationToken ct);

	}

}