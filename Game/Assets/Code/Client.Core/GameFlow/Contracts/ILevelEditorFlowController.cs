using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Core.GameFlow.Contracts {

	/// <summary>
	///     entry point for dev tools
	/// </summary>
	public interface ILevelEditorFlowController {
		// UniTask CreateLevelEditor(LevelArgumentData argumentData, CancellationToken ct = default);

		UniTask ExitLevelEditor(CancellationToken ct = default);
	}

}