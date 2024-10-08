using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.UI.Screens;
using XLib.UI.Types;
using XLib.UI.Views;

namespace XLib.UI.Contracts {

	public delegate void ScreenEventHandler(Type screenType);

	public interface IScreenManager {
		IUIScreen TopVisibleScreen { get; }
		IUIScreen LastOpenedScreen { get; }

		int HistoryCount { get; }
		IEnumerable<IUIScreen> HistoryStack { get; }

		/// <summary>
		///     Set in TutorialController and depends on presence of active TutorialSteps. Implemented to disallow physical back
		///     btn presses on devices
		/// </summary>
		Func<bool> CanGoBack { set; }

		bool IsAnyAnimInProgress { get; }
		event ScreenEventHandler BeforeScreenShown;
		event ScreenEventHandler AfterScreenShown;

		event ScreenEventHandler BeforeScreenHidden;
		event ScreenEventHandler AfterScreenHidden;

		Func<UIScreen, UIBuiltInAction, bool, UniTask> BuiltInAction { get; set; }

		void Register(UIView view);
		void Unregister(UIView view);

		UniTask TryPreloadScreen<T>() where T : IUIScreen;
		UniTask TryUnloadScreen<T>(bool transition = false) where T : IUIScreen;

		UniTask UnloadAllScreens(IList<Type> exceptList, bool transition = false);

		public TScreen GetScreenOrDefault<TScreen>() where TScreen : UIScreen;
		public UniTask<TScreen> GetUIScreen<TScreen>() where TScreen : UIScreen;
		internal UniTask SetScreenArgs<TScreen, TArgs>(TArgs args) where TScreen : IUIScreen, IUIScreenSetup<TArgs> where TArgs : IScreenArgs;
		internal UniTask<UIScreenEntry> Show(Type screenType, bool transition);
		UniTask ShowScreenWithType(Type screenType, bool transition);
		UniTask CloseLast(Type screenType, bool transition);
		UniTask CloseAllAboveLast(Type screenType, bool instant = false);

		public UniTask RunTaskWithBlockerView(ScreenLockTag tag, Func<Func<UniTask>, UniTask> task, Func<UniTask> doAfterVisible = null,
			IUIScreen screen = null);

		/// <summary>
		///     get screen from stack
		/// </summary>
		T GetLastFromStack<T>() where T : IUIScreen;

		bool IsAnimationInProgress<T>() where T : IUIScreen;

		UniTask PerformAction(UIScreen screen, UIBuiltInAction buttonAction, bool transition);

		/// <summary>
		/// create component and inject dependencies 
		/// </summary>
		T InstantiateUIPrefab<T>(T prefab, Transform parent) where T : MonoBehaviour;

		/// <summary>
		/// create component and inject dependencies 
		/// </summary>
		T InstantiateUIPrefab<T>(GameObject prefab, Transform parent) where T : MonoBehaviour;

		/// <summary>
		/// create component and inject dependencies 
		/// </summary>
		GameObject InstantiateUIPrefab(GameObject prefab, Transform parent);
	}

}