#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util;
using UnityEngine;
using XLib.Configs.Sheets.Contracts;
using XLib.Configs.Sheets.Types;
using Color = Google.Apis.Sheets.v4.Data.Color;

namespace XLib.Configs.Platforms {

	public static class GoogleDocsUtils {
		public enum BorderType {
			Top,
			Bottom,
			Left,
			Right,
			InnerHorizontal,
			InnerVertical,
		}

		public enum ValueInputType {
			RAW,
			USER_ENTERED,
		}
		
		private const string CLIENT_ID = "";
		private const string CLIENT_SECRET = "";
		private const string PROJECT_ID = "";

		public static SheetsService GetSpreadsheets(bool readOnly = false) =>
			new(new BaseClientService.Initializer {
				HttpClientInitializer = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets { ClientId = CLIENT_ID, ClientSecret = CLIENT_SECRET },
						new[] { readOnly ? SheetsService.Scope.SpreadsheetsReadonly : SheetsService.Scope.Spreadsheets },
						"user",
						CancellationToken.None)
					.Result,
				ApplicationName = PROJECT_ID
			});

		public static void Open(this Spreadsheet spreadsheet) => Open(spreadsheet.SpreadsheetId);

		public static void Open(string spreadsheetId) => Application.OpenURL($"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit");

		public static void Open(string spreadsheetId, Sheet sheet, int row) =>
			Application.OpenURL($"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit#gid={sheet.Properties.SheetId}&range={++row}:{row}");

		public static Spreadsheet GetSpreadsheet(this SheetsService service, string spreadsheetId, Repeatable<string> ranges = null, bool includeGridData = false) {
			var request = service.Spreadsheets.Get(spreadsheetId);
			request.IncludeGridData = includeGridData;
			if (ranges != null) request.Ranges = ranges;
			var result = request.ExecuteAsync();
			result.Wait(CancellationToken.None);
			return result.Result;
		}

		public static Spreadsheet CreateSpreadsheet(this SheetsService service, string title) =>
			service.Spreadsheets.Create(new Spreadsheet { Properties = new SpreadsheetProperties { Title = title, } }).Execute();

		public static Spreadsheet BatchUpdate(this SheetsService service, IList<Request> requests, string spreadsheetId) {
			var offset = 0;
			BatchUpdateSpreadsheetResponse response = null;
			while (offset <= requests.Count) {
				var batch = requests.Skip(offset).Take(50000).ToList();
				response = service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest { Requests = batch, IncludeSpreadsheetInResponse = true, }, spreadsheetId).Execute();
				offset += 50000;
			}

