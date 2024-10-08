using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using XLib.Core.Utils;
using XLib.UI.Types;

namespace XLib.UI.Screens {

	public enum ScreenStateType {
		Main = 1,
		Child = 2
	}

	public interface IScreenArgs { }

	public abstract class ScreenStateBase : IScreenArgs { }

	public interface IUIScreenSetup { }

	public interface IUIAsyncLoader {
		bool IsReady { get; }
	}

	public interface IUIScreenPreloadLocker {
		public void Register(IUIAsyncLoader loader);
		public void Unregister(IUIAsyncLoader loader);
		public void OnChange();
		public UniTask WaitReady();
	}
	
	public interface IUIScreenSetup<in TArgs> : IUIScreenSetup where TArgs : IScreenArgs {
		UniTask SetupScreen(TArgs args);
	}

	public interface IUIScreen {
		ScreenStateType ScreenHierarchyType { get; }
		ScreenState State { get; }
		ScreenStyle Style { get; }

		void Close();
		void Close(bool transition);
		internal UniTask OpenInternal(Func<UniTask> setVisible, bool forceRefresh = false);
		UniTask CloseAsync();
		UniTask CloseAsync(bool transition);
		internal UniTask CloseInternal(Func<UniTask> doAfterHide = null);
	}

	public interface IUiScreenWithState {
		UniTask<ScreenStateBase> SaveState();
		UniTask RecoverState(ScreenStateBase state);
	}

	public interface IUiScreenWithState<T> : IUiScreenWithState where T : ScreenStateBase {
		async UniTask<ScreenStateBase> IUiScreenWithState.SaveState() {
			var result = await SaveState();
			return result;
		}

		UniTask IUiScreenWithState.RecoverState(ScreenStateBase state) {
			return RecoverState(state as T);
		}

		new UniTask<T> SaveState();
		UniTask RecoverState(T state);
	}

	public interface IUiScreenWithStateResult {
		Action<object> ResultChanged { get; set; }
		object SaveState();
		void RecoverState(object state);
	}

}