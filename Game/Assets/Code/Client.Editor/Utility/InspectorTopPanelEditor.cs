using System.IO;
using System.Linq;
using DependenciesHunter;
using Newtonsoft.Json.Linq;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Core;
using XLib.Unity.Utils;

namespace Client.Utility {

	[InitializeOnLoad]
	internal static class InspectorTopPanelEditor {
		private static GUIStyle _btStyle;

		private static GUIContent _pasteIcon;
		private static GUIContent _copyIcon;
		private static GUIContent _duplicateIcon;
		private static GUIContent _pingIcon;
		private static GUIContent _searchAssetsIcon;
		private static GUIContent _genSettingsIcon;

		static InspectorTopPanelEditor() {
			UnityEditor.Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
		}

		private static void SetupStyles() {
			if (_duplicateIcon == null) {
				_duplicateIcon ??= new GUIContent(EditorGUIUtility.IconContent("d_Toolbar Plus"));
				_duplicateIcon.tooltip = "Duplicate";
			}

			if (_pingIcon == null) {
				_pingIcon ??= new GUIContent(EditorGUIUtility.IconContent("d_Grid.Default"));
				_pingIcon.tooltip = "Select in project view";
			}

			if (_pasteIcon == null) {
				_pasteIcon ??= new GUIContent(EditorGUIUtility.IconContent("d_Profiler.UIDetails"));
				_pasteIcon.tooltip = "Paste";
			}

			if (_searchAssetsIcon == null) {
				_searchAssetsIcon ??= new GUIContent(EditorGUIUtility.IconContent("d_SearchOverlay"));
				_searchAssetsIcon.tooltip = "Search asset references";
			}

			if (_copyIcon == null) {
				_copyIcon ??= new GUIContent(EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow"));
				_copyIcon.tooltip = "Copy";
			}

			if (_genSettingsIcon == null) {
				_genSettingsIcon ??= new GUIContent(EditorGUIUtility.IconContent("d_Settings Icon"));
				_genSettingsIcon.tooltip = "Settings";
			}

			_btStyle = SetupStyle(Color.yellow, new Color(0.42f, 0.38f, 0f));
			
			static GUIStyle SetupStyle(Color textColor, Color bgColor) {
				return new GUIStyle(GUI.skin.button) {
					normal = { textColor = textColor, background = GfxUtils.MakeTex(1, 1, bgColor) },
					hover = { textColor = textColor, background = GfxUtils.MakeTex(1, 1, bgColor) },
					active = { textColor = textColor, background = GfxUtils.MakeTex(1, 1, bgColor) },
					focused = { textColor = textColor, background = GfxUtils.MakeTex(1, 1, bgColor) },
					margin = new RectOffset(2, 2, 0, 10),
					padding = new RectOffset(2, 2, 2, 2)
				};
			}
		}

		private static void OnPostHeaderGUI(UnityEditor.Editor editor) {
			SetupStyles();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(_pingIcon, _btStyle, GUILayout.Height(20))) {
				GUIHelper.PingObject(editor.target);
			}

			if (GUILayout.Button(_searchAssetsIcon, _btStyle, GUILayout.Height(20))) {
				SelectedAssetsReferencesWindow.FindReferences();
			}

			if (GUILayout.Button(_duplicateIcon, _btStyle, GUILayout.Height(20))) {
				foreach (var target in editor.targets) Duplicate(AssetDatabase.GetAssetPath(target));
			}
			
			if (editor.target is GameItemBase gi) {
				if (GUILayout.Button(_copyIcon, _btStyle, GUILayout.Height(20))) Copy(gi);

				if (gi.CanPaste) {
					if (GUILayout.Button(_pasteIcon, _btStyle, GUILayout.Height(20))) Paste(gi);
				}
			}

			if (GUILayout.Button(_genSettingsIcon, _btStyle, GUILayout.Height(20), GUILayout.Width(20))) {
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Config Exporter"), false, () => SelectFirstAsset("t:SheetsSettings"));
				menu.AddItem(new GUIContent("Localize Exporter"), false, () => SelectFirstAsset("t:Localize"));

				menu.ShowAsContext();
			}

			GUILayout.FlexibleSpace();

			
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		private static void SelectFirstAsset(string search) {
			Selection.activeObject = AssetDatabase.FindAssets(search).Select(guid => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid))).FirstOrDefault();
		}

		private static void Copy(GameItemBase gameItem) => Clipboard.Copy(gameItem);

		private static void Paste(GameItemBase gameItem) {
			var jo = JObject.Parse(EditorJsonUtility.ToJson(Clipboard.Paste<GameItemBase>()));
			var mo = (JObject)jo["MonoBehaviour"];
			if (mo == null) return;
			mo.Property("_id")?.Remove();
			mo.Property("m_Name")?.Remove();
			mo.Property("Name")?.Remove();
			EditorJsonUtility.FromJsonOverwrite(jo.ToString(), gameItem);
		}

		private static void Duplicate(string path) {
			var ext = Path.GetExtension(path);
			var newPath = path.Replace(ext, $" copy{ext}");
			var idx = 0;
			while (File.Exists(newPath)) {
				newPath = path.Replace(ext, $" copy {++idx}{ext}");
			}

			if (AssetDatabase.CopyAsset(path, newPath)) Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameItemBase>(newPath);
		}

		private static void DrawWipWarning() {
			GUILayout.BeginHorizontal();
			var icon = EditorGUIUtility.IconContent("d_console.warnicon.sml");
			GUILayout.Label(icon.image);
			var labelStyle = new GUIStyle(GUI.skin.label) {
				alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, normal = { textColor = Color.yellow },
			};
			GUILayout.Label(new GUIContent("Asset is in WIP state"), labelStyle, GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
	}

}