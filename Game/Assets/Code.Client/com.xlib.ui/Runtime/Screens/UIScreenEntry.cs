using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.UI.Internal;

namespace XLib.UI.Screens {

	internal class UIScreenEntry : INotifyCompletion {
		private ScreenStateBase _state;
		public Type ScreenType { get; }
		internal UIScreenInstance ScreenInstance { get; }
		private readonly IUiScreenWithState _screenWithState;

		private Action _closeListeners;

		internal UIScreenEntry(UIScreenInstance screenInstance) {
			ScreenInstance = screenInstance;
			ScreenType = screenInstance.Screen.GetType();
			if (screenInstance.Screen is IUiScreenWithState screenWithState) _screenWithState = screenWithState;
		}

		public void Close() {
			try {
				_closeListeners?.Invoke();
			}
			catch (Exception ex) {
				Debug.LogException(ex);
			}
			finally {
				_closeListeners = null;
			}

			_isCompleted = true;
		}

		public async UniTask SaveState() {
			SaveStateInternal();
			if (_screenWithState == null || _state != null) return;
			_state = await _screenWithState.SaveState();
		}

		public async UniTask<bool> RecoverState() {
			RecoverStateInternal();
			if (_state == null) return false;
			await _screenWithState.RecoverState(_state);
			_state = null;
			return true;
		}

		protected virtual void SaveStateInternal() { }
		protected virtual void RecoverStateInternal() { }

		public void OnCompleted(Action continuation) {
			_closeListeners += continuation;
		}

		private bool _isCompleted;
		public bool IsCompleted => _isCompleted;

		public UIScreenEntry GetAwaiter() {
			return this;
		}

		public void GetResult() { }
	}

}