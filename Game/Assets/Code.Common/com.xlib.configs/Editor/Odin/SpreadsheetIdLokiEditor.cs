using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Google;
using Google.Apis.Sheets.v4.Data;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Platforms;
using XLib.Unity.Extensions;
using XLib.Unity.Utils;

namespace XLib.Configs.Odin {

	[UsedImplicitly]
	internal class SpreadsheetIdLokiEditor : LokiEditor<SpreadsheetId> {
		[UsedImplicitly]
		private class ValueDrawer : LokiValueDrawer {
			private InspectorProperty _id;
			private string _key;
			private string _title;

			protected override void Initialize() {
				base.Initialize();
				_id = Property.Children["_id"];
			}

			private void ClearTitleCache() {
				_key = null;
				_title = string.Empty;
			}

			private string GetTitle() {
				var spreadsheetId = _id.ValueEntry.WeakSmartValue as string;
				if (_key != spreadsheetId) {
					if (!string.IsNullOrEmpty(spreadsheetId)) {
						_key = spreadsheetId;
						LoadingTitle(spreadsheetId).Forget();
					}
					else {
						_key = null;
						_title = "<None>";
					}
				}

				return _title;
			}

			private async UniTask LoadingTitle(string spreadsheetId) {
				try {
					_title = "loading...";
					var spreadsheet = await GoogleDocsUtils.GetSpreadsheets().Spreadsheets.Get(spreadsheetId).ExecuteAsync();
					_title = spreadsheet.Properties.Title;
				}
				catch (GoogleApiException ex) {
					EditorUtility.DisplayDialog(ex.HttpStatusCode.ToString(), ex.Message, "Ok");
				}
			}

			protected override void DrawPropertyLayout(GUIContent label) {
				SirenixEditorGUI.Title(GetTitle(), string.Empty, TextAlignment.Left, true);
				SirenixEditorGUI.BeginHorizontalPropertyLayout(GUIContent.none);
				GUIHelper.PushGUIEnabled(false);
				_id.Draw(GUIContent.none);
				GUIHelper.PopGUIEnabled();

				GUIHelper.PushGUIEnabled(!string.IsNullOrEmpty(_key));
				if (SirenixEditorGUI.IconButton(EditorIcons.Link, SirenixGUIStyles.Button)) {
					GoogleDocsUtils.Open(_id.ValueEntry.WeakSmartValue as string);
				}

				GUIHelper.PopGUIEnabled();

				SirenixEditorGUI.EndHorizontalPropertyLayout();
				SirenixEditorGUI.BeginIndentedHorizontal(GUILayout.Height(20));
				SirenixEditorGUI.BeginHorizontalPropertyLayout(GUIContent.none);

				if (SirenixEditorGUI.SDFIconButton("New Document", 20, SdfIconType.FileEarmarkExcel)) {
					CreateNewDoc().Forget();
				}

				if (SirenixEditorGUI.SDFIconButton("Open", 20, SdfIconType.Folder2Open)) {
					OpenFromURL().Forget();
				}

				SirenixEditorGUI.EndHorizontalPropertyLayout();
				SirenixEditorGUI.EndIndentedHorizontal();
			}

			private async UniTask CreateNewDoc() {
				await UniTask.Yield();

				ClearTitleCache();
				var rootName = Property.SerializationRoot.ParentType.Name;
				var title = EditorUtils.ShowInputDialog("New spreadsheets", "Specify a document name", $"{rootName} {DateTime.Now}");
				if (title == null) return;

				var spreadsheet = await GoogleDocsUtils.GetSpreadsheets()
					.Spreadsheets.Create(new Spreadsheet() { Properties = new SpreadsheetProperties() { Title = title } })
					.ExecuteAsync();

				SetSpreadsheet(spreadsheet.SpreadsheetId);
				GoogleDocsUtils.Open(spreadsheet.SpreadsheetId);
			}

			private async UniTask OpenFromURL() {
				await UniTask.Yield();

				var url = EditorUtils.ShowInputDialog("Open Document", "Enter ulr to google spreadsheets", "");
				if (url == null) return;

				ClearTitleCache();
				if (url.Length > 44) {
					var match = Regex.Match(url, @"\/d\/([\w-+\!\$\&]+)\/?");
					if (match.Success) {
						SetSpreadsheet(match.Groups[1].Value);
						return;
					}
				}

				if (url.Length == 44) {
					SetSpreadsheet(url);
					return;
				}

				EditorUtility.DisplayDialog("Open Document", $"Unable to find document \"{url}\"", "Ok");
			}

			private void SetSpreadsheet(string sheetId) => _id.ValueEntry.WeakSmartValue = sheetId;
		}
	}

}