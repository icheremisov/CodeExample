using System;
using System.Threading;
using Client.Core.Common.Contracts;
using Cysharp.Threading.Tasks;
using XLib.Core.AsyncEx;
using XLib.UI.Contracts;
using XLib.Unity.Core;

namespace Client.Core.Common.UI.SystemDialog {

	internal class SystemDialog : ISystemDialog, IAsyncInitializable {
		private readonly IScreenManager _screenManager;

		private readonly AsyncLock _showGameLock = new();
		private readonly AsyncLock _showSystemLock = new();
		private SystemDialogScreenBase _screen;

		public SystemDialog(IScreenManager screenManager) {
			_screenManager = screenManager;
		}

		public async UniTask InitializeAsync(CancellationToken ct) {
			await _screenManager.TryPreloadScreen<SystemDialogScreen>();
			await _screenManager.TryPreloadScreen<GameDialogScreen>();
		}

		private async UniTask<DialogResult> ShowAndWaitResult(DialogType dialogType, SystemDialogScreenBase.Args args, CancellationToken ct) {
			switch (dialogType) {
				case DialogType.Game: {
					using var _ = await _showGameLock.LockAsync();
					return await _screenManager.Screen<GameDialogScreen, DialogResult>().ShowAndWaitResult(args);
				}

				case DialogType.SystemOverlay: {
					using var _ = await _showSystemLock.LockAsync();
					return await _screenManager.Screen<SystemDialogScreen, DialogResult>().ShowAndWaitResult(args);
				}

				default: throw new ArgumentOutOfRangeException(nameof(dialogType), dialogType, null);
			}
		}
		
		public async UniTask<DialogResult> ShowAsync(string titleText, string messageText, string closeText, bool withCloseButton, 
			DialogType dialogType = DialogType.Game, CancellationToken ct = default) {
			var args = new SystemDialogScreenBase.Args(titleText, messageText, closeText, null, null, withCloseButton);
			return await ShowAndWaitResult(dialogType, args, ct);
		}

		public async UniTask<DialogResult> ShowAsync(string titleText, string messageText, string closeText, DialogType dialogType = DialogType.Game,
			CancellationToken ct = default) {
			var args = new SystemDialogScreenBase.Args(titleText, messageText, closeText, null, null, false);
			return await ShowAndWaitResult(dialogType, args, ct);
		}

		public async UniTask<DialogResult> ShowAsync(string titleText, string messageText, string okText, string cancelText,
			DialogType dialogType = DialogType.Game,
			CancellationToken ct = default) {
			var args = new SystemDialogScreenBase.Args(titleText, messageText, okText, cancelText, null, false);
			return await ShowAndWaitResult(dialogType, args, ct);
		}
		
		public async UniTask<DialogResult> ShowAsync(string titleText, string messageText, string okText, string cancelText,
			bool withCloseButton, DialogType dialogType = DialogType.Game,
			CancellationToken ct = default) {
			var args = new SystemDialogScreenBase.Args(titleText, messageText, okText, cancelText, null, withCloseButton);
			return await ShowAndWaitResult(dialogType, args, ct);
		}

		public async UniTask<DialogResult> ShowAsync(string titleText, string messageText, string okText, string cancelText, string resetText, DialogType dialogType = DialogType.Game,
			CancellationToken ct = default) {
			var args = new SystemDialogScreenBase.Args(titleText, messageText, okText, cancelText, resetText, false);
			return await ShowAndWaitResult(dialogType, args, ct);
		}
		public UniTask Close() => _screenManager.Screen<GameDialogScreen>().Close();
	}

}