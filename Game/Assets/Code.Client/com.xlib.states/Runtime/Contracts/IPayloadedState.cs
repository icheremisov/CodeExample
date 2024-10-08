using System.Threading;
using Cysharp.Threading.Tasks;

namespace XLib.States.Contracts {

	/// <summary>
	///     call OnEnter function right after state activated
	/// </summary>
	public interface IPayloadedState<in T> {

		UniTask OnEnterAsync(T payload, CancellationToken ct);

	}

}