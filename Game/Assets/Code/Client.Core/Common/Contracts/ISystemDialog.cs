using System.Threading;
using Client.Core.Common.UI.SystemDialog;
using Cysharp.Threading.Tasks;

namespace Client.Core.Common.Contracts {

	public interface ISystemDialog {

		/// <summary>
		/// show dialog with one close button and wait to close
		/// return DialogResult.ButtonPressed if button pressed and DialogResult.Cancel if dialog was cancelled
		/// </summary>
		UniTask<DialogResult> ShowAsync(string titleText, string messageText, string closeText, DialogType dialogType = DialogType.Game, CancellationToken ct = default);
		
		/// <summary>
		/// show dialog with one close button and wait to close and with exit button
		/// return DialogResult.ButtonPressed if button pressed and DialogResult.Cancel if dialog was cancelled
		/// </summary>
		UniTask<DialogResult> ShowAsync(string titleText, string messageText, string closeText, bool withCloseButton, DialogType dialogType = DialogType.Game, CancellationToken ct = default);

		/// <summary>
		/// show dialog with two buttons (Ok/Cancel)
		/// return DialogResult.Ok (or ButtonPressed) if first button pressed
		/// and DialogResult.Cancel (or Button2Pressed) if second button was pressed or dialog was cancelled
		/// </summary>
		UniTask<DialogResult> ShowAsync(string titleText, string messageText, string okText, string cancelText, DialogType dialogType = DialogType.Game, CancellationToken ct = default);
		UniTask<DialogResult> ShowAsync(string titleText, string messageText, string okText, string cancelText, bool withCloseButton, DialogType dialogType = DialogType.Game, CancellationToken ct = default);

		/// <summary>
		/// show dialog with three buttons (Ok/Cancel/Reset)
		/// return DialogResult.Ok (or ButtonPressed) if first button pressed
		/// and DialogResult.Cancel (or Button2Pressed) if second button was pressed or dialog was cancelled
		/// and DialogResult.Reset (or Button3Pressed) if third button was pressed
		/// </summary>
		UniTask<DialogResult> ShowAsync(string titleText, string messageText, string okText, string cancelText, string resetText, DialogType dialogType = DialogType.Game, CancellationToken ct = default);

		UniTask Close();
	}

}