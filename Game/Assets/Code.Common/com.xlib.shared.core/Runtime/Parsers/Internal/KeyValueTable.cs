using System;
using System.Collections.Generic;
using System.Linq;
using XLib.Core.Parsers.Exceptions;

namespace XLib.Core.Parsers.Internal {

	/// <summary>
	///     interpret table from rows set as key=value pairs
	/// </summary>
	internal class KeyValueTable : IRawTableRow {

		private readonly HashSet<string> _headers = new(StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<string, IRawTableRow> _values = new(32, StringComparer.InvariantCultureIgnoreCase);

		private IRawTableRow _firstRow;
		private string _valueColumn;

		public KeyValueTable(RawTable table, string keyColumn, string valueColumn) {
			if (!table.HasColumnInHeader(keyColumn)) throw new RowValueException($"Required column in header is not found: {keyColumn}");

			if (!table.HasColumnInHeader(valueColumn)) throw new RowValueException($"Required column in header is not found: {valueColumn}");

			Initialize(table, table.Headers, keyColumn, valueColumn);
		}

		public KeyValueTable(IEnumerable<IRawTableRow> rows, string keyColumn, string valueColumn) {
			Initialize(rows, null, keyColumn, valueColumn);
		}

		public string Location { get; private set; }
		public IEnumerable<string> Headers => _headers.Concat(_values.Keys);

		public bool HasColumnInHeader(string columnName) {
			if (columnName.IsNullOrEmpty()) return false;

			return _values.ContainsKey(columnName) || _headers.Contains(columnName);
		}

		public bool IsEmpty() {
			return _values.Count == 0 && (_firstRow == null || _headers.All(x => _firstRow.IsEmpty(x)));
		}

		public bool IsEmpty(string columnName) {
			if (_headers.Contains(columnName)) return _firstRow?.IsEmpty(columnName) ?? true;

			var row = GetRowByKey(columnName);
			return row?.IsEmpty(_valueColumn) ?? true;
		}

		public string RawValue(string columnName) {
			if (_headers.Contains(columnName)) return _firstRow?.RawValue(columnName);

			return RawValue(columnName, out var result) && !result.IsNullOrEmpty() ? result : null;
		}

		public bool RawValue(string columnName, out string value) {
			if (_headers.Contains(columnName)) {
				value = null;
				return _firstRow?.RawValue(columnName, out value) ?? false;
			}

			var row = GetRowByKey(columnName);

			if (row == null) {
				value = null;
				return false;
			}

			return row.RawValue(_valueColumn, out value);
		}

		public Exception MakeError(string message) => new RowValueException($"Error at location:\n{Location}\n\n'{message}'");

		private void Initialize(IEnumerable<IRawTableRow> rows, IEnumerable<string> headers, string keyColumn, string valueColumn) {
			_valueColumn = valueColumn;

			foreach (var row in rows) {
				if (_firstRow == null) {
					_firstRow = row;
					if (!_firstRow.HasColumnInHeader(keyColumn)) throw new RowValueException($"Required column in header is not found: {keyColumn}");

					if (!_firstRow.HasColumnInHeader(valueColumn)) throw new RowValueException($"Required column in header is not found: {valueColumn}");
				}

				var key = row.GetString(keyColumn, string.Empty);
				if (key.IsNullOrEmpty()) continue;

				if (key.StartsWith("~")) continue;

				if (_values.ContainsKey(key)) throw row.MakeError($"Duplicate key found in column '{keyColumn}': '{key}'");

				_values.Add(key, row);
			}

			if (_firstRow != null)
				Location = $"{_firstRow.Location} (key-value pairs {keyColumn}={valueColumn})";
			else
				Location = $"(key-value pairs {keyColumn}={valueColumn})";

			var actualHeaders = headers ?? _firstRow?.Headers;

			if (actualHeaders != null) {
				foreach (var column in actualHeaders.Where(x =>
							 !x.Equals(keyColumn, StringComparison.InvariantCultureIgnoreCase) &&
							 !x.Equals(valueColumn, StringComparison.InvariantCultureIgnoreCase)))
					_headers.Add(column);
			}
		}

		private IRawTableRow GetRowByKey(string key) => !key.IsNullOrEmpty() ? _values.FirstOrDefault(key) : null;

	}

}