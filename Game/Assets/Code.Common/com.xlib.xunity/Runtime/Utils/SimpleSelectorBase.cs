#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;

namespace XLib.Unity.Utils {

	public abstract class SimpleSelectorBase<T> : OdinSelector<T> {
		private const int MaxDoubleClickMS = 500;

		protected SimpleSelectorBase() => SelectionChanged += OnSelectCheckDoubleClick;

		private DateTime _lastClick;
		private int _lastHash;

		private void OnSelectCheckDoubleClick(IEnumerable<T> obj) {
			if (!SelectionTree.Config.ConfirmSelectionOnDoubleClick) return;

			var selectHash = SelectionTree.Selection.Aggregate(0, (v, item) => HashCode.Combine(v, item.FlatTreeIndex));
			if (selectHash == 0) return;

			if ((DateTime.Now - _lastClick).Milliseconds < MaxDoubleClickMS && selectHash == _lastHash) {
				SelectionTree.Selection.ConfirmSelection();
			}

			_lastHash = selectHash;
			_lastClick = DateTime.Now;
		}
	}

}
#endif