			return response?.UpdatedSpreadsheet;
		}

		public static void BatchUpdateValues(this SheetsService service, IList<ValueRange> values, string spreadsheetId, ValueInputType inputType = ValueInputType.RAW) =>
			service.Spreadsheets.Values.BatchUpdate(new BatchUpdateValuesRequest { Data = values, ValueInputOption = inputType.ToString(), }, spreadsheetId).Execute();

		public static void Append(this SheetsService service, ValueRange value, string spreadsheetId,
			SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum inputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW) {
			var request = service.Spreadsheets.Values.Append(value, spreadsheetId, value.Range);
			request.ValueInputOption = inputOption;
			request.Execute();
		}

		public static ValueRange GetValue(this SheetsService service, Sheet sheet, string spreadsheetId,
			SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum majorDimension = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.COLUMNS,
			SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum renderOption =
				SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMATTEDVALUE) {
			var request = service.Spreadsheets.Values.Get(spreadsheetId, sheet.Properties.Title);
			request.MajorDimension = majorDimension;
			request.ValueRenderOption = renderOption;
			return request.Execute();
		}

		public static Dictionary<string, Sheet> GetSheetsByTitle(this Spreadsheet spreadsheet) => spreadsheet.Sheets.ToDictionary(sheet => sheet.Properties.Title, sheet => sheet);

		public static ValueRange CreateCellValue(Sheet sheet, int column, int row, object value) =>
			new() {
				Values = new IList<object>[] { new[] { value } },
				Range = GetRange(sheet, column, row, column + 1, row + 1)
					.ToA1Notation(sheet),
			};

		public static GridRange GetRange(Sheet sheet, int? startColIndex = null, int? startRowIndex = null, int? endColIndex = null, int? endRowIndex = null) =>
			new() {
				SheetId = sheet.Properties.SheetId,
				StartColumnIndex = startColIndex,
				StartRowIndex = startRowIndex,
				EndColumnIndex = endColIndex,
				EndRowIndex = endRowIndex,
			};

		public static string ToA1Notation(this GridRange range, Sheet sheet) => 
			$"'{sheet.Properties.Title}'" + A1NotationWithOutSheetName(range);

		public static string A1NotationWithOutSheetName(this GridRange range) {
			var result = string.Empty;
			//- range is from 0, A1 is from 1
			//- range end is exclusive, A1 is inclusive
			if (range.StartColumnIndex.HasValue) {
				result += $"!{GetColumnName(range.StartColumnIndex.Value)}";

				if (range.StartRowIndex.HasValue) result += $"{range.StartRowIndex + 1}";

				if (range.EndColumnIndex.HasValue) {
					result += $":{GetColumnName(range.EndColumnIndex.Value - 1)}";

					if (range.EndRowIndex.HasValue) result += $"{range.EndRowIndex}";
				}
			}
			else if (range.StartRowIndex.HasValue) {
				result += $"!{range.StartRowIndex + 1}";

				if (range.EndRowIndex.HasValue) result += $":{range.EndRowIndex}";
			}

			return result;
		}

		public static string GetColumnName(int column) {
			var d = column / 26f;
			var m = column % 26;
			return d >= 1
				? GetColumnName((int)d - 1) + GetColumnName(m)
				: ((char)('A' + column)).ToString();
		}

		public static Request AddSheet(string title) => new() { AddSheet = new AddSheetRequest { Properties = new SheetProperties { Title = title } } };

		public static Request DeleteSheet(this Sheet sheet) => new() { DeleteSheet = new DeleteSheetRequest { SheetId = sheet.Properties.SheetId } };

		public static Request AutoResizeColumns(this Sheet sheet, int? startIndex = null, int? endIndex = null) =>
			new() {
				AutoResizeDimensions = new AutoResizeDimensionsRequest {
					Dimensions = new DimensionRange {
						Dimension = "COLUMNS",
						SheetId = sheet.Properties.SheetId,
						StartIndex = startIndex,
						EndIndex = endIndex,
					}
				}
			};

		public static Request ResizeColumns(this Sheet sheet, int pixelSize, int? startIndex = null, int? endIndex = null) =>
			new() {
				UpdateDimensionProperties = new UpdateDimensionPropertiesRequest {
					Range = new DimensionRange {
						Dimension = "COLUMNS",
						SheetId = sheet.Properties.SheetId,
						StartIndex = startIndex,
						EndIndex = endIndex
					},
					Properties = new DimensionProperties { PixelSize = pixelSize },
					Fields = "pixelSize",
				}
			};

		public static Request SetColumnsVisible(this Sheet sheet, bool hidden, string dimension,  int? startIndex = null, int? endIndex = null) =>
			new() {
				UpdateDimensionProperties = new UpdateDimensionPropertiesRequest {
					Range = new DimensionRange {
						Dimension = dimension,
						SheetId = sheet.Properties.SheetId,
						StartIndex = startIndex,
						EndIndex = endIndex
					},
					Properties = new DimensionProperties { HiddenByUser = hidden },
					Fields = "hiddenByUser",
				}
			};

		public static Request UpdateFrozenRows(Sheet sheet, int rowCount, int columnCount) =>
			new() {
				UpdateSheetProperties = new UpdateSheetPropertiesRequest {
					Properties = new SheetProperties {
						SheetId = sheet.Properties.SheetId, GridProperties = new GridProperties { FrozenRowCount = rowCount, FrozenColumnCount = columnCount }
					},
					Fields = "gridProperties.frozenRowCount,gridProperties.frozenColumnCount",
				}
			};

		public static IEnumerable<Request> UpdateSheetsTitles(IEnumerable<Sheet> sheets, string title) => sheets.Select((sheet, i) => UpdateSheetTitle(sheet, $"{title} {i}"));

		public static Request UpdateSheetTitle(Sheet sheet, string title) =>
			new() {
				UpdateSheetProperties =
					new UpdateSheetPropertiesRequest { Properties = new SheetProperties { SheetId = sheet.Properties.SheetId, Title = title, }, Fields = "title" }
			};

		public static Request UpdateSpreadsheetTitle(string title) =>
			new() { UpdateSpreadsheetProperties = new UpdateSpreadsheetPropertiesRequest { Properties = new SpreadsheetProperties { Title = title }, Fields = "title" } };

		public static Request AddProtectedRange(GridRange range, string description) =>
			new() { AddProtectedRange = new AddProtectedRangeRequest { ProtectedRange = new ProtectedRange { Range = range, Description = description, WarningOnly = true } } };

		public static Request DeleteProtectedRange(int protectedRangeId) =>
			new() { DeleteProtectedRange = new DeleteProtectedRangeRequest { ProtectedRangeId = protectedRangeId } };

		public static Request ClearValues(GridRange range) =>
			new() {
				RepeatCell = new RepeatCellRequest {
					Range = range, Cell = new CellData { UserEnteredValue = new ExtendedValue(), }, Fields = "userEnteredValue",
				},
			};

		public static Request SetWrap(GridRange range, WrapStrategy wrap) =>
			new() {
				RepeatCell = new RepeatCellRequest {
					Range = range, Cell = new CellData { UserEnteredFormat = new CellFormat { WrapStrategy = wrap.ToString(), } }, Fields = "userEnteredFormat.wrapStrategy",
				}
			};

		public static Request SetFormat(GridRange range, UnityEngine.Color? color, UnityEngine.Color? textColor, int alignHor = -1, bool bold = false) =>
			new() {
				RepeatCell = new RepeatCellRequest {
					Range = range,
					Cell = new CellData {
						UserEnteredFormat = new CellFormat {
							BackgroundColor = color != null ? GetColor(color.Value) : null,
							TextFormat = new TextFormat { Bold = bold, ForegroundColor = (textColor != null ? GetColor(textColor.Value) : null) },
							HorizontalAlignment = alignHor == 0 ? "CENTER" : (alignHor < 0 ? "LEFT" : "RIGHT"),
						}
					},
					Fields = "userEnteredFormat(backgroundColor,textFormat,horizontalAlignment)",
				}
			};

		public static Request SetTooltip(GridRange range, string tooltip) =>
			new() { RepeatCell = new RepeatCellRequest { Range = range, Cell = new CellData { Note = tooltip }, Fields = "note" } };

		public static Request ClearFormat(GridRange range) =>
			new() {
				RepeatCell = new RepeatCellRequest {
					Range = range, Cell = new CellData { UserEnteredFormat = new CellFormat(), }, Fields = "userEnteredFormat",
				},
			};

		public static Request SetValidation(ValueRange sheetValue, GridRange range, ConditionType conditionType, bool strict, IEnumerable<object> values) {
			var valuesHashSet = values.Select(o => o.ToString()).ToHashSet();
			ConditionValue[] allValues;
			if (valuesHashSet.Count > 500) {
				var newHashSet = sheetValue.Values.SelectMany(list => list.Where(o => o is string s && valuesHashSet.Contains(s))).OfType<string>().ToHashSet();

				foreach (var hash in valuesHashSet) {
					if (newHashSet.Count < 500)
						newHashSet.Add(hash);
					else
						break;
				}
				allValues = newHashSet.OrderBy(s => s).Select(value => new ConditionValue { UserEnteredValue = value })
					.ToArray();

				Debug.LogWarning($"validation: 500 value limit exceeded {range.A1NotationWithOutSheetName()}");
				allValues = allValues.Take(500).ToArray();
			}
			else {
				allValues = valuesHashSet.OrderBy(s => s).Select(value => new ConditionValue { UserEnteredValue = value })
					.ToArray();
			}

			return new() {
				SetDataValidation = new SetDataValidationRequest {
					Range = range,
					Rule = new DataValidationRule {
						Condition = new BooleanCondition { Type = conditionType.ToString(), Values = allValues, }, Strict = strict, ShowCustomUi = true,
					}
				}
			};
		}

		public static Request SetBoolValidation(GridRange range) =>
			new() {
				SetDataValidation = new SetDataValidationRequest {
					Range = range,
					Rule = new DataValidationRule {
						Condition = new BooleanCondition { Type = "BOOLEAN" }, Strict = true, ShowCustomUi = true,
					}
				}
			};

		public static IEnumerable<Request> SetFormatRule(GridRange range, UnityEngine.Color from, UnityEngine.Color to, ICollection<string> values) {
			var count = values.Count;
			return values.Select((v, i) => new Request() {
				AddConditionalFormatRule = new AddConditionalFormatRuleRequest() {
					Rule = new ConditionalFormatRule() {
						Ranges = new List<GridRange>() { range },
						BooleanRule = new BooleanRule() {
							Condition = new BooleanCondition() { Type = "TEXT_EQ", Values = new List<ConditionValue>() { new() { UserEnteredValue = v } } },
							Format = new CellFormat() {
								TextFormat = new TextFormat() { Bold = true }, BackgroundColor = GetColor(UnityEngine.Color.Lerp(from, to, (float)i / count))
							}
						}
					}
				},
			});
		}

		public static Request SetRequiredFormatRule(GridRange range) {
			return new Request() {
				AddConditionalFormatRule = new AddConditionalFormatRuleRequest() {
					Rule = new ConditionalFormatRule() {
						Ranges = new List<GridRange>() { range },
						BooleanRule = new BooleanRule() {
							Condition = new BooleanCondition() { Type = ConditionType.BLANK.ToString() },
							Format = new CellFormat() { BackgroundColor = GetColor(UnityEngine.Color.Lerp(UnityEngine.Color.red, UnityEngine.Color.white, 0.5f)) }
						}
					}
				}
			};
		}

		public static Request ClearValidation(GridRange range) => new() { SetDataValidation = new SetDataValidationRequest { Range = range } };

		public static IEnumerable<Request> ClearFormatRules(Sheet sheet) {
			var sheetId = sheet.Properties.SheetId;
			if (sheet.ConditionalFormats == null) yield break;
			for (var i = sheet.ConditionalFormats.Count - 1; i >= 0; i--) {
				yield return new Request() { DeleteConditionalFormatRule = new DeleteConditionalFormatRuleRequest() { Index = i, SheetId = sheetId } };
			}
		}

		public static Request Merge(GridRange range) => new() { MergeCells = new MergeCellsRequest { Range = range, MergeType = "MERGE_ALL" } };

		public static Request Unmerge(GridRange range) => new() { UnmergeCells = new UnmergeCellsRequest { Range = range } };

		public static Request UpdateBorders(GridRange range, BorderType type, UnityEngine.Color? color = null) {
			var border = new Border { Style = "SOLID", Color = GetColor(color ?? UnityEngine.Color.black) };

			return new Request {
				UpdateBorders = new UpdateBordersRequest {
					Range = range,
					Top = type == BorderType.Top ? border : null,
					Bottom = type == BorderType.Bottom ? border : null,
					Left = type == BorderType.Left ? border : null,
					Right = type == BorderType.Right ? border : null,
					InnerHorizontal = type == BorderType.InnerHorizontal ? border : null,
					InnerVertical = type == BorderType.InnerVertical ? border : null,
				}
			};
		}

		public static UnityEngine.Color GetColor(Color color) =>
			new() {
				r = color.Red ?? 0,
				g = color.Green ?? 0,
				b = color.Blue ?? 0,
				a = color.Alpha ?? 1,
			};

		private static Color GetColor(UnityEngine.Color color) =>
			new() {
				Blue = color.b,
				Green = color.g,
				Red = color.r,
				Alpha = color.a,
			};

		public static Request SetBasicFilter(GridRange range) => new() { SetBasicFilter = new SetBasicFilterRequest() { Filter = new BasicFilter() { Range = range } } };

		public static Request ClearBasicFilter(int? sheetId) => new() { ClearBasicFilter = new ClearBasicFilterRequest() { SheetId = sheetId } };
	}

}
#else
namespace XLib.Configs.Sheets.Platforms { }
#endif