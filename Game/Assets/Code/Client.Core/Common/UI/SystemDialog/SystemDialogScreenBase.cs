using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLib.UI.Screens;
using XLib.UI.Types;

namespace Client.Core.Common.UI.SystemDialog {

	public abstract class SystemDialogScreenBase : UIScreenWithResult<DialogResult>, IUIScreenSetup<SystemDialogScreenBase.Args> {
		public struct Args : IScreenArgs {
			public string TitleText { get; }
			public string MessageText { get; }
			public string Button1Text { get; }
			public string Button2Text { get; }
			public string Button3Text { get; }
			public bool WithCloseButton { get; }

			public Args(string titleText, string messageText, string button1Text, string button2Text, string button3Text, bool withCloseButton) {
				TitleText = titleText;
				MessageText = messageText;
				Button1Text = button1Text;
				Button2Text = button2Text;
				Button3Text = button3Text;
				WithCloseButton = withCloseButton;
			}
		}

		public override ScreenStyle Style => ScreenStyle.Default.With(ScreenStyle.System);

		[SerializeField, Required] private TMP_Text _txtTitle;
		[SerializeField, Required] private TMP_Text _txtText;

		[Space, SerializeField, Required]
		private Button _btOk;

		[SerializeField, Required] private TMP_Text _txtOk;

		[Space, SerializeField, Required]
		private Button _btCancel;

		[SerializeField, Required] private TMP_Text _txtCancel;

		[Space, SerializeField, Required]
		private Button _btReset;

		[SerializeField, Required] private TMP_Text _txtReset;
		[SerializeField, Required, ListDrawerSettings(DefaultExpandedState = true)] private Button[] _closeButton;

		protected override void InitializeView() {
			_btOk.onClick.AddListener(() => CloseWithResult(DialogResult.Ok));
			_btCancel.onClick.AddListener(() => CloseWithResult(DialogResult.Cancel));
			_btReset.onClick.AddListener(() => CloseWithResult(DialogResult.Reset));
		}

		private void SetText(Args args) {
			_txtTitle.text = args.TitleText ?? string.Empty;
			_txtText.text = args.MessageText ?? string.Empty;

			SetButton(_btOk, _txtOk, args.Button1Text);
			SetButton(_btCancel, _txtCancel, args.Button2Text);
			SetButton(_btReset, _txtReset, args.Button3Text);
		}

		private void SetButton(Button btn, TMP_Text btnTxt, string actionText) {
			if (actionText != null) btnTxt.text = actionText;
			btn.SetActive(actionText != null);
		}

		public UniTask SetupScreen(Args args) {
			SetText(args);
			SetResult(DialogResult.Cancel);
			_closeButton.SetActive(args.WithCloseButton);

			return UniTask.CompletedTask;
		}
	}

}