using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using XLib.Core.CommonTypes;
using XLib.Core.Parsers.Internal;
using XLib.Core.Utils;
using Range = XLib.Core.CommonTypes.Range;

namespace XLib.Core.Parsers {

	public static class RawTableExtensions {

		/// <summary>
		///     interpret table as key=value pairs with two headers and use this as raw table row
		///     keys must be unique!
		/// </summary>
		public static IRawTableRow ToKeyValue(this IEnumerable<IRawTableRow> rows, string keyColumn, string valueColumn) => new KeyValueTable(rows, keyColumn, valueColumn);

		/// <summary>
		///     interpret table as key=value pairs with two headers and use this as raw table row
		///     keys must be unique!
		/// </summary>
		public static IRawTableRow ToKeyValue(this RawTable table, string keyColumn, string valueColumn) => new KeyValueTable(table, keyColumn, valueColumn);

		/// <summary>
		///     join multiple rows into one long virtual row.
		///     headers must be unique in all rows
		/// </summary>
		public static IRawTableRow Join(IEnumerable<IRawTableRow> rows) => new JoinedRow(rows);

		/// <summary>
		///     join multiple rows into one long virtual row.
		///     headers must be unique in all rows
		/// </summary>
		public static IRawTableRow Join(this IRawTableRow row, IRawTableRow otherRow, params IRawTableRow[] extraRows) => new JoinedRow(extraRows.Append(row).Append(otherRow));

		/// <summary>
		///     filter columns by name prefix and optionally remove prefix from column name
		/// </summary>
		public static IRawTableRow SelectColumns(this IRawTableRow row, string prefix, bool removePrefix) => new FilteredRow(row, prefix, removePrefix);

		/// <summary>
		///     replace one RAW value with another
		/// </summary>
		public static IRawTableRow RawReplace(this IRawTableRow row, string columnName, string newRawValue) =>
			RawReplace(row, new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { columnName, newRawValue } });

		/// <summary>
		///     replace one RAW value with another
		/// </summary>
		private static IRawTableRow RawReplace(IRawTableRow row, Dictionary<string, string> columnValues) => new ReplaceRow(row, columnValues);

		public static string FormatLocation(string context, object rowIndex) => $"{context}[row={rowIndex}]";

		/// <summary>
		///     get row from raw values
		/// </summary>
		public static IRawTableRow MakeRow(string columnName, string value, string location = "Raw") => MakeRow(new Dictionary<string, string> { { columnName, value } }, location);

		/// <summary>
		///     get row from raw values
		/// </summary>
		public static IRawTableRow MakeRow(Dictionary<string, string> keys2Values, string location = "",
			int rowIndex = 0) {
			var kvp = keys2Values.OrderBy(x => x.Key)
				.Select((x, index) => new { Index = index, x.Key, x.Value })
				.ToArray();

			return new RawTableRow(rowIndex, FormatLocation(location, rowIndex),
				kvp.ToDictionary(x => x.Key, x => x.Index),
				kvp.Select(x => x.Value).ToArray());
		}

		public static string[] GetStringArray(this IRawTableRow row, string columnName, bool optional = false, char separator = ',') {
			return (optional ? row.GetString(columnName, string.Empty) : row.GetString(columnName))
				.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
		}

		public static string GetString(this IRawTableRow row, string columnName) {
			var raw = row.RawValue(columnName);
			if (raw == null) throw row.MakeError($"'{columnName}' column not found or empty!'");

			return raw;
		}

		public static string GetString(this IRawTableRow row, string columnName, string defaultVal) {
			var raw = row.RawValue(columnName);
			return raw ?? defaultVal;
		}

		public static bool GetBool(this IRawTableRow row, string columnName, bool? defaultVal = null) {
			if (!row.RawValue(columnName, out var raw)) return defaultVal ?? throw row.MakeError($"'{columnName}' column not found or empty!'");

			var v = raw?.Trim().ToLowerInvariant();

			if (v == "true" || v == "1" || v == "yes" || v == "y" || v == "+" || v == "x") return true;

			if (v.IsNullOrEmpty() || v == "false" || v == "0" || v == "no" || v == "n" || v == "-") return false;

			throw row.MakeError($"Cannot convert column '{columnName}' ({raw}) to bool!");
		}

		public static int GetInt(this IRawTableRow row, string columnName, int? defaultVal = null) {
			var raw = row.RawValue(columnName);
			if (raw == null) return defaultVal ?? throw row.MakeError($"'{columnName}' column not found or empty!'");

			return int.TryParse(raw, out var result)
				? result
				: throw row.MakeError($"Cannot convert column '{columnName}' ({raw}) to int32!");
		}

		public static float GetFloat(this IRawTableRow row, string columnName, float? defaultVal = null) {
			var raw = row.RawValue(columnName);
			if (raw == null) return defaultVal ?? throw row.MakeError($"'{columnName}' column not found or empty!'");

			return float.TryParse(raw.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture,
				out var result)
				? result
				: throw row.MakeError($"Cannot convert column '{columnName}' ({raw}) to float!");
		}

