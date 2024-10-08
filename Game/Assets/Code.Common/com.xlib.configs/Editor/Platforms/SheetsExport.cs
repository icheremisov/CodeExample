#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Google;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Sheets;
using XLib.Configs.Sheets.Contracts;
using XLib.Configs.Sheets.Core;
using XLib.Configs.Sheets.Types;
using XLib.Unity.Utils;
using Color = UnityEngine.Color;
using Request = Google.Apis.Sheets.v4.Data.Request;
using ValueRenderOptionEnum = Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum;

namespace XLib.Configs.Platforms {

	public class NoHeaderException : SystemException {
		public NoHeaderException(string message) : base(message) { }
	}

	public static class SheetsExport {
		private const string FileName = "Asset";

		public enum SheetExportMode {
			
			/// <summary>
			/// add new sheets and replace contents of existing 
			/// </summary>
			Merge,
			
			/// <summary>
			/// remove all sheets from document and set new sheets
			/// </summary>
			Replace,
		}

		public static void Export(string spreadsheetId, bool skipClientTypes, SheetExportMode mode, params ISheetData[] data) {
			using var progress = EditorUtils.DisplayProgress("Export Sheets", false);

			var error = false;
			try {
				progress.Progress("Connecting to service", 0.1f);
				var service = GoogleDocsUtils.GetSpreadsheets();

				progress.Progress("Loading spreadsheet", 0.2f);

				if (mode == SheetExportMode.Replace) RemoveAllSheets(service, spreadsheetId);
				var sheets = GetOrAddSheets(service, spreadsheetId, data).ToArray();

				var values = new List<ValueRange>();
				var preprocess = new List<Request>();
				var postprocess = new List<Request>();

				for (var i = 0; i < sheets.Length; i++) {
					(var sheet, var sheetData) = sheets[i];
					Debug.Log($"Exporting sheet {sheetData.Title}");

					progress.Progress($"Reading sheet {sheetData.Title}", 0.6f, i, sheets.Length);
					var currentValue = service.GetValue(sheet, spreadsheetId, renderOption: ValueRenderOptionEnum.FORMULA,
						majorDimension: sheetData.Direction == DirectionType.Horizontal
							? SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.COLUMNS
							: SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.ROWS);

					progress.Progress($"Exporting sheet {sheetData.Title}", 0.7f, i, sheets.Length);
					ExportSheet(sheet, currentValue, sheetData, skipClientTypes, out var sheetValue, out var sheetRequests);

					if (sheetValue.Values.Count > 0) values.Add(sheetValue);
					preprocess.AddRange(Clear(sheet));
					postprocess.AddRange(sheetRequests);
				}

				if (preprocess.Count > 0) {
					progress.Progress("Preprocess spreadsheet", 0.8f);
					service.BatchUpdate(preprocess, spreadsheetId);
				}

				if (values.Count > 0) {
					progress.Progress("Updating spreadsheet values", 0.9f);
					service.BatchUpdateValues(values, spreadsheetId, GoogleDocsUtils.ValueInputType.USER_ENTERED);
				}

				if (postprocess.Count > 0) {
					progress.Progress("Postprocess spreadsheet", 1f);
					service.BatchUpdate(postprocess, spreadsheetId);
				}
			}
			catch (GoogleApiException apiException) {
				EditorUtility.DisplayDialog($"Google Sheet API Error: {apiException.HttpStatusCode}", "Check the table and repeat:\n" + apiException.Message, "Ok");
				Debug.LogError($"Google Sheet API Error: {apiException.HttpStatusCode}. Check the table and repeat\n{apiException}");
				throw new OperationCanceledException("Google Sheet API Error", apiException);
			}
			catch (Exception e) {
				Debug.LogException(e);
				throw new OperationCanceledException("Export error", e);
			}
			finally {
				progress.Log($"Export complete to {spreadsheetId}");
			}
		}

