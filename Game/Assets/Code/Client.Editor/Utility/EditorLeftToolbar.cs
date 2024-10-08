using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XLib.Unity.Scene;
using XLib.Unity.SceneSet;
using XLib.Unity.Tools;
using XLib.Unity.Utils;

namespace Client.Utility {

	[InitializeOnLoad]
	public static class EditorLeftToolbar {
		private struct SubMenu {
			public SubMenu(string name, Func<IEnumerable<ParsedItemData>> getItems) {
				Name = name;
				GetItems = getItems;
			}

			public string Name { get; set; }
			public Func<IEnumerable<ParsedItemData>> GetItems { get; set; }
		}

		// @formatter:off
		private static readonly object[] Items = {
			$"Assets/Data/Scenes/Initialize.unity", 
			
			new SubMenu("Common >>", SearchCommonScenes),
			new SubMenu("Screens >>", SearchScreenScenes),
			new SubMenu("Environment >>", SearchEnvScenes),
		};

		// @formatter:on

		private struct ParsedItemData {
			public ParsedItemData(string name, string fullName, Action<bool> action) {
				Name = name;
				FullName = fullName;
				Action = action;
				Items = null;
			}

			public ParsedItemData(string name, ParsedItemData[] items) {
				Name = name;
				FullName = name;
				Action = null;
				Items = items;
			}

			public string FullName { get; set; }
			public string Name { get; }
			public Action<bool> Action { get; }
			public ParsedItemData[] Items { get; }
		}

		private static ParsedItemData[] _items;

		static EditorLeftToolbar() {
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

			Refresh();
		}

		private static void Refresh() {
			_items = Items.SelectToArray(MakeItem);
		}

		private static void OpenByPath(bool shiftPressed, string path) {
			if (path.EndsWith("asset")) {
				OpenSceneSet(shiftPressed, path);
				return;
			}
			
			if (shiftPressed) {
				EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
			}
			else {
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) 
					EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
			}
		}

		private static void OpenSceneByName(bool shiftPressed, string name) {
			var foundScenes = EditorUtils.GetAssetPaths<UnityEngine.SceneManagement.Scene>(name);
			if (foundScenes.Length != 1) {
				EditorUtility.DisplayDialog("Error", $"Expected only 1 scene with name '{name}' found found {foundScenes.Length}", "Close");
				return;
			}

			OpenByPath(shiftPressed, foundScenes.First());
		}

