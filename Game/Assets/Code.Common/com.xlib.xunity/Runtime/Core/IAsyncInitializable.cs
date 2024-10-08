using System.Threading;
using Cysharp.Threading.Tasks;

namespace XLib.Unity.Core {

	/// <summary>
	///     initialize class right after root classes initialized
	/// </summary>
	public interface IAsyncInitializable {

		UniTask InitializeAsync(CancellationToken ct);

	}

}