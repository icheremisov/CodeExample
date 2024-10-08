using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using XLib.UI.Buttons;
using XLib.UI.Contracts;
using XLib.UI.Types;
using XLib.UI.Views;

namespace XLib.UI.Internal {

	internal class ScreenLocker : IScreenLocker, IDisposable {

		private readonly HashSet<ScreenLockTag> _lockers = new();
		private readonly HashSet<ScreenLockTag> _unlockers = new();
		private readonly UIScreenLockView _view;

		private CancellationTokenSource _delayCts;

		private ScreenLockTag? _redirect;

		public ScreenLocker(UIScreenLockView view) {
			_view = view;

			_view.gameObject.SetActive(true);
			_view.SetLockerVisible(false);
			_view.Redirect.gameObject.SetActive(false);
		}

		public void Dispose() {
			CancelDelay();

			if (_view) ResetRedirect();
		}

		public event Action LockAreaClick;
		public event Action LockChanged;

		public bool IsLocked => _lockers.Count > 0 && _unlockers.Count == 0;

		public void LockScreen(ScreenLockTag tag) {
			_lockers.AddOnce(tag);
			UpdateView();
		}

		public void UnlockScreen(ScreenLockTag tag) {
			_lockers.Remove(tag);
			UpdateView();
		}

		public void UnlockAll() {
			_lockers.Clear();
			ResetRedirect();
			UpdateView();
		}

		public void AddUnlocker(ScreenLockTag tag) {
			_unlockers.AddOnce(tag);
			UpdateView();
		}

		public void RemoveUnlocker(ScreenLockTag tag) {
			_unlockers.Remove(tag);
			UpdateView();
		}

		private class LockScopeInternal : IDisposable{
			private readonly Action _callback;
			public LockScopeInternal(Action callback) => _callback = callback;
			void IDisposable.Dispose() => _callback.Invoke();
		}
		public IDisposable LockScope(ScreenLockTag tag) {
			LockScreen(tag);
			return new LockScopeInternal(() => UnlockScreen(tag));
		}

		public void LockScreen(ScreenLockTag tag, Component target) {
			ResetRedirect();

			_redirect = tag;
			LockScreen(tag);

			var redirect = _view.Redirect;
			var targetCanvas = target.GetTopmostCanvas();

			if (targetCanvas != null)
				FocusOverCanvas(target, targetCanvas.worldCamera);
			else
				FocusOverWorld(target, Camera.main);

			redirect.gameObject.SetActive(true);

			UIButton uiButton;
			Button button;
			IManualClick handler;

			var clickRedirect = target.GetComponent<ITutorialRedirect>();
			var redirectButton = clickRedirect?.GetTutorialButton();

			if (redirectButton != null) {
				handler = redirectButton.GetComponent<IManualClick>();
				uiButton = redirectButton.GetComponent<UIButton>();
				button = redirectButton.GetComponent<Button>();
			}
			else {
				handler = target.GetComponent<IManualClick>();
				uiButton = target.GetComponent<UIButton>();
				button = target.GetComponent<Button>();
			}

			if (uiButton == null && button == null && handler == null) {
				UILogger.LogError($" Cannot find redirect buttons for {target.GetFullPath()}!");
				return;
			}

			if (handler != null) {
				redirect.PointerClick = x => {
					handler.DoClick();
					LockAreaClick?.Invoke();
				};
				return;
			}

			redirect.PointerDown = x => {
				if (uiButton) uiButton.OnPointerDown(x);

				if (button) button.OnPointerDown(x);
			};
			redirect.PointerClick = x => {
				if (button) button.OnPointerClick(x);

				LockAreaClick?.Invoke();
			};

			redirect.PointerUp = x => {
				if (uiButton) uiButton.OnPointerUp(x);

				if (button) button.OnPointerUp(x);
			};
			redirect.PointerEnter = x => {
				if (uiButton) uiButton.OnPointerEnter(x);

				if (button) button.OnPointerEnter(x);
			};
			redirect.PointerExit = x => {
				if (uiButton) uiButton.OnPointerExit(x);

				if (button) button.OnPointerExit(x);
			};
		}

