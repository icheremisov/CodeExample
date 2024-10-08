using System.Threading;
using Cysharp.Threading.Tasks;

namespace XLib.UI.Animation.Contracts {

	public interface IUISceneAnimation {
		UniTask Show(CancellationToken ct, float delay);
		UniTask Hide(CancellationToken ct, float delay);
		UniTask Play(string animationName, CancellationToken ct, float delay);
		UniTask Play(int animationHash, CancellationToken ct, float delay);
	}

}