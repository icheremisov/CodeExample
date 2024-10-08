using System.Threading;
using Cysharp.Threading.Tasks;

namespace XLib.UI.Animation {
	
	public class UIViewAnimation : UISceneAnimation {
		public override async UniTask Show(CancellationToken ct, float delay = 0) => await ShowInternal(ct, delay);
		public override async UniTask Hide(CancellationToken ct, float delay = 0) => await HideInternal(ct, delay);
		public override async UniTask Play(string animationName, CancellationToken ct, float delay = 0) => await PlayInternal(animationName, ct, delay);
		public override async UniTask Play(int animationHash, CancellationToken ct, float delay = 0) => await PlayInternal(animationHash, ct, delay);
	}

}