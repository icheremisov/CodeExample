using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Core.GameFlow.Contracts {
	/// <summary>
	///     entry point for level
	/// </summary>
	public interface ILevelFlowController {

		UniTask EnterLevel(LevelArgumentData argumentData, CancellationToken ct = default);

		UniTask ExitLevel(CancellationToken ct = default);
	}

}