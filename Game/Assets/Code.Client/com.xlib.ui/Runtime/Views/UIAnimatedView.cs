using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.Core.AsyncEx;

namespace XLib.UI.Views {

	public class UIAnimatedView : MonoBehaviour {
		private AsyncEvent _animationEvent;
		
		public event Action<UIAnimatedView> Enabled;
		public event Action<UIAnimatedView> Disabled;
		
		public event Action<UIAnimatedView> BeforeShown;
		public event Action<UIAnimatedView> OnShown;
		public event Action<UIAnimatedView> AfterShown;
		public event Action<UIAnimatedView> BeforeHidden;
		public event Action<UIAnimatedView> OnHidden;
		public event Action<UIAnimatedView> AfterHidden;

		private void OnEnable() {
			Enabled?.Invoke(this);
		}

		private void OnDisable() {
			ReportAnimationFinished();
			Disabled?.Invoke(this);
		}
		
		public UniTask ShowViewAsync() {
			if (gameObject.activeSelf) return UniTask.CompletedTask;
			
#if VIEW_LOGS
			UILogger.Log($"[{GetType().Name}] Show");
#endif

			return ShowInternalAsync();
		}

		public UniTask HideViewAsync() {
			if (!gameObject.activeSelf) return UniTask.CompletedTask;
			
#if VIEW_LOGS
			UILogger.Log($"[{GetType().Name}] Hide");
#endif

			return HideInternalAsync();
		}
		
		private async UniTask ShowInternalAsync() {
			BeforeShown?.Invoke(this);
			
			await BeforeShowAsync();

			ReplaceEvent(null);

			OnShown?.Invoke(this);

			if (_animationEvent != null) await _animationEvent.WaitAsync();

			await AfterShowAsync();

			AfterShown?.Invoke(this);
		}

		private async UniTask HideInternalAsync() {
			BeforeHidden?.Invoke(this);
			
			await BeforeHideAsync();

			ReplaceEvent(null);

			OnHidden?.Invoke(this);

			if (_animationEvent != null) await _animationEvent.WaitAsync();

			gameObject.SetActive(false);

			await AfterHideAsync();

			AfterHidden?.Invoke(this);
		}
		
		protected virtual UniTask BeforeShowAsync() => UniTask.CompletedTask;

		protected virtual UniTask AfterShowAsync() => UniTask.CompletedTask;

		protected virtual UniTask BeforeHideAsync() => UniTask.CompletedTask;

		protected virtual UniTask AfterHideAsync() => UniTask.CompletedTask;
		
		public void SetAnimationEvent(AsyncEvent animationEvent) {
			_animationEvent = animationEvent;
		}
		
		private void ReplaceEvent(AsyncEvent newEvent) {
			var ev = _animationEvent;
			_animationEvent = null;
			ev?.FireEvent();

			_animationEvent = newEvent;
		}

		public void ReportAnimationStarted() {
			ReplaceEvent(new AsyncEvent());
		}

		public void ReportAnimationFinished() {
			ReplaceEvent(null);
		}
	}

}