using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XLib.Core.Utils;
using XLib.Unity.Editors;
using XLib.Unity.Utils;

namespace XLib.Configs {

	public class ClassTypeSelector : EditorWindow {
		private Action<Type> _callback;

		private struct TypeInfo {
			public Type Type;
			public string Name;
			public string Category;
		}

		private TypeInfo[] _typeList;
		private string _search = string.Empty;
		private Vector2 _scroll;
		private bool _focused;
		private Func<Type, (string category, string name)> _labelFunc;
		private string[] _categories = Array.Empty<string>();
		private int _currentCategory;
		private bool _showNone;

		public static void Show<T>(string title, Action<Type> callback, Func<Type, bool> filter = null,
			bool showNone = false, Func<Type, (string category, string name)> labelFunction = null) {
			var window = CreateInstance<ClassTypeSelector>();
			window.titleContent = new GUIContent(title);
			window.Select<T>(callback, filter, showNone, labelFunction);
			window.ShowModal();
		}

		private void Select<T>(Action<Type> callback, Func<Type, bool> filter, bool showNone,
			Func<Type, (string category, string name)> labelFunction) {
			_callback = callback;
			_labelFunc = labelFunction ?? EditorConfigUtils.ClassName;

			var types = TypeCache<T>.CachedTypes
				.Where(itm => !itm.IsAbstract && itm.Namespace != null &&
					(filter == null || filter(itm)))
				.Select(type => {
					(var category, var name) = _labelFunc(type);
					return new TypeInfo() { Type = type, Name = ObjectNames.NicifyVariableName(name), Category = category ?? "Other" };
				}).OrderBy(info => info.Name);
			_showNone = showNone;
			_typeList = types.ToArray();
			_categories = "*".ToEnumerable().Concat(_typeList.Select(info => info.Category).ToHashSet()).ToArray();
		}

		private void OnGUI() {
			GUI.SetNextControlName("prpr");
			_search = EditorGUILayout.TextField(GUIContent.none, _search, "SearchTextField");
			if (!_focused) {
				EditorGUI.FocusTextInControl("prpr");
				_focused = true;
			}

			string category = null;
			if (_categories.Length > 2 && _search.Length <= 0) {
				GUILayout.Label("Categories", EditorStyles.boldLabel);
				EditorGuiEx.Separator();
				using var _ = GuiEx.BackgroundColor(Color.green);
				_currentCategory = GUILayout.SelectionGrid(_currentCategory, _categories, 5);
				category = _categories[_currentCategory];
				if (category == "*") category = null;
				EditorGuiEx.Separator();
			}

			_scroll = EditorGUILayout.BeginScrollView(_scroll);
			try {
				if (_showNone) {
					using var _ = GuiEx.BackgroundColor(Color.blue);
					if (GUILayout.Button("None")) {
						Close();
						_callback(null);
					}
				}

				var i = 0f;
				var total = (float)_typeList.Length;
				foreach (var info in _typeList) {
					using var _ = GuiEx.BackgroundColor(Color.HSVToRGB(i++ / total, 0.3f, 1.2f));

					if (_search.Length > 0) {
						if (!info.Name.Replace(" ", "").Contains(_search, StringComparison.InvariantCultureIgnoreCase)) continue;
					}
					else {
						if (category != null && info.Category != category) continue;
					}

					if (GUILayout.Button(info.Name)) {
						Close();
						_callback(info.Type);
					}
				}
			}
			finally {
				EditorGUILayout.EndScrollView();
			}
		}
	}

}