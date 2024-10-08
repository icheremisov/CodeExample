using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Core;
using XLib.Configs.Sheets;
using XLib.Configs.Sheets.Core;
using XLib.Core.Utils;
using XLib.Unity.Utils;

namespace XLib.Configs.Platforms {

	public static class SheetsImport {
		public static void Import(string spreadsheetId, params ISheetData[] data) {
			using var progress = EditorUtils.DisplayProgress("Import Sheets", true);
			ISheetData currentSheetData = null;
			try {
				progress.Progress("Connecting to service", 0.1f);
				var service = GoogleDocsUtils.GetSpreadsheets();

				progress.Progress("Loading spreadsheet", 0.2f);
				var sheets = GetSheets(service, spreadsheetId, data).ToArray();

				for (var i = 0; i < sheets.Length; i++) {
					(var sheet, var sheetData) = sheets[i];

					if (!CheckExportedDataBranch(sheet, sheetData)) continue;
					currentSheetData = sheetData;
					progress.SetHeader($"Importing sheet {sheetData.Title}");
					progress.Progress($"Importing sheet {sheetData.Title}", 1f, i, sheets.Length);
					ImportSheet(sheet, service.GetValue(sheet, spreadsheetId,
						sheetData.Direction == DirectionType.Horizontal
							? SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.COLUMNS
							: SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.ROWS), sheetData);
				}
			}
			catch (TaskCanceledException canceledException) {
				progress.Log($"Import canceled from {spreadsheetId}");
			}
			catch (GoogleApiException apiException) {
				Debug.LogError($"Ошибка API Google: {apiException.HttpStatusCode}. Проверьте таблицу и повторите\n{apiException}");
				Debug.LogException(apiException);
				ShowError(apiException, currentSheetData);
			}
			catch (Exception e) {
				Debug.LogException(e);
				ShowError(e, currentSheetData);
			}
			finally {
				AssetDatabase.SaveAssets();
				progress.Log($"Import complete from {spreadsheetId}");
			}
		}

		private static void ShowError(Exception ex, ISheetData sheetData) {
			throw new Exception($"Importing {sheetData?.Title ?? "???"}: {ex.Message}", ex);
		}

		private static bool CheckExportedDataBranch(Sheet sheet, ISheetData sheetData) {
			if (!sheetData.CheckBranch) return true;
			var currBranchInfo = GitInfoUtils.GetGitBranch();
			ConfigExportInfo exportFromBranchJson = null;

			var data = sheet.Data;
			if (data.IsNullOrEmpty()) return ShowDialog();

			var rawData = data[0].RowData;
			if (rawData.IsNullOrEmpty()) return ShowDialog();

			var values = rawData[0].Values;
			if (values.IsNullOrEmpty() || values[0] == null) return ShowDialog();

			var branchExportNote = values[0].Note;

			if (branchExportNote.IsNullOrEmpty()) return ShowDialog();

			exportFromBranchJson = JsonConvert.DeserializeObject<ConfigExportInfo>(branchExportNote);
			if (exportFromBranchJson == null) return ShowDialog();

			return exportFromBranchJson.Branch == currBranchInfo || ShowDialog();

			bool ShowDialog() {
				return EditorUtility.DisplayDialog("WARNING",
					$"Sheet with name:{sheetData.Title}\n was exported from branch:{(exportFromBranchJson != null ? exportFromBranchJson.Branch : "No info")}\n and you try to import it while working in branch: {currBranchInfo}\n Are you sure?.",
					"Sure", "Skip this sheet");
			}
		}

		public static IEnumerable<(Sheet, ISheetData)> GetSheets(SheetsService service, string spreadsheetId, ISheetData[] data) {
			var spreadsheet = service.GetSpreadsheet(spreadsheetId, new Repeatable<string>(data.Select(x => x.Title)), true);
			var sheetsByTitle = spreadsheet.GetSheetsByTitle();

			foreach (var sheetData in data) {
				if (sheetsByTitle.TryGetValue(sheetData.Title, out var sheet))
					yield return (sheet, sheetData);
				else {
					var message = $"Document \"{spreadsheet.Properties.Title}\" does not have a sheet named \"{sheetData.Title}\"";
					EditorUtility.DisplayDialog($"Import {sheetData.Title}", message, "Ok");
					Debug.LogWarning(message);
				}
			}
		}

		private static void ImportSheet(Sheet sheet, ValueRange sheetValue, ISheetData sheetData) {
			var title = sheet.Properties.Title;

			var headerRange = GetHeaderRange(sheet).AtDimension(sheetValue.MajorDimension);
			if (headerRange == null) throw new Exception($"No header in sheet {title}");

			var headerCount = headerRange.EndRowIndex.Value;
			var maxCount = sheetValue.Values.Max(list => list.Count);
			foreach (var list in sheetValue.Values) {
				while (list.Count < maxCount) list.Add(null);
			}

			var values = new List<SheetRowValues>();
			for (var columnIndex = 0; columnIndex < sheetValue.Values.Count; columnIndex++) {
				var columnValues = sheetValue.Values[columnIndex];
				if (columnValues.Count <= headerCount) {
					Debug.LogWarning($"Skipping column {GoogleDocsUtils.GetColumnName(columnIndex)} in sheet {title} - no values");
					continue;
				}

				var property = GetPropertyFromColumnHeader(sheet, sheetValue, columnIndex, headerCount);
				if (property == null) throw new Exception($"Can't get property from column {GoogleDocsUtils.GetColumnName(columnIndex)} in sheet {title}");

				if (property.IsExtra) {
					Debug.Log($"Skipping extra column {property.Name} (column {GoogleDocsUtils.GetColumnName(columnIndex)}) in sheet {title}");
					continue;
				}

				for (var i = headerCount; i < maxCount; i++) {
					var rowIndex = i - headerCount;

					SheetRowValues rowValues;
					if (rowIndex < values.Count)
						rowValues = values[rowIndex];
					else
						values.Add(rowValues = new SheetRowValues());
					var value = (string)columnValues[Math.Min(i, columnValues.Count - 1)];
					if (string.IsNullOrEmpty(value)) {
						var mergeRange = sheet.Merges?
							.FirstOrDefault(range => range.Contains(i, columnIndex, sheetValue.MajorDimension))
							?.AtDimension(sheetValue.MajorDimension);
						if (mergeRange != null && mergeRange.StartColumnIndex.HasValue && mergeRange.StartRowIndex.HasValue) {
							value = (string)sheetValue.Values[mergeRange.StartColumnIndex.Value][mergeRange.StartRowIndex.Value];
						}
					}

					rowValues.SetValue(property, value);
				}
			}

			sheetData.Import(values, new SheetContext(sheet, false));
		}

