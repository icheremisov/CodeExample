using System.Threading;
using Client.Core.Common.Contracts;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.BuildSystem;
using XLib.UI.ConnectionBlocker;
using XLib.UI.Types;

namespace Client.Core.Common.UI.LoadingScreen {

	internal class LoadingScreen : ILoadingScreen {
		private static readonly ScreenLockTag ScreenLockTag = new("LoadingScreen");
		private readonly LoadingView _view;
		private readonly IBlockerView _blockerView;

		public LoadingScreen(LoadingView view, IBlockerView blockerView) {
			_view = view;
			_view.Hide(true).Forget();
			_view.SetVersion(string.Empty);
			_view.SetProgress(0);

			_blockerView = blockerView;
		}

		public void Report(float value) {
			_view.SetProgress(value);
		}

		public bool IsVisible => _view != null && _view.IsVisible;
		public bool IsBarVisible => _view.IsBarVisible;

		public async UniTask ShowAsync(bool force = false, CancellationToken ct = default) {
			if (_view.IsVisible) return;

			Debug.Log("Loading show");
			
			_view.SetVersion(VersionService.FullVersionString);
			_view.SetProgress(0);

			await _view.Show(force);
			_blockerView.DisableOpening(ScreenLockTag);

			await UniTask.NextFrame(ct);
		}

		public async UniTask HideAsync(bool force = false, CancellationToken ct = default) {
			Debug.Log("Loading hide");
			await _view.Hide(force);
			_blockerView.EnableOpening(ScreenLockTag);
			await UniTask.NextFrame(ct);
		}

		public void SetContentVisible(bool v) => _view.SetContentVisible(v);
	}

}