		private static void RemoveAllSheets(SheetsService service, string spreadsheetId) {
			var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
			var requests = new List<Request>();
			for (var i = 1; i < spreadsheet.Sheets.Count; i++) requests.Add(spreadsheet.Sheets[i].DeleteSheet());

			if (requests.Count > 0) service.BatchUpdate(requests, spreadsheetId);
		}

		private static IEnumerable<(Sheet, ISheetData)> GetOrAddSheets(SheetsService service, string spreadsheetId, ISheetData[] data) {
			var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
			var sheetsByTitle = spreadsheet.GetSheetsByTitle();

			var requests = new List<Request>();
			foreach (var sheetData in data) {
				if (!sheetsByTitle.ContainsKey(sheetData.Title)) requests.Add(GoogleDocsUtils.AddSheet(sheetData.Title));
			}

			if (requests.Count > 0) {
				spreadsheet = service.BatchUpdate(requests, spreadsheetId);
				sheetsByTitle = spreadsheet.GetSheetsByTitle();
			}

			foreach (var sheetData in data) {
				if (sheetsByTitle.TryGetValue(sheetData.Title, out var sheet)) yield return (sheet, sheetData);
			}
		}

		private static IEnumerable<Request> Clear(Sheet sheet) {
			var range = GoogleDocsUtils.GetRange(sheet);

			var requests = new List<Request> {
				GoogleDocsUtils.Unmerge(range),
				GoogleDocsUtils.ClearFormat(range),
				GoogleDocsUtils.ClearValidation(range),
				GoogleDocsUtils.ClearValues(range),
			};

			if (sheet.ProtectedRanges != null) {
				requests.AddRange(sheet.ProtectedRanges.Select(protectedRange =>
					GoogleDocsUtils.DeleteProtectedRange(protectedRange.ProtectedRangeId.Value)));
			}

			requests.AddRange(GoogleDocsUtils.ClearFormatRules(sheet));

			return requests;
		}