		private static void OpenSceneSet(bool shiftPressed, string arg) {
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
				var sceneSet = AssetDatabase.LoadAssetAtPath<SceneSet>(arg);
				SceneSetEditor.Open(sceneSet, shiftPressed);
			}
		}

		private static ParsedItemData MakeItem(object arg) {
			if (arg is string path) {
				var title = Path.GetFileNameWithoutExtension(path);
				return new(title, path, shiftPressed => OpenByPath(shiftPressed, path));
			}
			else if (arg is SubMenu subMenu) {
				return new(subMenu.Name, subMenu.GetItems().ToArray());
			}

			Debug.LogError($"Unknown item: {arg}");
			return new("Unknown", "", _ => { });
		}

		private class ParsedItemDataSelectorBase : SimpleSelectorBase<ParsedItemData> {
			private readonly ParsedItemData[] _source;
			private readonly GUIStyle _helper;

			public ParsedItemDataSelectorBase(ParsedItemData[] source) {
				_source = source;
				_helper = new GUIStyle(GUI.skin.label) {
					alignment = TextAnchor.MiddleRight, fontStyle = FontStyle.Italic, normal = new GUIStyleState() { textColor = Color.gray }
				};
			}

			protected override void BuildSelectionTree(OdinMenuTree tree) {
				tree.Selection.SupportsMultiSelect = false;
				tree.Config.ConfirmSelectionOnDoubleClick = true;
				tree.Config.DrawSearchToolbar = true;
				tree.Config.AutoFocusSearchBar = true;

				foreach (var item in tree.AddRange(_source, x => x.Name)) {
					item.OnDrawItem += OnDrawItem;
				}
			}

			private void OnDrawItem(OdinMenuItem item) {
				if (item.Value is not ParsedItemData data) return;
				var text = new GUIContent(data.FullName.Length > 20 ? $"{data.FullName[..20]}..." : data.FullName, data.FullName);
				var rc = item.LabelRect;
				GUI.Label(new Rect(rc.xMax - rc.width / 2 - 20, rc.y, rc.width / 2, rc.height), text, _helper);
			}
		}

		private static void OnToolbarGUI() {
			if (_items.IsNullOrEmpty()) _items = Items.SelectToArray(MakeItem);
			// GUILayout.FlexibleSpace();
			foreach (var item in _items) {
				if (item.Action != null) {
					if (GUILayout.Button(new GUIContent(item.Name, "Open scene"), (GUIStyle)"toolbarbutton", GUILayout.Height(ToolbarExtender.defaultHeight-2))) {
						item.Action(Event.current.shift);
						GUIUtility.ExitGUI();
						return;
					}
				}
				else if (item.Items.Length > 0) {
					if (GUILayout.Button(new GUIContent(item.Name, "Open scene"), (GUIStyle)"toolbarbutton", GUILayout.Height(ToolbarExtender.defaultHeight-2))) {
						var selector = new ParsedItemDataSelectorBase(item.Items);
						selector.SelectionConfirmed += OnSelectionConfirmed;
						selector.ShowInPopup(400, 400);
						GUIUtility.ExitGUI();
						return;
					}
				}
			}

			if (GUILayout.Button(EditorGUIUtility.IconContent("refresh", "|Refresh Scenes"), (GUIStyle)"toolbarbutton", GUILayout.Width(30),
					GUILayout.Height(ToolbarExtender.defaultHeight))) {
				Refresh();
				GUIUtility.ExitGUI();
			}
		}

		private static void OnSelectionConfirmed(IEnumerable<ParsedItemData> selectItems) {
			var menuItem = selectItems.FirstOrDefault();
			menuItem.Action?.Invoke(Event.current.shift);
		}

		private static IEnumerable<ParsedItemData> SearchCommonScenes() {
			static string GetName(string s) =>
				s.Replace($"Assets/Data/Scenes/", "")
					.Replace(".asset", "")
					.Replace(".unity", "");

			return EditorUtils.GetAssetPaths<UnityEngine.SceneManagement.Scene>()
				.Where(x => x.IsMatch($"Assets/Data/Scenes/*"))
				.OrderBy(GetName)
				.Select(x => new ParsedItemData(GetName(x), x, shiftPressed => OpenByPath(shiftPressed, x)));
		}

		private static IEnumerable<ParsedItemData> SearchScreenScenes() {
			static string GetName(string s) => s
					.Replace($"Assets/UI/Screens", "")
					.Replace(".unity", "");

			return EditorUtils.GetAssetPaths<UnityEngine.SceneManagement.Scene>()
				.Where(x => x.IsMatch($"Assets/UI/Screens/*"))
				.OrderBy(GetName)
				.Select(x => new ParsedItemData(GetName(x), x, shiftPressed => OpenByPath(shiftPressed, x)));
		}

		private static IEnumerable<ParsedItemData> SearchEnvScenes() {
			static string GetName(string s) =>
				s.Replace($"Assets/Environment/", "")
					.Replace(".unity", "");

			return EditorUtils.GetAssetPaths<UnityEngine.SceneManagement.Scene>()
				.Where(x => x.IsMatch($"Assets/Environment/*"))
				.OrderBy(GetName)
				.Select(x => new ParsedItemData(GetName(x), x, shiftPressed => OpenByPath(shiftPressed, x)));
		}
		
		private static string GetGroup(string s) {
			var separator = s.IndexOf("_", StringComparison.Ordinal);
			return (separator > 0) ? s[..separator] : "<empty>";
		}
	}

}