		private void UpdateView() {
			if (!_view) return;

			var needLock = IsLocked;

			//UILogger.Log($"[Locker] needLock={needLock}; lockers={_lockers.JoinToString()}; unlockers={_unlockers.JoinToString()}");

			CancelDelay();

			if (needLock)
				_view.SetLockerVisible(true);
			else {
				_delayCts = new CancellationTokenSource();
				UniTask.Delay(100, cancellationToken: _delayCts.Token)
					.OnComplete(() => {
						if (_view) _view.SetLockerVisible(false);
					});
			}

			if (_redirect.HasValue && !_lockers.Contains(_redirect.Value)) ResetRedirect();

			LockChanged?.Invoke();
		}

		private void CancelDelay() {
			_delayCts?.Cancel();
			_delayCts = null;
		}

		private void ResetRedirect() {
			var redirect = _view.Redirect;
			if (!redirect) return;

			_redirect = null;
			redirect.gameObject.SetActive(false);

			redirect.PointerDown = null;
			redirect.PointerClick = null;
			redirect.PointerUp = null;
			redirect.PointerEnter = null;
			redirect.PointerExit = null;
		}

		private void FocusOverCanvas(Component target, Camera targetCamera) {
			var redirect = _view.Redirect;

			var viewCamera = _view.GetTopmostCanvas().worldCamera;

			var worldTm = (RectTransform)target.transform;
			var redirectTm = (RectTransform)redirect.transform;
			var parentTm = (RectTransform)redirectTm.parent;

			var rect = worldTm.GetWorldRect();

			rect = UIExtensions.ConvertWorldRect(rect, targetCamera, viewCamera);
			rect = parentTm.ToLocalSpace(rect, viewCamera);

			redirectTm.anchoredPosition = rect.center;
			redirectTm.sizeDelta = rect.size;
		}

		private void FocusOverWorld(Component target, Camera srcCamera) {
			var redirect = _view.Redirect;

			var uiCamera = _view.GetTopmostCanvas().worldCamera;

			var redirectTm = (RectTransform)redirect.transform;
			var parentTm = (RectTransform)redirectTm.parent;

			var colliders = target.GetComponentsInChildren<BoxCollider>(true);
			var collider = colliders.FirstOrDefault(x => x.gameObject.activeInHierarchy);
			if (collider == null) {
				UILogger.LogError($"Cannot find {nameof(BoxCollider)} in object {target.GetFullPath()}");
				return;
			}

			var extents = collider.bounds.extents;

			var position = collider.transform.position;
			var p1 = srcCamera.WorldToScreenPoint(position + new Vector3(extents.x, extents.y, extents.z));
			var p2 = srcCamera.WorldToScreenPoint(position + new Vector3(extents.x, extents.y, -extents.z));
			var p3 = srcCamera.WorldToScreenPoint(position + new Vector3(extents.x, -extents.y, extents.z));
			var p4 = srcCamera.WorldToScreenPoint(position + new Vector3(extents.x, -extents.y, -extents.z));
			var p5 = srcCamera.WorldToScreenPoint(position + new Vector3(-extents.x, extents.y, extents.z));
			var p6 = srcCamera.WorldToScreenPoint(position + new Vector3(-extents.x, extents.y, -extents.z));
			var p7 = srcCamera.WorldToScreenPoint(position + new Vector3(-extents.x, -extents.y, extents.z));
			var p8 = srcCamera.WorldToScreenPoint(position + new Vector3(-extents.x, -extents.y, -extents.z));

			var maxX = Mathf.Max(p1.x, p2.x, p3.x, p4.x, p5.x, p6.x, p7.x, p8.x);
			var minX = Mathf.Min(p1.x, p2.x, p3.x, p4.x, p5.x, p6.x, p7.x, p8.x);
			var maxY = Mathf.Max(p1.y, p2.y, p3.y, p4.y, p5.y, p6.y, p7.y, p8.y);
			var minY = Mathf.Min(p1.y, p2.y, p3.y, p4.y, p5.y, p6.y, p7.y, p8.y);

			var lb = new Vector2(minX, minY);
			var rt = new Vector2(maxX, maxY);

			lb = uiCamera.ScreenToWorldPoint(lb);
			rt = uiCamera.ScreenToWorldPoint(rt);

			var rect = new Rect(lb, rt - lb);

			rect = parentTm.ToLocalSpace(rect, uiCamera);

			redirectTm.anchoredPosition = rect.center;
			redirectTm.sizeDelta = new Vector2(Mathf.Abs(rect.size.x), Mathf.Abs(rect.size.y));
		}

	}

}