		private static void ExportSheet(Sheet sheet, ValueRange currentValue, ISheetData sheetData, bool skipClientTypes, out ValueRange sheetValue, out IList<Request> requests) {
			var exportedValues = sheetData.Export(new SheetContext(sheet, skipClientTypes)).ToArray();

			var hasInlineProperty = exportedValues.Any(columnValues => !columnValues.Column.RootInlineProperty.IsNullOrEmpty());
			if (currentValue.Values != null && !hasInlineProperty) {
				try {
					var readValues = Read(sheet, currentValue, sheetData);
					exportedValues = Combine(sheet, exportedValues, readValues.ToArray(), sheetData.Settings.SortByColumnIndex).ToArray();
				}
				catch (NoHeaderException ex) {
					if (!EditorUtility.DisplayDialog($"Loading a table '{sheet.Properties.Title}'",
							$"The table header could not be recognized. If you continue, the contents of page \"{sheet.Properties.Title}\" will be overwritten",
							"Continue", "Cancel"))
						throw;
				}
			}

			var rowCount = 0;
			var headerCount = 0;
			var values = new List<IList<object>>();

			var fixedColumns = 0;
			var protectedColumns = new Dictionary<int, string>();
			var mergedColumns = new Dictionary<string, MergedRangeData>();
			var boolColumns = new List<int>();
			// var enumColumns = new List<ValueRangeData>();
			var columnsWithValidation = new Dictionary<int, ValueEnumRangeData>();
			var visibleColumns = new Dictionary<int, bool>();

			var formatColumns = new Dictionary<int, SheetRowProperty>();
			var sizeColumns = new Dictionary<int, SheetRowProperty>();
			var headers = new Dictionary<(int rowIndex, int colIndex), SheetRowProperty>();

			var isTranspose = sheetData.Direction == DirectionType.Vertical;
			var rangeWithOffset = new List<MergedRangeData>();

			var columnIndex = 0;
			foreach (var columnValues in exportedValues) {
				var list = new List<object>();
				var merged = new List<MergedRangeData>();

				var column = columnValues.Column;
				var property = column.Property;

				var s = property?.IsSkip(column);
				if (s.HasValue && s.Value) continue;

				if (property != null && (property.IsProtected || !property.CanWrite)) protectedColumns.Add(columnIndex, column.Header);

				if (property != null && property.Type == typeof(bool)) {
					boolColumns.Add(columnIndex);
				}

				if (property != null && property.HasFormat) {
					formatColumns.Add(columnIndex, property);
				}

				if (property != null) {
					sizeColumns.Add(columnIndex, property);
				}

				if (property != null && property.Priority <= -100) {
					fixedColumns = Mathf.Max(fixedColumns, columnIndex + 1);
				}

				var v = property?.IsVisible(column);
				if (v.HasValue) visibleColumns.Add(columnIndex, v.Value);

				var count = 0;

				while (column != null && !column.IsInline) {
					list.Add(column.Header);
					headers.TryAdd(new() { rowIndex = GetRowIndex(column), colIndex = columnIndex }, column.Property);

					count++;
					if (count > headerCount) {
						for (var i = 0; i < values.Count; i++) {
							values[i].Insert(headerCount, null);

							if (columnsWithValidation.TryGetValue(i, out var enumRangeData)) enumRangeData.InsertRow();
						}

						rowCount++;
						headerCount = count;
					}

					column = column.Parent;

					if (column != null && !column.IsInline) {
						if (!mergedColumns.TryGetValue(column.Path, out var data)) {
							data = new MergedRangeData(count, columnIndex);
							merged.Add(data);
							mergedColumns.Add(column.Path, data);
						}

						data.AddColumn(columnIndex);
					}
				}

				list.Reverse();
				foreach (var data in merged) {
					data.RowMin = (count - 1) - data.RowMin;
					data.RowMax = (count - 1) - data.RowMax;
				}

				// if (attribute != null && attribute.EnumType != null) 
				// enumColumns.Add(new EnumRangeData(count - 1, columnIndex, attribute.EnumType));

				for (var i = count; i < headerCount; i++) list.Insert(i, null);

				list.AddRange(columnValues);
				if (list.Count > rowCount) rowCount = list.Count;

				if (property != null && property.WithValidation && property.ValidationRule != null) {
					columnsWithValidation.Add(columnIndex,
						new ValueEnumRangeData(columnIndex, list.Count, property.ValidationRule));
				}

				if (property.IsMergeEqual && !sheetData.WithFilter) {
					object prevValue = null;
					var path = columnValues.Column.Path;
					for (var i = 0; i < columnValues.Count; ++i) {
						var columnValue = columnValues[i];

						var blockId = $"{path}#{columnValue}";
						if (!mergedColumns.TryGetValue(blockId, out var data)) {
							data = new MergedRangeData(i, columnIndex);
							merged.Add(data);
							mergedColumns.Add(blockId, data);
							rangeWithOffset.Add(data);
						}

						data.AddRow(i);
					}
				}

				columnIndex++;
				values.Add(list);
			}

			if (sheetData.WithFilter) {
				foreach (var t in values) t.Insert(headerCount, null);

				++headerCount;
				++rowCount;
			}

			foreach (var rangeData in rangeWithOffset) {
				rangeData.OffsetRow(headerCount);
			}

			var dimension = isTranspose ? "ROWS" : "COLUMNS";

			sheetValue = new ValueRange {
				Values = values, MajorDimension = dimension, Range = GoogleDocsUtils.GetRange(sheet, 0, 0, values.Count, rowCount).AtDimension(dimension).ToA1Notation(sheet),
			};

			var headerRange = GoogleDocsUtils.GetRange(sheet, startRowIndex: 0, endRowIndex: headerCount).AtDimension(sheetValue.MajorDimension);

			requests = new List<Request> {
				GoogleDocsUtils.AddProtectedRange(headerRange, SheetColumnData.HeaderRangeDescription),
				GoogleDocsUtils.UpdateBorders(headerRange, isTranspose ? GoogleDocsUtils.BorderType.InnerHorizontal : GoogleDocsUtils.BorderType.InnerVertical),
				GoogleDocsUtils.SetFormat(headerRange, sheetData.Settings.HeaderColor, Color.black, 0, true),
				GoogleDocsUtils.UpdateFrozenRows(sheet, isTranspose ? fixedColumns : headerCount, isTranspose ? headerCount : fixedColumns),
			};

			if (sheetData.WithFilter) {
				var range = GoogleDocsUtils.GetRange(sheet, 0, headerCount - 1, values.Count).AtDimension(sheetValue.MajorDimension);
				requests.Add(GoogleDocsUtils.SetBasicFilter(range));
			}
			else {
				requests.Add(GoogleDocsUtils.ClearBasicFilter(sheet.Properties.SheetId));
			}

			foreach ((var index, var name) in protectedColumns) {
				var range = GoogleDocsUtils.GetRange(sheet, index, headerCount, index + 1).AtDimension(sheetValue.MajorDimension);
				requests.Add(GoogleDocsUtils.AddProtectedRange(range, name));
				requests.Add(GoogleDocsUtils.SetFormat(range, sheetData.Settings.ProtectedColor, Color.black));
			}

			foreach (var index in boolColumns) {
				var range = GoogleDocsUtils.GetRange(sheet, index, headerCount, index + 1).AtDimension(sheetValue.MajorDimension);
				requests.Add(GoogleDocsUtils.SetBoolValidation(range));
			}

			foreach ((var index, var visible) in visibleColumns) {
				requests.Add(sheet.SetColumnsVisible(!visible, dimension, index, index + 1));
			}

			foreach ((var index, var property) in formatColumns) {
				var range = GoogleDocsUtils.GetRange(sheet, index, headerCount, index + 1).AtDimension(sheetValue.MajorDimension);
				requests.Add(GoogleDocsUtils.SetFormat(range, property.BackgroundColor, property.Color));
			}

			foreach (var data in mergedColumns.Values) {
				if (data.TotalCells <= 1) continue;
				var range = data.GetRange(sheet).AtDimension(sheetValue.MajorDimension);
				requests.Add(GoogleDocsUtils.Merge(range));
				requests.Add(GoogleDocsUtils.UpdateBorders(range, isTranspose ? GoogleDocsUtils.BorderType.Right : GoogleDocsUtils.BorderType.Bottom));
			}

			foreach (var data in columnsWithValidation.Values) {
				var f = 0.8f * data.Collumn / columnIndex;
				var range = data.GetRangeInfinity(sheet, headerCount).AtDimension(sheetValue.MajorDimension);
				requests.Add(GoogleDocsUtils.SetValidation(sheetValue, range, data.ValidationRule.ConditionType, data.ValidationRule.Strict, data.ValidationRule.Values));
				if (data.ValidationRule.IsRequired) requests.Add(GoogleDocsUtils.SetRequiredFormatRule(data.GetRange(sheet, headerCount).AtDimension(sheetValue.MajorDimension)));

				if (sheetData.Settings.WithEnumColors) {
					var color = sheetData.Settings.EnumColors.Evaluate(f);
					var endColor = sheetData.Settings.EnumColors.Evaluate(f + 0.2f);

					if (data.ValidationRule.ConditionType == ConditionType.ONE_OF_LIST)
						requests.AddRange(GoogleDocsUtils.SetFormatRule(range, color, endColor, data.ValidationRule.Values.Select(o => o.ToString()).ToList()));
				}
			}

			foreach (((var rowIndex, var colIndex), var property) in headers) {
				var range = GoogleDocsUtils.GetRange(sheet, colIndex, rowIndex, colIndex + 1, rowIndex + 1).AtDimension(sheetValue.MajorDimension);
				requests.Add(GoogleDocsUtils.SetTooltip(range, property.Tooltip));
			}

			if (sizeColumns.Any(pair => pair.Value.FixedSize > 0)) {
				foreach ((var index, var property) in sizeColumns) {
					requests.Add(property.FixedSize <= 0 ? sheet.AutoResizeColumns(index, index + 1) : sheet.ResizeColumns(property.FixedSize, index, index + 1));

					if (property.FixedSize > 0) {
						var range = GoogleDocsUtils.GetRange(sheet, index, headerCount, index + 1).AtDimension(sheetValue.MajorDimension);
						requests.Add(GoogleDocsUtils.SetWrap(range, property.WrapStrategy));
					}
				}
			}
			else {
				//после всех модификаций
				requests.Add(sheet.AutoResizeColumns());
			}
		}

