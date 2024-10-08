using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneOf.Types;
using UnityEngine;
using XLib.Unity.Utils;

namespace XLib.Unity.Editors {

	public class EditorToolbar {
		private List<ToolbarItem> _list = new(8);
		private GUIStyle _toolbarStyle;
		private GUIStyle _toolbarStyleMenu;

		private class ToolbarItem {
			public GUIContent ItemName;
			public Func<string> NameAction;
			public bool Menu;
			public Action Action;
			public Color? Color;
			public GUIContent Name {
				get {
					if (NameAction != null) ItemName.text = NameAction?.Invoke();
					return ItemName;
				}
			}
		}

		public void AddItem(string name, Action action, bool menu = false, Color? color = null) => AddItemInternal(name, null, null, action, menu, color);
		public void AddItem(Func<string> getName, Action action, bool menu = false, Color? color = null) => AddItemInternal(string.Empty, null, getName, action, menu, color);

		public void AddItem(Func<string> getName, Texture icon, Action action, bool menu = false, Color? color = null) =>
			AddItemInternal(string.Empty, icon, getName, action, menu, color);
		
		public void Draw() {
			if (_toolbarStyle == null) _toolbarStyle = "toolbarbutton";
			if (_toolbarStyleMenu == null) _toolbarStyleMenu = "ToolbarPopup";

			GUILayout.BeginHorizontal();
			foreach (var item in _list) {
				var name = item.Name;
				if (name.text.IsNullOrEmpty()) continue;

				IDisposable colorUsing = null;
				if (item.Color != null) colorUsing = GuiEx.BackgroundColor(item.Color.Value);
				if (GUILayout.Button(name, item.Menu ? _toolbarStyleMenu : _toolbarStyle)) {
					item.Action?.Invoke();
				}

				colorUsing?.Dispose();
			}

			GUILayout.EndHorizontal();
		}

		public void Clear() => _list.Clear();
		private void AddItemInternal(string name, Texture icon, Func<string> getName, Action action, bool menu = false, Color? color = null) {
			_list.Add(new ToolbarItem() {
				ItemName = new GUIContent(name, icon),
				NameAction = getName,
				Menu = menu,
				Action = action,
				Color = color
			});
		}

	}

}