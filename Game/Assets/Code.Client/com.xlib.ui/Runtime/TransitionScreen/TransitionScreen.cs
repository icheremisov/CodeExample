using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.UI.Contracts;

namespace XLib.UI.TransitionScreen {

	public class TransitionScreen : ITransitionScreen {
		private readonly TransitionView _view;

		public TransitionScreen(TransitionView view) {
			_view = view;
			_view.Hide(true).Forget();
		}

		public bool IsVisible => _view != null && _view.IsVisible;
		public bool IsNeedShow { get; private set; }

		public void MarkShow() {
			IsNeedShow = true;
			_view.Prepare();
		}

		public async UniTask ShowAsync(CancellationToken ct = default, float delay = 0f) {
			IsNeedShow = false;
			
			if (_view.IsVisible) return;

			Debug.Log("Transition show");

			await _view.Show(false, delay);

			await UniTask.NextFrame(ct);
		}

		public async UniTask HideAsync(CancellationToken ct = default, float delay = 0f) {
			Debug.Log("Transition hide");

			await _view.Hide(false, delay);
			
			await UniTask.NextFrame(ct);
		}

	}

}