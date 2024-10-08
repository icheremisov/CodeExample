using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using XLib.Core.Utils;
using XLib.UI.Contracts;
using XLib.UI.Screens;
using XLib.UI.Types;
using XLib.Unity.Cameras;

namespace XLib.UI.Internal {

	internal partial class ScreenManager {
		private readonly List<UIScreenEntry> _screensClosing = new(5);

		public async UniTask<TScreen> GetUIScreen<TScreen>() where TScreen : UIScreen {
			return (TScreen)(await _screenLoader.GetOrLoadScreenView(TypeOf<TScreen>.Raw)).Screen;
		}

		private async UniTask<UIScreenInstance> GetScreenInstance(Type screenType) {
			var screen = await _screenLoader.GetOrLoadScreenView(screenType);
			if (screen.Screen != null) return screen;
			UILogger.LogError($"Cannot find screen of type '{screenType}'");
			return null;
		}

		public async UniTask ShowScreenWithType(Type screenType, bool transition) {
			await ((IScreenManager)this).Show(screenType, transition);
		}

		async UniTask<UIScreenEntry> IScreenManager.Show(Type screenType, bool transition) {
			if (transition) _transitionScreen.MarkShow();
			
			UIScreenInstance screenInst;

			await ScreenIsInCloseQueue(screenType);

			var tag = new ScreenLockTag(string.Format(SwitchScreenTag, screenType.Name));

			UIScreenEntry screenEntry = null;
			try {
				_screenLocker.ValueOrNull?.LockScreen(tag);

				screenInst = await GetScreenInstance(screenType);
				if (screenInst?.Screen == null) {
					UILogger.LogError($"Cannot find screen of type '{screenType.Name}'");
					return null;
				}

				if (screenInst.Screen.State is not (ScreenState.Visible or ScreenState.Hidden)) {
					UILogger.LogWarning($"Trying to Open screen {screenType.Name} which state is {screenInst.Screen.State}");

					while (screenInst.Screen.State is not (ScreenState.Visible or ScreenState.Hidden)) await UniTask.DelayFrame(1);
					if (screenInst.Screen.State == ScreenState.Visible) return null;
				}

				if (screenInst.Screen is not IUIScreen uiScreen) throw new Exception($"Type {screenType} must be instance of {nameof(IUIScreen)}");
				

				if (screenInst.Screen is IBeforeScreenDynamicCameraProvider provider) provider.CameraChanged += FireStackChanged;

				var forceRefresh = await TryCloseScreensBeforeOpen(screenInst);

				screenEntry = _screenStack.AddToHistoryStack(screenInst);

				BeforeScreenShown.SafeInvoke(screenType);

				await RunTaskWithBlockerView(tag, waitVisible => uiScreen.OpenInternal(waitVisible, forceRefresh),
					() => SetObjectsVisible(screenInst, true), screen: screenInst.Screen);

				if (forceRefresh) await UpdateVisualStack();
				
				AfterScreenShown.SafeInvoke(screenType);
			}
			catch (Exception) {
				if (screenEntry != null) _screenStack.RemoveFromHistory(screenEntry);
				throw;
			}
			finally {
				_blockerView.Close(tag);
				_screenLocker.ValueOrNull?.UnlockScreen(tag);
			}

			SetScreenScrolls(screenInst);
			return screenEntry;
		}

		public async UniTask CloseLast(Type screenType, bool transition) {
			if (transition) _transitionScreen.MarkShow();
			
			var tag = new ScreenLockTag(string.Format(SwitchScreenTag, screenType.Name));
			try {
				_screenLocker.ValueOrNull?.LockScreen(tag);

				await ScreenIsInCloseQueue(screenType);

				var screenInst = await GetScreenInstance(screenType);

				if (screenInst?.Screen == null) {
					UILogger.LogError($"Cannot find screen of type '{screenType.Name}'");
					return;
				}

				var screenEntry = _screenStack.GetLastScreens(screenInst).FirstOrDefault();
				if (screenEntry == null) return;

				_screenStack.RemoveFromHistory(screenEntry);
				await CloseScreenInstance(screenEntry, true);
			}
			finally {
				_screenLocker.ValueOrNull?.UnlockScreen(tag);
			}
		}

		private async UniTask<bool> TryCloseScreensBeforeOpen(UIScreenInstance screenInstance) {
			var screens = _screenStack.GetLastScreens(screenInstance);
			if (screens.IsNullOrEmpty()) return false;
			if (screenInstance.Screen.ScreenHierarchyType == ScreenStateType.Main) {
				var mainScreen = screens.Last();
				var isTopMainScreen = _screenStack.IsTopOfType(mainScreen);
				if (!isTopMainScreen)
					await CloseScreens(screens, true);
				else {
					await CloseScreens(screens.Take(screens.Length - 1).ToArray(), false);
					_screenStack.RemoveFromHistory(mainScreen);
					mainScreen.Close();
					return true;
				}
			}
			else {
				var screenEntry = screens.First();
				await screenEntry.SaveState();
				await CloseScreenInstance(screenEntry, true);
			}

			return false;
		}

