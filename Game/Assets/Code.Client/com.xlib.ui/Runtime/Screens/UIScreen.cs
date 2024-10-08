using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using XLib.UI.Animation;
using XLib.UI.Contracts;
using XLib.UI.Types;
using XLib.UI.Views;
using Zenject;

namespace XLib.UI.Screens {

	/// <summary>
	///     base class for UIScreen without arguments
	/// </summary>
	public abstract class UIScreen : UIView, IUIScreen, IUIScreenPreloadLocker {
		[Inject] private IScreenManager _screenManager;
		[Inject] private ITransitionScreen _transitionScreen;
		public ScreenState State { get; private set; } = ScreenState.Hidden;
		public virtual ScreenStateType ScreenHierarchyType { get; } = ScreenStateType.Main;

		public abstract ScreenStyle Style { get; }
		protected IScreenManager ScreenManager => _screenManager;
		protected IScreenLocker ScreenLocker => ScreenLockerInternal;

		private bool _visible;
		private UIScreenAnimation _screenAnimation;

		private protected override void InitializeInternal() {
			var actionButtons = GetComponentsInChildren<UIActionButton>(true);
			foreach (var button in actionButtons) {
				if (button == null) continue;

				var bt = button.GetExistingComponent<Button>();
				var action = button.Action;
				var transition = button.Transition;
				bt.onClick.AddListener(() => {
					if (CanPerform(action)) ScreenManager.PerformAction(this, action, transition).Forget();
				});
			}

			_screenAnimation = GetComponent<UIScreenAnimation>();
		}

		public virtual bool CanPerform(UIBuiltInAction action) => true;

		async UniTask IUIScreen.OpenInternal(Func<UniTask> setVisible, bool forceRefresh) {
			while (State != ScreenState.Visible && State != ScreenState.Hidden) await UniTask.DelayFrame(1);

			if (!forceRefresh && State == ScreenState.Visible) return;

			State = ScreenState.Showing;

			try {
				if (!forceRefresh) PreWarm();
				await OnOpenAsync();
				await ((IUIScreenPreloadLocker)this).WaitReady();
				await ShowViewAsync(setVisible);
				
				State = ScreenState.Visible;
			}
			catch (Exception) {
				State = ScreenState.Hidden;
				throw;
			}
		}

		async UniTask IUIScreen.CloseInternal(Func<UniTask> doAfterHide) {
			while (State != ScreenState.Visible && State != ScreenState.Hidden) await UniTask.DelayFrame(1);

			if (State == ScreenState.Hidden) return;

			State = ScreenState.Hiding;

			try {
				await OnCloseAsync();
				await HideViewAsync(doAfterHide);
				State = ScreenState.Hidden;
			}
			catch (Exception) {
				State = ScreenState.Visible;
				throw;
			}
		}

		public override async UniTask CamerasVisibleChanged(bool visible) {
			await base.CamerasVisibleChanged(visible);

			if (_visible == visible) return;

			if (_screenAnimation != null) {
				if (!_visible) {
					await UniTask.WhenAll(_screenAnimation.Show(CancellationToken.None, _transitionScreen.IsVisible ? 0.25f : 0),
						_transitionScreen.IsVisible ? _transitionScreen.HideAsync() : UniTask.CompletedTask);
				}
				else {
					await UniTask.WhenAll(_screenAnimation.Hide(CancellationToken.None),
						_transitionScreen.IsNeedShow ? _transitionScreen.ShowAsync(CancellationToken.None, 0.25f) : UniTask.CompletedTask);
				}
			}
			else {
				switch (_visible) {
					case false when _transitionScreen.IsVisible: await _transitionScreen.HideAsync();
						break;
					case true when _transitionScreen.IsNeedShow: await _transitionScreen.ShowAsync();
						break;
				}
			}
			
			_visible = visible;
		}
		
		public void Close() => CloseAsync().Forget();
		public void Close(bool transition) => CloseAsync(transition).Forget();
		public UniTask CloseAsync() => _screenManager.CloseLast(GetType(), false);
		public UniTask CloseAsync(bool transition) => _screenManager.CloseLast(GetType(), transition);

		protected virtual UniTask OnOpenAsync() => UniTask.CompletedTask;
		protected virtual UniTask OnCloseAsync() => UniTask.CompletedTask;

		public override string ToString() => GetType().Name;

		private HashSet<IUIAsyncLoader> _loaders = null;
		private bool _isReady = true;
		void IUIScreenPreloadLocker.Register(IUIAsyncLoader loader) {
			_loaders ??= new HashSet<IUIAsyncLoader>();
			_loaders.Add(loader);
			((IUIScreenPreloadLocker)this).OnChange();
		}

		void IUIScreenPreloadLocker.Unregister(IUIAsyncLoader loader) {
			_loaders?.Remove(loader);
			((IUIScreenPreloadLocker)this).OnChange();
		}

		void IUIScreenPreloadLocker.OnChange() {
			if(_loaders == null) return;
			_isReady = _loaders.All(loader => loader.IsReady);
		}

		async UniTask IUIScreenPreloadLocker.WaitReady() {
			await UniTask.Yield();
			if (!_isReady) 
				await UniTask.WaitUntil(() => _isReady);
		}
	}

}