		private static IEnumerable<SheetColumnValues> Read(Sheet sheet, ValueRange sheetValue, ISheetData sheetData) {
			var title = sheet.Properties.Title;

			var headerRange = SheetsImport.GetHeaderRange(sheet).AtDimension(sheetValue.MajorDimension);
			if (headerRange is not { EndRowIndex: { } }) throw new NoHeaderException($"No header in sheet {title}");

			var headerCount = headerRange.EndRowIndex.Value;

			for (var columnIndex = 0; columnIndex < sheetValue.Values.Count; columnIndex++) {
				var column = GetColumnFromHeader(sheet, sheetValue, columnIndex, headerCount);
				if (column == null) throw new Exception($"Can't read column {GoogleDocsUtils.GetColumnName(columnIndex)} in sheet {title}");

				var columnValues = new SheetColumnValues(column);
				columnValues.AddRange(sheetValue.Values[columnIndex].Skip(headerCount));
				columnValues.CropEmpty();
				yield return columnValues;
			}
		}

		private static SheetColumnData GetColumnFromHeader(Sheet sheet, ValueRange sheetValue, int columnIndex, int headerCount) {
			SheetColumnData column = null;

			var i = 0;
			while (i < headerCount) {
				var header = SheetsImport.GetHeaderValue(sheet.Merges?.Select(range => range.AtDimension(sheetValue.MajorDimension)), sheetValue, columnIndex, i);
				if (header == null) break;

				i++;
				column = SheetColumnData.Parse(header, column);
			}

			return column;
		}

