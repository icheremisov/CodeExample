using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using XLib.Core.Utils;
using XLib.UI.Contracts;
using XLib.UI.Screens;
using XLib.UI.Utils;

namespace XLib.UI.Utils {

	public struct ScreenScope<TScreen> where TScreen : IUIScreen {
		public readonly IScreenManager ScreenManager;

		public ScreenScope(IScreenManager screenManager) {
			ScreenManager = screenManager;
		}
	}

	public struct ScreenScope<TScreen, TResult> where TScreen : UIScreenWithResult<TResult> {
		public readonly IScreenManager ScreenManager;

		public ScreenScope(IScreenManager screenManager) {
			ScreenManager = screenManager;
		}
	}

}

public static class ScreenManagerExtensions {
	public static ScreenScope<TScreen> Screen<TScreen>(this IScreenManager screenManager) where TScreen : IUIScreen => new(screenManager);

	public static ScreenScope<TScreen, TResult> Screen<TScreen, TResult>(this IScreenManager screenManager) where TScreen : UIScreenWithResult<TResult> => new(screenManager);

	public static UniTask Open<TScreen>(this ScreenScope<TScreen> screen, bool transition = false) where TScreen : UIScreen => screen.ScreenManager.Show(TypeOf<TScreen>.Raw, transition);

	public static async UniTask Open<TScreen, TArgs>(this ScreenScope<TScreen> screen, TArgs args, bool transition = false)
		where TScreen : IUIScreen, IUIScreenSetup<TArgs> where TArgs : IScreenArgs {
		await screen.ScreenManager.SetScreenArgs<TScreen, TArgs>(args);
		await screen.ScreenManager.Show(TypeOf<TScreen>.Raw, transition);
	}

	public static async UniTask OpenAndWait<TScreen>(this ScreenScope<TScreen> screen, bool transition = false)
		where TScreen : UIScreen {
		var screenEntry = await screen.ScreenManager.Show(TypeOf<TScreen>.Raw, transition);
		await screenEntry;
	}

	public static async UniTask OpenAndWait<TScreen, TArgs>(this ScreenScope<TScreen> screen, TArgs args, bool transition = false)
		where TScreen : UIScreen, IUIScreenSetup<TArgs> where TArgs : IScreenArgs {
		await screen.ScreenManager.SetScreenArgs<TScreen, TArgs>(args);
		var screenEntry = await screen.ScreenManager.Show(TypeOf<TScreen>.Raw, transition);
		await screenEntry;
	}

	public static async UniTask<TResult> ShowAndWaitResult<TScreen, TResult>(this ScreenScope<TScreen, TResult> screen, bool transition = false) where TScreen : UIScreenWithResult<TResult> {
		var screenEntry = await screen.ScreenManager.Show(TypeOf<TScreen>.Raw, transition);
		var screenEntryWithResult = screenEntry as UIScreenEntryWithResult;
		return (TResult)await screenEntryWithResult;
	}

	public static async UniTask<TResult> ShowAndWaitResult<TScreen, TArgs, TResult>(this ScreenScope<TScreen, TResult> screen, TArgs args, bool transition = false)
		where TScreen : UIScreenWithResult<TResult>, IUIScreenSetup<TArgs> where TArgs : IScreenArgs {
		await screen.ScreenManager.SetScreenArgs<TScreen, TArgs>(args);
		var screenEntry = await screen.ScreenManager.Show(TypeOf<TScreen>.Raw, transition);
		var screenEntryWithResult = screenEntry as UIScreenEntryWithResult;
		return (TResult)await screenEntryWithResult;
	}

	public static UniTask Close<TScreen>(this ScreenScope<TScreen> screen, bool transition = false) where TScreen : IUIScreen => screen.ScreenManager.CloseLast(TypeOf<TScreen>.Raw, transition);

	public static bool IsInStack<T>(this IScreenManager manager)
		where T : IUIScreen {
		bool Selector(IUIScreen x) => x is T;
		return manager.HistoryStack.Any(Selector);
	}

	public static bool IsInStack(this IScreenManager manager, Type type) {
		bool Selector(IUIScreen x) => x.GetType() == type;
		return manager.HistoryStack.Any(Selector);
	}

	public static bool IsOnVisibleTop<T>(this IScreenManager manager)
		where T : IUIScreen =>
		manager.TopVisibleScreen is T;

	public static bool IsOnVisibleTop(this IScreenManager manager, Type type) => manager.TopVisibleScreen != null && manager.TopVisibleScreen.GetType() == type;

	/// <summary>
	///     hide all screens by stack
	/// </summary>
	public static async UniTask CloseAllScreens(this IScreenManager manager, bool transition = false) {
		while (manager.HistoryCount > 0) {
			var topScreen = manager.LastOpenedScreen;
			await manager.CloseLast(topScreen.GetType(), transition);
		}
	}

	/// <summary>
	///     hide all screens by stack excluding 'T' screen and stop
	/// </summary>
	public static async UniTask CloseScreensUntilScreen<T>(this IScreenManager manager)
		where T : IUIScreen {
		var type = TypeOf<T>.Raw;

		while (manager.HistoryCount > 0) {
			var topScreen = manager.LastOpenedScreen;

			if (type.IsInstanceOfType(topScreen)) break;

			await manager.CloseLast(topScreen.GetType(), false);
		}
	}

	/// <summary>
	///     Asynchronously wait for for all animations to finish
	/// </summary>
	public static async UniTask WaitForAllAnimAsync(this IScreenManager manager, CancellationToken ct = default) {
		while (manager.IsAnyAnimInProgress) await UniTask.NextFrame(ct);
	}
}