		public static double GetDouble(this IRawTableRow row, string columnName, double? defaultVal = null) {
			var raw = row.RawValue(columnName);
			if (raw == null) return defaultVal ?? throw row.MakeError($"'{columnName}' column not found or empty!'");

			return double.TryParse(raw.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture,
				out var result)
				? result
				: throw row.MakeError($"Cannot convert column '{columnName}' ({raw}) to double!");
		}

		public static T GetEnum<T>(this IRawTableRow row, string columnName, T? defaultVal = null) where T : unmanaged, Enum {
			var raw = row.RawValue(columnName);
			if (raw == null) return defaultVal ?? throw row.MakeError($"'{columnName}' column not found or empty!'");

			try {
				return Enums.ToEnum<T>(raw);
			}
			catch (Exception e) {
				throw row.MakeError($"Cannot convert column '{columnName}' ({raw}): {e.Message}");
			}
		}

		public static object GetEnum(this IRawTableRow row, string columnName, Type enumType, Enum defaultVal = null) {
			var raw = row.RawValue(columnName);
			if (raw == null) return defaultVal ?? throw row.MakeError($"'{columnName}' column not found or empty!'");

			try {
				return Enums.ToEnum(enumType, raw);
			}
			catch (Exception e) {
				throw row.MakeError($"Cannot convert column '{columnName}' ({raw}): {e.Message}");
			}
		}

		public static BigInteger GetBigInteger(this IRawTableRow row, string columnName, BigInteger? defaultVal = null) {
			var raw = row.RawValue(columnName);
			if (raw == null) return defaultVal ?? throw row.MakeError($"'{columnName}' column not found or empty!'");

			return BigInteger.TryParse(raw.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture,
				out var result)
				? result
				: throw row.MakeError($"Cannot convert column '{columnName}' ({raw}) to BigInteger!");
		}

		public static DateTime GetDateTimeUTC(this IRawTableRow row, string columnName, string format, DateTime? defaultVal = null) {
			var raw = row.RawValue(columnName);
			if (raw == null) return defaultVal ?? throw row.MakeError($"'{columnName}' column not found or empty!'");

			var dateString = raw.Trim();
			if (dateString == string.Empty) return defaultVal ?? throw row.MakeError($"Cannot convert column '{columnName}' ({raw}) to DateTime!");

			return DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
				out var result)
				? new DateTime(result.Ticks, DateTimeKind.Utc)
				: defaultVal ??
				throw row.MakeError($"Cannot convert column '{columnName}' ({raw}) to DateTime ({format})!");
		}

		public static Range GetRange(this IRawTableRow row, string columnName) {
			var items = row.GetString(columnName, string.Empty).SplitIntArray('-');

			if (items.IsNullOrEmpty() || (items.Length != 1 && items.Length != 2)) throw row.MakeError($"Must be 'int-int' or 'int' in '{columnName}'");

			return items.Length == 1 ? new Range(items[0], items[0]) : new Range(items[0], items[1]);
		}

		public static RangeF GetRangeF(this IRawTableRow row, string columnName) {
			var items = row.GetString(columnName, string.Empty).SplitFloatArray('-');

			if (items.IsNullOrEmpty() || (items.Length != 1 && items.Length != 2)) throw row.MakeError($"Must be 'float-float' or 'float' in '{columnName}'");

			return items.Length == 1 ? new RangeF(items[0], items[0]) : new RangeF(items[0], items[1]);
		}

		public static Range GetRange(this IRawTableRow row, string columnWithMin, string columnWithMax) => new(row.GetInt(columnWithMin), row.GetInt(columnWithMax));

		public static RangeF GetRangeF(this IRawTableRow row, string columnWithMin, string columnWithMax) => new(row.GetFloat(columnWithMin), row.GetFloat(columnWithMax));

		public static T GetValue<T>(this IRawTableRow row, string columnName) => (T)row.GetValue(columnName, TypeOf<T>.Raw);

		public static object GetValue(this IRawTableRow row, string columnName, Type valueType) {
			if (valueType == TypeOf<string>.Raw) return row.GetString(columnName);

			if (valueType == TypeOf<int>.Raw) return row.GetInt(columnName);

			if (valueType == TypeOf<float>.Raw) return row.GetFloat(columnName);

			if (valueType == TypeOf<double>.Raw) return row.GetDouble(columnName);

			if (valueType == TypeOf<bool>.Raw) return row.GetBool(columnName);

			if (valueType == TypeOf<BigInteger>.Raw) return row.GetBigInteger(columnName);

			if (valueType == TypeOf<Range>.Raw) return row.GetRange(columnName);

			if (valueType == TypeOf<RangeF>.Raw) return row.GetRangeF(columnName);

			if (valueType.IsEnum) return row.GetEnum(columnName, valueType);

			throw row.MakeError($"Type '{valueType.FullName}' is not supported!");
		}

	}

}