		private static IEnumerable<SheetColumnValues> Combine(Sheet sheet, IList<SheetColumnValues> exported, IList<SheetColumnValues> read, int? sortByColumn) {
			var title = sheet.Properties.Title;

			var keyColumns = new List<string>();
			var exportedIndexByPath = exported.ToColumnIndexByPath(out var exportedRowCount, (column, i) => {
				if (column.Property.IsKey) keyColumns.Add(column.Path);
			});

			var extraColumns = new List<int>();
			var readIndexByPath = read.ToColumnIndexByPath(out var readRowCount, (column, i) => {
				if (column.IsExtra) extraColumns.Add(i);
			});

			if (keyColumns.Count > 0) {
				var rowsOrder = new List<int>();
				var rowsToRemove = new List<(int, bool)>();
				for (var rr = 0; rr < readRowCount; rr++) {
					int? index = null;
					for (var er = 0; er < exportedRowCount; er++) {
						var equals = keyColumns.All(path => {
							var x = readIndexByPath.TryGetValue(path, out var rc) && rr < read[rc].Count
								? read[rc][rr]
								: null;

							var y = exportedIndexByPath.TryGetValue(path, out var ec) && er < exported[ec].Count
								? exported[ec][er]
								: null;

							if (y != null && y is string s && s.StartsWith("'")) y = s[1..];

							return x != null && y != null && Convert.ChangeType(x, y.GetType()).Equals(y);
						});

						if (!equals) continue;

						index = er;
						break;
					}

					var duplicate = index.HasValue && rowsOrder.Contains(index.Value);
					if (duplicate || !index.HasValue)
						rowsToRemove.Add((rr, duplicate));
					else
						rowsOrder.Add(index.Value);
				}

				read.RemoveRows(rowsToRemove, keyColumns, sheet);
				if (sortByColumn.HasValue) {
					var sortedIndices = exported[sortByColumn.Value]
						.Select((x, i) => ((string)x, i))
						.OrderBy(x => x.Item1[..Mathf.Max(x.Item1.LastIndexOf(".", StringComparison.Ordinal), 0)])
						.ThenBy(x => Regex.Replace(x.Item1[Mathf.Max(x.Item1.LastIndexOf(".", StringComparison.Ordinal), 0)..], "[^0-9]", "").TryParse(out int num) ? num : -1)
						.Select(x => x.i)
						.ToArray();
					exported.SortRows(sortedIndices, exportedRowCount, keyColumns, sheet);
				}
				else
					exported.SortRows(rowsOrder, exportedRowCount, keyColumns, sheet);
			}
			else {
				var delta = exportedRowCount - readRowCount;
				if (delta != 0) Debug.Log($"{Math.Abs(delta)} rows {(delta > 0 ? "added to" : "removed from")} sheet {title}");
			}

			var shiftedExtraColumns = new Queue<int>();
			var count = Mathf.Max(read.Count, exported.Count);
			for (var columnIndex = 0; columnIndex < count; columnIndex++) {
				var canInsert = columnIndex == 0 ||
					columnIndex >= exported.Count ||
					exported[columnIndex - 1].Column.Top.Path != exported[columnIndex].Column.Top.Path;

				if (canInsert) {
					while (shiftedExtraColumns.Count > 0) yield return read[shiftedExtraColumns.Dequeue()].Trim(exportedRowCount);
				}

				if (extraColumns.Contains(columnIndex)) {
					if (!canInsert)
						shiftedExtraColumns.Enqueue(columnIndex);
					else
						yield return read[columnIndex].Trim(exportedRowCount);
				}
				else if (columnIndex < read.Count) {
					var readColumnPath = read[columnIndex].Column.Path;
					if (!exportedIndexByPath.Keys.Contains(readColumnPath)) Debug.Log($"Column removed from sheet {title}: {readColumnPath}");
				}

				if (columnIndex >= exported.Count) continue;

				var exportedValues = exported[columnIndex];
				var exportedColumnPath = exportedValues.Column.Path;
				if (readIndexByPath.TryGetValue(exportedColumnPath, out var index)) {
					var readValues = read[index];
					for (var rowIndex = 0; rowIndex < exportedValues.Count; rowIndex++) {
						if (rowIndex < readValues.Count && readValues.ValueIsFormula(rowIndex)) exportedValues[rowIndex] = readValues[rowIndex];
					}
				}
				else {
					Debug.Log($"Column added to sheet {title}: {exportedColumnPath}");
				}

				yield return exportedValues;
			}
		}

