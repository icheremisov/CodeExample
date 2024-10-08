#if UNITY_EDITOR
using System;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Sheets.Contracts;
using XLib.Core.Reflection;
using XLib.Core.Utils;
using XLib.Unity.Utils;
using Color = UnityEngine.Color;

namespace XLib.Configs.Sheets.Core {

	public partial class SheetFormatSettings {
		[ShowIf(nameof(WithEnumColors))]
		public Gradient EnumColors = new() {
			mode = GradientMode.Blend,
			colorKeys =
				Enumerable.Range(0, 8)
					.Select(i => new GradientColorKey(Color.HSVToRGB(i / 7f, 0.15f, 1f), i / 7f))
					.ToArray()
		};
	}

	public partial class SheetsSettings {
		[Space, InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.CompletelyHidden), ListDrawerSettings(HideAddButton = true,
			 OnTitleBarGUI = nameof(OnTitleBarGUI),
			 ShowItemCount = true, CustomRemoveElementFunction = nameof(OnRemoveElement))]
		[SerializeField] protected SheetData[] _sheets = { };

		public SheetData[] Sheets => _sheets;

		[ShowInInspector, PropertyOrder(-2)]
		public SpreadsheetId SheetId {
			get => new(EditorPrefs.GetString(nameof(SpreadsheetId)));
			set => EditorPrefs.SetString(nameof(SpreadsheetId), value.ToString());
		}

		[MenuItem("Tools/Database/Sheet Settings")]
		public static void Select() => Selection.activeObject = EditorUtils.LoadFirstAsset<SheetsSettings>();

		[ContextMenu("Clear SpreadsheetId")]
		private void ClearSpreadsheet() => SheetId = new SpreadsheetId(string.Empty);

		public void OnRemoveElement(SheetData data) => Remove(data);

		public void OnTitleBarGUI() {
			if (!SirenixEditorGUI.ToolbarButton(EditorIcons.Plus)) return;

			var menu = new GenericMenu();
			foreach (var type in TypeCache<SheetData>.CachedTypes.Where(type => type.HasAttribute<AllowMultipleSheetsAttribute>() || _sheets.All(data => data.GetType() != type))) {
				var content = new GUIContent(ObjectNames.NicifyVariableName(type.Name));
				menu.AddItem(content, false, userData => Add((Type)userData), type);
			}

			if (menu.GetItemCount() == 0) menu.AddItem(new GUIContent("<NONE>"), false, _ => { }, null);

			menu.ShowAsContext();
		}

		public SheetData Add(Type type) {
			var sheet = (SheetData)CreateInstance(type);
			Undo.RegisterCreatedObjectUndo(sheet, "Create sheet");

			sheet.Setup(this);

			var path = AssetDatabase.GetAssetPath(this);
			AssetDatabase.AddObjectToAsset(sheet, path);
			AssetDatabase.Refresh();

			UpdateSheets();
			AssetDatabase.SaveAssets();
			return sheet;
		}

		private void Remove(SheetData sheet) {
			if (!_sheets.Contains(sheet)) return;

			Undo.RecordObject(this, "Remove sheets");
			Undo.DestroyObjectImmediate(sheet);

			UpdateSheets();
			AssetDatabase.SaveAssets();
		}

		private void UpdateSheets() {
			var path = AssetDatabase.GetAssetPath(this);
			var assets = AssetDatabase.LoadAllAssetsAtPath(path).OfType<SheetData>().ToArray();
			if (assets.Length == _sheets.Length && !_sheets.Contains(null)) return;

			Undo.RecordObject(this, "Update sheets");
			_sheets = _sheets.Where(x => x != null).Concat(assets.Except(_sheets)).ToArray();
		}
	}

}
#endif