		private async UniTask TrySaveScreenState(UIScreenInstance screenInstance) {
			if (screenInstance.Screen.ScreenHierarchyType != ScreenStateType.Child) return;
			var screenEntry = _screenStack.GetLastScreens(screenInstance).FirstOrDefault();
			if (screenEntry == null) return;
			await screenEntry.SaveState();
		}

		private async UniTask CloseScreens(UIScreenEntry[] screens, bool instant) {
			if (screens.IsNullOrEmpty()) return;

			if (_screensClosing.Count > 0) {
				UILogger.LogError($"Trying to close screens while another screens are not ended closing '{screens.Select(x => x.ScreenType).JoinToString()}'");
				await UniTask.WaitWhile(() => _screensClosing.Count > 0);
			}

			_screensClosing.AddRange(screens);
			if (instant) {
				foreach (var screenEntry in screens) {
					if (_screenStack.IsTopOfType(screenEntry)) await SetObjectsVisible(screenEntry.ScreenInstance, false);
					_screenStack.RemoveFromHistory(screens);
				}

				await UpdateVisualStack();
			}

			var tag = new ScreenLockTag(string.Format(SwitchScreenTag, screens.Last().ScreenType));

			try {
				_screenLocker.ValueOrNull?.LockScreen(tag);

				foreach (var uiScreenEntry in screens) {
					if (!instant) _screenStack.RemoveFromHistory(screens);
					if (_screenStack.IsTopOfType(uiScreenEntry))
						await CloseScreenInstance(uiScreenEntry, !instant);
					else
						uiScreenEntry.Close();
				}
			}
			finally {
				_screensClosing.RemoveAll(screens.Contains);
				_screenLocker.ValueOrNull?.UnlockScreen(tag);
			}
		}

		private async UniTask CloseScreenInstance(UIScreenEntry screenEntry, bool updateVisualStack) {
			BeforeScreenHidden.SafeInvoke(screenEntry.ScreenType);

			var screen = screenEntry.ScreenInstance.Screen;

			if (screen.State is not (ScreenState.Visible or ScreenState.Hidden)) {
				UILogger.LogWarning($"Trying to Close screen {screenEntry.ScreenType.Name} which state is {screen.State}");

				while (screen.State is not (ScreenState.Visible or ScreenState.Hidden)) await UniTask.DelayFrame(1);
				if (screen.State == ScreenState.Hidden) return;
			}

			await screenEntry.ScreenInstance.Screen.CloseInternal(() => SetObjectsVisible(screenEntry.ScreenInstance, false));
			screenEntry.Close();

			if (screenEntry.ScreenInstance.Screen is IBeforeScreenDynamicCameraProvider provider) provider.CameraChanged -= FireStackChanged;

			await TryRecoverStates();

			AfterScreenHidden.SafeInvoke(screenEntry.ScreenType);
			
			if (updateVisualStack) await UpdateVisualStack();

			if (screenEntry.ScreenInstance.Screen.Style.HasFlag(ScreenStyle.UnloadOnClose)) await _screenLoader.Unload(screenEntry.ScreenType);
		}

		private async UniTask TryRecoverStates() {
			var visualStack = _screenStack.VisualStack.ToList();

			for (var i = visualStack.Count - 1; i >= 0; i--) {
				var screenEntry = visualStack[i];
				var screenInst = screenEntry.ScreenInstance;
				var recovered = await screenEntry.RecoverState();
				if (recovered) {
					await screenInst.Screen.OpenInternal(() => SetObjectsVisible(screenInst, true));
				}

				if (screenInst.Screen.Style.HasFlag(ScreenStyle.FullScreen)) break;
			}
		}

		private static async UniTask SetObjectsVisible(UIScreenInstance screen, bool visible) {
			if (visible) {
				foreach (var layer in screen.ScreenLayers) layer.SetVisible(true);
				await CameraLayerManager.S.RegisterLayer(screen);
			}
			else {
				await CameraLayerManager.S.UnregisterLayer(screen);
				foreach (var layer in screen.ScreenLayers) layer.SetVisible(false);
			}
		}

		private UniTask ScreenIsInCloseQueue(Type screenType) {
			if (_screensClosing.Any(x => x.ScreenType == screenType))
				UILogger.LogError($"screen:'{screenType}' is in que for close");
			else
				return UniTask.CompletedTask;

			return UniTask.WaitWhile(() => _screensClosing.Any(x => x.ScreenType == screenType));
		}
	}

}