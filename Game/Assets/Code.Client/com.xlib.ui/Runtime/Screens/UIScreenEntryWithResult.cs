using XLib.UI.Internal;

namespace XLib.UI.Screens {

	internal class UIScreenEntryWithResult : UIScreenEntry {
		private object _result;
		private readonly IUiScreenWithStateResult _screenWithResult;

		internal UIScreenEntryWithResult(UIScreenInstance screenInstance, IUiScreenWithStateResult screenWithStateResult) : base(screenInstance) {
			_screenWithResult = screenWithStateResult;
			_result = screenWithStateResult.SaveState();
			_screenWithResult.ResultChanged = SetResult;
		}

		public new UIScreenEntryWithResult GetAwaiter() {
			return this;
		}

		public new object GetResult() {
			return _result;
		}

		private void SetResult(object result) {
			_result = result;
		}

		protected override void SaveStateInternal() {
			_result = _screenWithResult?.SaveState();
		}

		protected override void RecoverStateInternal() {
			if (_screenWithResult == null) return;
			_screenWithResult.RecoverState(_result);
			_screenWithResult.ResultChanged = SetResult;
		}
	}

}