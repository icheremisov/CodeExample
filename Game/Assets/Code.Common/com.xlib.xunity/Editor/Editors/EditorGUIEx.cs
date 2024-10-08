using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RectEx;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.Unity.Editors {

	public static class EditorGuiEx {
		public static string DrawFolderSelect(string text, string value, out bool hasChanges) {
			EditorGUILayout.BeginHorizontal();
			var newValue = EditorGUILayout.TextField(text, value);
			if (GUILayout.Button("...", GUILayout.Width(50))) newValue = EditorUtility.OpenFolderPanel(text, newValue, "Import");

			EditorGUILayout.EndHorizontal();

			hasChanges = newValue != value;

			return newValue;
		}

		public static string DrawFileSelect(string text, string value, string ext, out bool hasChanges) {
			EditorGUILayout.BeginHorizontal();
			var newValue = EditorGUILayout.TextField(text, value);
			if (GUILayout.Button("...", GUILayout.Width(50))) {
				newValue = EditorUtility.OpenFilePanel(text, newValue, ext);
				if (newValue.IsNullOrEmpty()) newValue = value;
			}

			EditorGUILayout.EndHorizontal();

			hasChanges = newValue != value;

			return newValue;
		}

		public static void SavePrefs(string name, object data) {
			if (data == null) {
				EditorPrefs.DeleteKey(name);
				return;
			}

			var str = JsonConvert.SerializeObject(data);
			EditorPrefs.SetString(name, str);
		}

		public static T LoadPrefs<T>(string name) {
			var data = EditorPrefs.GetString(name, string.Empty);
			if (data.IsNullOrEmpty()) return default;

			try {
				return JsonConvert.DeserializeObject<T>(data);
			}
			catch (Exception e) {
				Debug.LogError(e);
				return default;
			}
		}
		
		public static void Separator() {
			GUI.Box(EditorGUILayout.GetControlRect(false, 3f), GUIContent.none);
		}

		public static void DrawBadge(GUIContent content, GUIStyle style, Color color, Color background, float radius = 0.5f, float border = 1f) {
			var curRect = GUILayoutUtility.GetRect(content, style, GUILayout.ExpandWidth(false));
			GUI.DrawTexture(curRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, color, 0f, curRect.height * radius);
			GUI.DrawTexture(new Rect(curRect.position + Vector2.one * border, curRect.size - Vector2.one * (border * 2f)), Texture2D.whiteTexture, ScaleMode.StretchToFill, false,
				0f, background, 0f, (curRect.height - border * 2) * radius);
			EditorGUI.LabelField(curRect, content, style);
		}

		public static GenericMenu TreeAssetMenu<T>(Action<string> select, string baseDirectory = null, Func<string, bool> isSelect = null) {
			var menu = new GenericMenu();
			if (baseDirectory == null) baseDirectory = "Assets";
			foreach (var path in AssetDatabase.FindAssets($"t:{TypeOf<T>.Name}").Select(AssetDatabase.GUIDToAssetPath)) {
				var file = Path.GetRelativePath(baseDirectory, path).ReplaceAll('\\', '/');
				file = file[..^Path.GetExtension(file).Length];
				menu.AddItem(new GUIContent(file), isSelect?.Invoke(path) ?? false, (o) => select(o as string), path);
			}
			return menu;
		}

		public static Rect Cut(this Rect rect, float leftPercent, float rightPercent, float heightPixel = 0f, float gap = float.NaN)
		{
			if (float.IsNaN(gap))
				gap = (Math.Abs(rightPercent - 1f) < 0.001f) ? 0f : 5f;
			return new Rect(rect.x + rect.width * leftPercent, rect.y, rect.width * (rightPercent - leftPercent) - gap,
				heightPixel == 0f ? rect.height : heightPixel);
		}
		
		public static bool SelectButtonList(ref Type selectedType, Type[] typesToDisplay) {
			var rect = GUILayoutUtility.GetRect(0, 25);
			for (var i = 0; i < typesToDisplay.Length; ++i) {
				var name = typesToDisplay[i].Name;
				var btnRect = rect.SplitHorizontal(i, typesToDisplay.Length);
				if (SelectButton(btnRect, name, typesToDisplay[i] == selectedType)) {
					selectedType = typesToDisplay[i];
					return true;
				}
			}
			return false;
		}
		public static bool SelectButtonList(ref int selectedIndex, string[] namesToDisplay) {
			var rect = GUILayoutUtility.GetRect(0, 25);
			for (var i = 0; i < namesToDisplay.Length; ++i) {
				var name = namesToDisplay[i];
				var btnRect = rect.SplitHorizontal(i, namesToDisplay.Length);
				if (SelectButton(btnRect, name, i == selectedIndex)) {
					selectedIndex = i;
					return true;
				}
			}
			return false;
		}

		public static bool SelectButton(Rect rect, string name, bool selected) {
			if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) return true;

			if (Event.current.type == EventType.Repaint) {
				var style = new GUIStyle(EditorStyles.miniButtonMid) { stretchHeight = true, fixedHeight = rect.height };
				style.Draw(rect, GUIHelper.TempContent(name), false, false, selected, false);
			}
			return false;
		}
	}

}