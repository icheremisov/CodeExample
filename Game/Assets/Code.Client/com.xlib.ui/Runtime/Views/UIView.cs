using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using XLib.Core.Utils;
using XLib.UI.Contracts;
using XLib.UI.Internal;
using XLib.UI.Types;
using Zenject;

namespace XLib.UI.Views {

	/// <summary>
	///     base class for all UI views
	/// </summary>
	[RequireComponent(typeof(RectTransform)), RequireComponent(typeof(Canvas)), RequireComponent(typeof(GraphicRaycaster)),
	 RequireComponent(typeof(CanvasGroup))]
	public abstract class UIView : MonoBehaviour, ILockable {
#pragma warning disable CS0414 // Field is assigned but its value is never used
		private float _animationTargetValue = -1;
#pragma warning restore CS0414 // Field is assigned but its value is never used

		private RectTransform _rectTm;
		private int _locks;
		private protected IScreenLocker ScreenLockerInternal;
		private IScreenManager _screenManager;
		protected RectTransform RectTransform => _rectTm != null ? _rectTm : _rectTm = (RectTransform)transform;

		private CanvasGroup _canvasGroupCached;
		private CanvasGroup CanvasGroup => _canvasGroupCached ??= GetComponent<CanvasGroup>();
		private Canvas _canvas;
		private Canvas Canvas => _canvas ??= this.GetTopmostCanvas();
		private Camera _camera;
		private Camera Camera => _camera ??= Canvas.worldCamera;
		protected bool IsVisible => gameObject.activeSelf && CanvasGroup.alpha != 0 && Camera.enabled;

		protected virtual void OnDestroy() {
			_screenManager?.Unregister(this);
		}

		protected virtual void InitializeView() { }

		private protected virtual void InitializeInternal() { }

		public event Func<UIView, UniTask> BeforeShown;
		public event Func<UIView, UniTask> OnShown;
		public event Func<UIView, UniTask> BeforeHidden;
		public event Func<UIView, UniTask> OnHidden;

		[Inject]
		private void Construct(IScreenManager screenManager, IScreenLocker screenLocker) {
			_screenManager = screenManager;
			ScreenLockerInternal = screenLocker;
			Initialize();
		}

		private void Initialize() {
			_screenManager.Register(this);
			InitializeInternal();
			InitializeView();
			gameObject.SetActive(false);
		}

		private protected UniTask ShowViewAsync(Func<UniTask> setVisible) {
#if VIEW_LOGS
			UILogger.Log($"[{GetType().Name}] Show");
#endif

			return ShowInternalAsync(setVisible);
		}

		private protected UniTask HideViewAsync(Func<UniTask> doAfterHide) {
#if VIEW_LOGS
			UILogger.Log($"[{GetType().Name}] Hide");
#endif

			return HideInternalAsync(doAfterHide);
		}

		private protected void PreWarm() {
			CanvasGroup.alpha = 0.0f;
			gameObject.SetActive(true);
		}

		private async UniTask ShowInternalAsync(Func<UniTask> setVisible) {
#if VIEW_LOGS
			UILogger.Log($"[{GetType().Name}] ViewOnBeforeShown");
#endif
			if (BeforeShown != null) await BeforeShown(this);

			await BeforeShowAsync();

			CanvasGroup.alpha = 1.0f;

			await setVisible();

			if (OnShown != null) await OnShown(this);

			await AfterShowAsync();
		}

		private async UniTask HideInternalAsync(Func<UniTask> doAfterHide) {
			if (BeforeHidden != null) await BeforeHidden(this);

			await BeforeHideAsync();

			if (OnHidden != null) await OnHidden(this);

			await doAfterHide();
			
			await AfterHideAsync();
		}

		protected virtual UniTask BeforeShowAsync() => UniTask.CompletedTask;

		protected virtual UniTask AfterShowAsync() => UniTask.CompletedTask;

		protected virtual UniTask BeforeHideAsync() => UniTask.CompletedTask;

		protected virtual UniTask AfterHideAsync() => UniTask.CompletedTask;

		public LockInstance Lock() {
			++_locks;
			if (_locks == 1) ScreenLockerInternal.LockScreen(new ScreenLockTag(name));

			return new LockInstance(this);
		}

		void ILockableInternal.Unlock(LockInstance inst) {
			--_locks;
			if (_locks != 0) return;
			if (CanvasGroup != null) ScreenLockerInternal.UnlockScreen(new ScreenLockTag(name));
		}

		public bool IsLocked => _locks > 0;

#if UNITY_EDITOR
		[Button]
		public void SetEditorBuildSettingsScenes() {
			var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
			var newScene = new EditorBuildSettingsScene(gameObject.scene.path, true);
			if (scenes.FirstOrDefault(x => x.path == newScene.path) != null) return;
			scenes.Add(newScene);
			EditorBuildSettings.scenes = scenes.ToArray();
		}

		private bool NoCamera => Canvas == null || Canvas.worldCamera == null;

		[InfoBox("No Camera added to screen", InfoMessageType.Error, "@NoCamera", GUIAlwaysEnabled = true)]
		[PropertyOrder(-1)]
		[Button("Add Camera", ButtonSizes.Medium), GUIColor(1, 0, 0), ShowIf(nameof(NoCamera))]
		public void InstantiateAndSetupCamera() {
			UIScreenLayer.SetupMainCamera(gameObject);
		}
#endif
		public virtual UniTask CamerasVisibleChanged(bool visible) => UniTask.CompletedTask;
	}

}