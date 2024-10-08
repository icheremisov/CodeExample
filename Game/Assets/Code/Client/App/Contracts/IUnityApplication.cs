using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.App.Contracts {

	public interface IUnityApplication {
		string DeviceId { get; }
		UniTask MainLoop(CancellationToken ct);
		UniTask Reset(bool forceShowConnectionScreen);

	}

}