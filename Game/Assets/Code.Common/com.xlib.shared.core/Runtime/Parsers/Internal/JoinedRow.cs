using System;
using System.Collections.Generic;
using System.Linq;
using XLib.Core.Parsers.Exceptions;

namespace XLib.Core.Parsers.Internal {

	/// <summary>
	///     join rows into ine table. first row used as main row for errors reporting
	/// </summary>
	internal class JoinedRow : IRawTableRow {

		private readonly Dictionary<string, IRawTableRow> _header2Row = new(32, StringComparer.InvariantCultureIgnoreCase);
		private readonly IRawTableRow[] _rows;

		public JoinedRow(IEnumerable<IRawTableRow> rows) {
			_rows = rows?.ToArray();

			if (_rows.IsNullOrEmpty()) throw new ArgumentNullException(nameof(rows), "Rows must be non-empty collection!");

			foreach (var rawTableRow in _rows) {
				foreach (var header in rawTableRow.Headers) {
					if (_header2Row.ContainsKey(header)) throw new RowValueException($"Cannot join rows: duplicate header name='{header}' found");

					_header2Row.Add(header, rawTableRow);
				}
			}
		}

		public string Location => _rows[0].Location;
		public IEnumerable<string> Headers => _header2Row.Keys;

		public bool HasColumnInHeader(string columnName) => _header2Row.ContainsKey(columnName);

		public bool IsEmpty() {
			return _rows.All(x => x.IsEmpty());
		}

		public bool IsEmpty(string columnName) {
			var row = GetRow(columnName);
			return row?.IsEmpty(columnName) ?? true;
		}

		public string RawValue(string columnName) => RawValue(columnName, out var result) && !result.IsNullOrEmpty() ? result : null;

		public bool RawValue(string columnName, out string value) {
			var row = GetRow(columnName);
			if (row == null) {
				value = null;
				return false;
			}

			return row.RawValue(columnName, out value);
		}

		public Exception MakeError(string message) => new RowValueException($"Error at location:\n{Location}\n\n'{message}'");

		private IRawTableRow GetRow(string columnName) => _header2Row.TryGetValue(columnName, out var result) ? result : null;

	}

}