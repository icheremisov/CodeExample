using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using XLib.Core.AsyncEx;
using XLib.Core.Utils;
using XLib.UI.ConnectionBlocker;
using XLib.UI.Contracts;
using XLib.UI.Screens;
using XLib.UI.Types;
using XLib.UI.Views;
using XLib.Unity.Cameras;
using XLib.Unity.Installers;
using Zenject;

namespace XLib.UI.Internal {

	internal static class ScreenManagerDelegateUtils {
		public static void SafeInvoke(this ScreenEventHandler self, Type screenType) {
			try {
				self?.Invoke(screenType);
			}
			catch (Exception e) {
				UILogger.LogError($"Error while invoking ScreenEventHandler: {e}");
			}
		}
	}

	internal partial class ScreenManager : IScreenManager, ITickable {
		private const float BlockerMinTimeout = 0.5f;
		private const float ShowBlockerAfterSec = 0.5f;

		private const string SwitchScreenTag = "Screen${0}";

		public event ScreenEventHandler BeforeScreenShown;
		public event ScreenEventHandler AfterScreenShown;

		public event ScreenEventHandler BeforeScreenHidden;
		public event ScreenEventHandler AfterScreenHidden;

		public Func<UIScreen, UIBuiltInAction, bool, UniTask> BuiltInAction { get; set; }

		public IUIScreen TopVisibleScreen => _screenStack.TopVisibleScreen;
		public IUIScreen LastOpenedScreen => _screenStack.LastOpenedScreen;

		public int HistoryCount => _screenStack.HistoryStack.Count;
		public IEnumerable<IUIScreen> HistoryStack => _screenStack.HistoryStack.Select(x => x.ScreenInstance.Screen);

		public Func<bool> CanGoBack { get; set; }

		public bool IsAnyAnimInProgress => _screenLoader.LoadedScreens.Any(x => x.Value.Screen.State == ScreenState.Showing || x.Value.Screen.State == ScreenState.Hiding);

		private readonly LazyBinding<IScreenLocker> _screenLocker;

		private readonly UIScreenStack _screenStack;
		private readonly UIScreenLoader _screenLoader;
		private readonly IBlockerView _blockerView;
		private readonly ITransitionScreen _transitionScreen;

		private UniversalAdditionalCameraData _camDataCached;

		public ScreenManager(
			LazyBinding<IScreenLocker> screenLocker,
			UIScreenLoader uiScreenLoader,
			IBlockerView blockerView,
			ITransitionScreen transitionScreen) {
			_screenLocker = screenLocker;

			_screenStack = new UIScreenStack();
			_screenLoader = uiScreenLoader;
			_blockerView = blockerView;
			_transitionScreen = transitionScreen;
		}

		public bool IsAnimationInProgress<T>() where T : IUIScreen {
			var result = _screenLoader.LoadedScreens.FirstOrDefaultType<T>();
			if (result == null) return false;

			return result.State == ScreenState.Showing || result.State == ScreenState.Hiding;
		}

		public async UniTask PerformAction(UIScreen screen, UIBuiltInAction buttonAction, bool transition) {
			switch (buttonAction) {
				case UIBuiltInAction.Close:
					if (this.IsInStack(screen.GetType())) await screen.CloseAsync(transition);
					break;

				default:
					if (BuiltInAction != null) await BuiltInAction.Invoke(screen, buttonAction, transition);
					break;
			}
		}

		private void CheatSetScreensHidden(bool v) {
			if (UIScreenLayer.HideScreensCheat == v) return;

			UIScreenLayer.HideScreensCheat = v;
			UpdateVisualStack().Forget();
		}

		public void Register(UIView view) {
			view.BeforeShown += ViewOnBeforeShown;
		}

		public void Unregister(UIView view) {
			view.BeforeShown -= ViewOnBeforeShown;
		}

		private UniTask ViewOnBeforeShown(UIView view) => UpdateVisualStack();

		private async UniTask UpdateVisualStack() {
			_screenLoader.UpdateScreenPositions();
			await _screenStack.UpdateVisualStack();
		}

		public UniTask TryPreloadScreen<T>() where T : IUIScreen {
			return _screenLoader.GetOrLoadScreenView(TypeOf<T>.Raw);
		}