		private static Dictionary<string, int> ToColumnIndexByPath(
			this IList<SheetColumnValues> values,
			out int rowCount,
			Action<SheetColumnData, int> columnHandler) {
			rowCount = 0;

			var result = new Dictionary<string, int>();
			for (int i = 0; i < values.Count; i++) {
				var columnValues = values[i];
				var column = columnValues.Column;

				columnHandler(column, i);

				var count = columnValues.Count;
				if (count > rowCount) rowCount = count;

				result.Add(column.Path, i);
			}

			return result;
		}

		private static void RemoveRows(this IEnumerable<SheetColumnValues> values, IList<(int, bool)> rowsToRemove, IList<string> keyColumns, Sheet sheet) {
			var removed = new List<(List<Tuple<string, object>> list, bool duplicate)>();
			foreach (var columnValues in values) {
				var path = columnValues.Column.Path;
				var logColumn = keyColumns.Contains(path) || path == FileName;

				for (int i = rowsToRemove.Count - 1, j = 0; i >= 0; i--, j++) {
					(var index, var duplicate) = rowsToRemove[i];
					if (index >= columnValues.Count) continue;

					if (logColumn) {
						List<Tuple<string, object>> list;
						if (j < removed.Count)
							list = removed[j].list;
						else
							removed.Add((list = new List<Tuple<string, object>>(), duplicate));

						list.Add(new Tuple<string, object>(path, columnValues[index]));
					}

					columnValues.RemoveAt(index);
				}
			}

			var title = sheet.Properties.Title;
			foreach (var (list, duplicate) in removed)
				Debug.Log($"{(duplicate ? "DUPLICATE " : "")}Row removed from sheet {title}: {string.Join("; ", list.Select(tuple => $"{tuple.Item1} = {tuple.Item2}"))}");
		}

