using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using XLib.UI.Screens;
using XLib.UI.Types;
using XLib.Unity.Cameras;

[assembly: InternalsVisibleTo("XLib.UI.Editor.Tests")]

namespace XLib.UI.Internal {

	internal class UIScreenStack {
		public IUIScreen TopVisibleScreen => _visualStack.Count > 0 ? _visualStack.Last().Screen : null;
		public IUIScreen LastOpenedScreen => HistoryStack.Count > 0 ? HistoryStack.Last().ScreenInstance.Screen : null;

		private readonly List<UIScreenEntry> _historyStack = new(16);
		private readonly List<UIScreenInstance> _visualStack = new(16);

		public IReadOnlyList<UIScreenEntry> HistoryStack => _historyStack;
		public IEnumerable<UIScreenEntry> VisualStack => OrderHistoryStack(_historyStack);

		public UIScreenEntry AddToHistoryStack(UIScreenInstance screenInstance) {
			UIScreenEntry result;
			if (screenInstance.Screen is IUiScreenWithStateResult screenWithStateResult)
				result = new UIScreenEntryWithResult(screenInstance, screenWithStateResult);
			else
				result = new UIScreenEntry(screenInstance);

			_historyStack.Add(result);
			return result;
		}

		public void RemoveFromHistory(UIScreenEntry screenEntry) {
			_historyStack.Remove(screenEntry);
		}

		public void RemoveFromHistory(IEnumerable<UIScreenEntry> screenEntries) {
			_historyStack.RemoveAll(screenEntries.Contains);
		}

		public IEnumerable<UIScreenEntry> PopAllScreensAboveLast(UIScreenEntry screenContainer) {
			var toClose = new List<UIScreenEntry>();
			var screenIdx = _historyStack.LastIndexOf(screenContainer);
			for (var i = _historyStack.Count - 1; i > screenIdx; i--) toClose.Add(_historyStack[i]);
			return toClose;
		}

		private static IEnumerable<UIScreenEntry> OrderHistoryStack(IEnumerable<UIScreenEntry> historyStack) {
			return
				historyStack
					.OrderBy(x => x.ScreenInstance.Screen.Style.HasFlag(ScreenStyle.System))
					.ThenBy(y => y.ScreenInstance.Screen.Style.HasFlag(ScreenStyle.AlwaysOnTop));
		}

		public async UniTask UpdateVisualStack() {
			try {
				CameraLayerManager.S.BeginUpdate();

				_visualStack.Clear();
				_visualStack.AddRange(OrderHistoryStack(_historyStack).Select(x => x.ScreenInstance));
				for (var i = 0; i < _visualStack.Count; i++) _visualStack[i].VisualOrder = i;
			}
			finally {
				await CameraLayerManager.S.EndUpdate();
			}
		}

		public UIScreenEntry[] GetLastScreens(UIScreenInstance screenInstance) {
			UIScreenEntry[] result;
			var screenType = screenInstance.Screen.GetType();
			var screenHistoryIdx = _historyStack.LastIndexOf(x => x.ScreenType == screenType);
			if (screenHistoryIdx < 0) return Array.Empty<UIScreenEntry>();
			var nextMainScreenIdx = -1;

			for (var i = screenHistoryIdx + 1; i < _historyStack.Count; i++) {
				if (_historyStack[i].ScreenInstance.Screen.ScreenHierarchyType != ScreenStateType.Main) continue;
				nextMainScreenIdx = i;
				break;
			}

			switch (screenInstance.Screen.ScreenHierarchyType) {
				case ScreenStateType.Main: {
					if (nextMainScreenIdx <= screenHistoryIdx) nextMainScreenIdx = _historyStack.Count;
					result = new UIScreenEntry[nextMainScreenIdx - screenHistoryIdx];
					nextMainScreenIdx--;
					for (var i = 0; i < result.Length; i++) result[i] = _historyStack[nextMainScreenIdx - i];
					break;
				}

				case ScreenStateType.Child: {
					result = new[] { _historyStack[screenHistoryIdx] };
					break;
				}

				default: throw new ArgumentOutOfRangeException();
			}

			return result;
		}

		public bool IsTopOfType(UIScreenEntry screenEntry) {
			var topScreenEntryOfSameType = _historyStack.LastOrDefault(x => x.ScreenType == screenEntry.ScreenType);
			return topScreenEntryOfSameType == null || topScreenEntryOfSameType == screenEntry;
		}
	}

}