		public async UniTask TryUnloadScreen<T>(bool transition = false) where T : IUIScreen {
			var screenType = TypeOf<T>.Raw;
			if (this.IsInStack(screenType)) await CloseLast(screenType, transition);
			if (_screenStack.HistoryStack.All(x => x.ScreenType != screenType)) await _screenLoader.Unload(screenType);
		}

		public async UniTask UnloadAllScreens(IList<Type> exceptList, bool transition = false) {
			await this.CloseAllScreens(transition);
			await _screenLoader.UnloadAll(exceptList);
		}

		async UniTask IScreenManager.SetScreenArgs<TScreen, TArgs>(TArgs args) {
			await ScreenIsInCloseQueue(TypeOf<TScreen>.Raw);

			var screenInstance = await GetScreenInstance(TypeOf<TScreen>.Raw);
			if (screenInstance.Screen == null) {
				UILogger.LogError($"Cannot find screen of type '{TypeOf<TScreen>.Raw}'");
				return;
			}

			await TrySaveScreenState(screenInstance);
			if (args != null) await ((IUIScreenSetup<TArgs>)screenInstance.Screen).SetupScreen(args);
		}

		private static void FireStackChanged() => CameraLayerManager.S.UpdateStack().Forget();

		private void SetScreenScrolls(UIScreenInstance screen) {
			var rootCanvas = ((UIView)screen.Screen).GetTopmostCanvas();
			rootCanvas.GetRectTransform().AdjustScrollBasedOnContent();
		}

		public void Tick() {
			// disable android back button by gamedesign
			// 	if (Input.GetKeyUp(KeyCode.Escape) && (CanGoBack == null || CanGoBack())) {
			// 		var top = (UIScreen)TopVisibleScreen;
			// 		if (top is { State: ScreenState.Visible } && top.Style.Has(ScreenStyle.HandleAndroidBackButton) && top.CanPerform(UIBuiltInAction.Close)) PerformAction(top, UIBuiltInAction.Close);
			// 	}

#if UNITY_EDITOR || PLATFORM_STANDALONE
			if (Input.GetKeyUp(KeyCode.Space)) CheatSetScreensHidden(!UIScreenLayer.HideScreensCheat);
#endif
		}

		public TScreen GetScreenOrDefault<TScreen>() where TScreen : UIScreen {
			return (TScreen)_screenLoader.GetOrDefault(TypeOf<TScreen>.Raw)?.Screen;
		}

		public override string ToString() {
			return $"[stack={_screenStack.HistoryStack.Select(x => x.ScreenType.Name).JoinToString()}]";
		}

		public T InstantiateUIPrefab<T>(T prefab, Transform parent) where T : MonoBehaviour => _screenLoader.InstantiateUIPrefab(prefab, parent).GetExistingComponent<T>();

		public T InstantiateUIPrefab<T>(GameObject prefab, Transform parent) where T : MonoBehaviour => _screenLoader.InstantiateUIPrefab(prefab, parent).GetExistingComponent<T>();

		public GameObject InstantiateUIPrefab(GameObject prefab, Transform parent) => _screenLoader.InstantiateUIPrefab(prefab, parent);

		public async UniTask RunTaskWithBlockerView(ScreenLockTag tag, Func<Func<UniTask>, UniTask> task, Func<UniTask> doAfterVisible = null,
			IUIScreen screen = null) {
			var cts = new CancellationTokenSource();
			var evVis = new AsyncEvent();

			TryShowBlocker(evVis, cts.Token).Forget(() => cts.Cancel());

			await task(() => WaitShow(cts));

			return;

			async UniTask TryShowBlocker(AsyncEvent ev, CancellationToken ct) {
				if (screen?.State is ScreenState.Visible) return;
				await UniEx.DelaySec(ShowBlockerAfterSec, ct);
				_blockerView.Open(tag);
				await UniEx.DelaySec(BlockerMinTimeout, ct);
				ev.FireEvent();
			}

			async UniTask WaitShow(CancellationTokenSource tokenSource) {
				try {
					if (_blockerView.HasTag(tag) && !tokenSource.IsCancellationRequested) await evVis.WaitAsync(tokenSource.Token);
				}
				finally {
					tokenSource.Cancel();
				}

				_blockerView.Close(tag);
				if (doAfterVisible != null) await doAfterVisible();
			}
		}
	}

}