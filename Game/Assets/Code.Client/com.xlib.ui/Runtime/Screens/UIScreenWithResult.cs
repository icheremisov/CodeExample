using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace XLib.UI.Screens {

	/// <summary>
	///     base class for UIScreen with return result. must used with _screenManager.ShowAndWaitAsync
	/// </summary>
	public abstract class UIScreenWithResult<TResult> : UIScreen, IUIScreenSetup<UIScreenWithResult<TResult>.ResultInternal>, IUiScreenWithStateResult {
		public struct ResultInternal : IScreenArgs {
			public TResult Result { get; set; }
		}

		public Action<object> ResultChanged { get; set; }
		private TResult _result;
		private TResult Result {
			get => _result;
			set {
				_result = value;
				ResultChanged?.Invoke(_result);
			}
		}

		protected override UniTask OnOpenAsync() {
			Result = default;
			return base.OnOpenAsync();
		}

		protected void SetResult(TResult result) => Result = result;

		protected void CloseWithResult(TResult result) {
			Result = result;
			ResultChanged = null;
			Close();
		}

		protected UniTask CloseAsyncWithResult(TResult result) {
			Result = result;
			ResultChanged = null;
			return CloseAsync();
		}

		public UniTask SetupScreen(ResultInternal args) {
			Result = args.Result;
			return UniTask.CompletedTask;
		}

		public void RecoverState(object state) {
			switch (state) {
				case null:
					Result = default;
					break;

				case TResult result:
					Result = result;
					break;

				default:
					Debug.LogError(
						$"Error while recovering state in screen wit type {nameof(GetType)}, type of screen result is {nameof(TResult)} and recover result type is {nameof(state.GetType)}");
					break;
			}
		}

		public object SaveState() {
			return Result;
		}
	}

}