		public static GridRange GetHeaderRange(Sheet sheet) =>
			sheet.ProtectedRanges
				?.FirstOrDefault(range => range.Description == SheetColumnData.HeaderRangeDescription)
				?.Range;

		public static GridRange AtDimension(this GridRange range, string dimension) {
			if (range == null) return null;
			if (dimension == "COLUMNS") return range;
			if (dimension == "ROWS")
				return new GridRange() {
					SheetId = range.SheetId,
					StartColumnIndex = range.StartRowIndex,
					StartRowIndex = range.StartColumnIndex,
					EndColumnIndex = range.EndRowIndex,
					EndRowIndex = range.EndColumnIndex
				};
			throw new ArgumentException($"No support dimension {dimension}", nameof(dimension));
		}

		public static bool Contains(this GridRange range, int row, int column, string dimension) {
			if (dimension == "ROWS") (column, row) = (row, column);
			return row >= range.StartRowIndex && row <= (range.EndRowIndex ?? int.MaxValue)
				&& column >= range.StartColumnIndex && column <= (range.EndColumnIndex ?? int.MaxValue);
		}

		// public static string GetValue(ValueRange sheetValue, GoogleDocsUtils.RangeType rangeType, int columnIndex, int rowIndex) {
		// 	if (rangeType != GoogleDocsUtils.RangeType.Normal) (columnIndex, rowIndex) = (rowIndex, columnIndex);
		//
		// 	var columnValues = sheetValue.Values[columnIndex];
		// 	if (rowIndex >= columnValues.Count) return null;
		//
		// 	return (string)columnValues[rowIndex];
		// }

		public static string GetHeaderValue(IEnumerable<GridRange> mergeHeaderRanges, ValueRange sheetValue, int columnIndex, int rowIndex) {
			string value = null;
			var columnValues = sheetValue.Values[columnIndex];
			if (rowIndex < columnValues.Count) {
				value = (string)columnValues[rowIndex];
			}

			if (!string.IsNullOrWhiteSpace(value)) return value;

			var merge = mergeHeaderRanges?.FirstOrDefault(range =>
				columnIndex >= range.StartColumnIndex &&
				columnIndex < range.EndColumnIndex &&
				rowIndex >= range.StartRowIndex &&
				rowIndex < range.EndRowIndex);
			if (merge != null) {
				value = (string)sheetValue.Values[merge.StartColumnIndex.Value][merge.StartRowIndex.Value];
			}

			return string.IsNullOrWhiteSpace(value) ? null : value;
		}

		private static SheetRowProperty GetPropertyFromColumnHeader(Sheet sheet, ValueRange sheetValue, int columnIndex, int headerCount) {
			SheetRowProperty top = null;
			SheetRowProperty current = null;

			var i = 0;
			while (i < headerCount) {
				var header = GetHeaderValue(sheet.Merges?.Select(range => range.AtDimension(sheetValue.MajorDimension)), sheetValue, columnIndex, i);
				if (header == null) break;

				var property = SheetRowProperty.Parse(header);
				if (property.IsExtra) return property;

				if (top == null) top = property;
				if (current != null) current.Inner = property;

				i++;
				current = property;
			}

			return top;
		}

		private static void SetValue(this SheetRowValues values, SheetRowProperty property, string value) {
			while (property != null) {
				var name = property.Name;
				if (!values.TryGetValue(name, out var propertyValue)) {
					values.Add(name, propertyValue = new SheetRowPropertyValue(name));

					if (property.Inner == null)
						propertyValue.Values = new List<string>();
					else
						propertyValue.Items = new List<SheetRowValues>();
				}

				if (property.Inner == null) {
					if (!string.IsNullOrWhiteSpace(value)) propertyValue.Values.Add(value);

					break;
				}

				var index = property.Index ?? 0;
				var items = propertyValue.Items;

				SheetRowValues itemValues;
				if (index < items.Count) {
					itemValues = items[index];
				}
				else {
					if (string.IsNullOrWhiteSpace(value)) break;

					//заполняем массивы полностью, учитывая дырки в нумерации
					//дырки будут вырезаны позже или опционально сохранены
					while (index > items.Count) items.Add(null);

					items.Add(itemValues = new SheetRowValues());
				}

				values = itemValues;
				property = property.Inner;
			}
		}

		private class SheetRowProperty {
			public readonly int? Index;
			public readonly string Name;
			public readonly bool IsExtra;

			public SheetRowProperty Inner;

			private SheetRowProperty(string name, int? index, bool isExtra) {
				Name = name;
				Index = index;
				IsExtra = isExtra;
			}

			public static SheetRowProperty Parse(string header) {
				SheetColumnData.ParseHeader(header, out var name, out _, out var index, out var isExtra);
				return new SheetRowProperty(name, index, isExtra);
			}
		}
	}

}