		private static void SortRows(this IEnumerable<SheetColumnValues> values, IList<int> rowsOrder, int rowCount, IList<string> keyColumns, Sheet sheet) {
			var added = new List<List<Tuple<string, object>>>();
			foreach (var columnValues in values) {
				var path = columnValues.Column.Path;
				var logColumn = keyColumns.Contains(path) || path == FileName;

				var addIndex = 0;
				var ordered = new object[rowCount];
				for (var i = 0; i < columnValues.Count; i++) {
					var value = columnValues[i];
					var orderIndex = rowsOrder.IndexOf(i);
					if (orderIndex < 0) {
						if (logColumn) {
							List<Tuple<string, object>> list;
							if (addIndex < added.Count)
								list = added[addIndex];
							else
								added.Add(list = new List<Tuple<string, object>>());

							list.Add(new Tuple<string, object>(path, value));
						}

						orderIndex = rowsOrder.Count + addIndex;
						addIndex++;
					}

					ordered[orderIndex] = value;
				}

				columnValues.Replace(ordered);
			}

			var title = sheet.Properties.Title;
			foreach (var list in added) Debug.Log($"Row added to sheet {title}: {string.Join("; ", list.Select(tuple => $"{tuple.Item1} = {tuple.Item2}"))}");
		}

		private class MergedRangeData {
			public int RowMin;
			public int RowMax;
			public int ColumnMin;
			public int ColumnMax;

			public MergedRangeData(int row, int column) {
				RowMin = RowMax = row;
				ColumnMin = ColumnMax = column;
			}

			public void AddColumn(int c) {
				ColumnMin = Mathf.Min(ColumnMin, c);
				ColumnMax = Mathf.Max(ColumnMax, c);
			}

			public void AddRow(int c) {
				RowMin = Mathf.Min(RowMin, c);
				RowMax = Mathf.Max(RowMax, c);
			}

			public int TotalCells => (ColumnMax - ColumnMin + 1) * (RowMax - RowMin + 1);
			public GridRange GetRange(Sheet sheet) => GoogleDocsUtils.GetRange(sheet, ColumnMin, RowMin, ColumnMax + 1, RowMax + 1);

			public void OffsetRow(int offset) {
				RowMin += offset;
				RowMax += offset;
			}
		}

		private class ValueEnumRangeData {
			private readonly int _column;
			private int _rowCount;
			public ValueValidationRule ValidationRule { get; }
			public int Collumn => _column;

			public ValueEnumRangeData(int column, int rowCount, ValueValidationRule validationRule) {
				_column = column;
				_rowCount = rowCount;
				ValidationRule = validationRule;
			}

			public void InsertRow() => _rowCount++;

			public GridRange GetRange(Sheet sheet, int headerCount) => GoogleDocsUtils.GetRange(sheet, _column, headerCount, _column + 1, _rowCount);
			public GridRange GetRangeInfinity(Sheet sheet, int headerCount) => GoogleDocsUtils.GetRange(sheet, _column, headerCount, _column + 1);
		}

		private static int GetRowIndex(SheetColumnData data) {
			return data.Index.Hierarchy.Count - 1;
		}
	}

}
#endif