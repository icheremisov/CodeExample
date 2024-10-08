using System.Threading;
using Cysharp.Threading.Tasks;

namespace XLib.UI.Contracts {

	public interface ITransitionScreen {

		bool IsVisible { get; }
		bool IsNeedShow { get; }
		void MarkShow();
		UniTask ShowAsync(CancellationToken ct = default, float delay = 0f);
		UniTask HideAsync(CancellationToken ct = default, float delay = 0f);

	}

}