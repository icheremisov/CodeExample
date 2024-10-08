#if UNITY_EDITOR

using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;

namespace XLib.Unity.Utils {

	public class SimpleSelector<T> : SimpleSelectorBase<T> {
		private readonly IEnumerable<T> _source;

		public SimpleSelector(IEnumerable<T> source) {
			_source = source;
		}

		protected override void BuildSelectionTree(OdinMenuTree tree) {
			tree.Selection.SupportsMultiSelect = false;
			tree.Config.DrawSearchToolbar = true;
			tree.Config.AutoFocusSearchBar = true;

			foreach (var item in tree.AddRange(_source, x => $"{x}")) {
				if (item.Value is T i) item.SearchString = $"{i.ToString()}";
			}
		}
	}

}

#endif