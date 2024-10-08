using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using XLib.Core.Utils;
using XLib.UI.Screens;

namespace XLib.UI.Internal {

	internal partial class ScreenManager {
		public T GetLastFromStack<T>() where T : IUIScreen {
			var result = _screenStack.HistoryStack.Select(s => s.ScreenInstance.Screen).OfType<T>().LastOrDefault();
			if (result != null) return result;
			UILogger.LogError($"Cannot find screen of type '{TypeOf<T>.Name}' in stack!");
			return default;
		}

		public async UniTask CloseAllAboveLast(Type screenType, bool instant) {
			var screenInst = await GetScreenInstance(screenType);
			if (screenInst?.Screen == null) {
				UILogger.LogError($"Cannot find screen of type '{screenType.Name}'");
				return;
			}

			var screenEntry = _screenStack.GetLastScreens(screenInst).FirstOrDefault();
			if (screenEntry == null) return;

			var screens = _screenStack.PopAllScreensAboveLast(screenEntry).ToArray();
			if (screens.IsNullOrEmpty()) return;

			await CloseScreens(screens, instant);
		}
	}

}