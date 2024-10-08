using System;
using System.Collections.Generic;
using System.Linq;
using XLib.Core.Parsers.Exceptions;

namespace XLib.Core.Parsers.Internal {

	/// <summary>
	///     one row in a table
	/// </summary>
	internal class RawTableRow : IRawTableRow {

		private readonly Dictionary<string, int> _header;
		private readonly IList<string> _values;

		internal RawTableRow(int index, string location, Dictionary<string, int> header, IList<string> values) {
			Index = index;
			Location = location;
			_header = header;
			_values = values;
		}

		public int Index { get; }
		public string Location { get; }

		public IEnumerable<string> Headers => _header.Keys;

		public bool HasColumnInHeader(string columnName) {
			if (columnName.IsNullOrEmpty()) return false;

			return _header.ContainsKey(columnName);
		}

		public bool IsEmpty() {
			return _values.Count == 0 || _values.All(raw => raw == null || raw.ToString().Trim() == string.Empty);
		}

		public bool IsEmpty(string columnName) {
			var val = RawValue(columnName)?.Trim();

			return val.IsNullOrEmpty();
		}

		public string RawValue(string columnName) => RawValue(columnName, out var result) && !result.IsNullOrEmpty() ? result : null;

		public bool RawValue(string columnName, out string value) {
			if (columnName.IsNullOrEmpty()) {
				value = null;
				return false;
			}

			if (!_header.TryGetValue(columnName, out var index)) {
				value = null;
				return false;
			}

			value = _values.GetOrDefaultList(index);
			return true;
		}

		public Exception MakeError(string message) => new RowValueException($"Error at location:\n{Location}\n\n'{message}'");

		public override string ToString() => $"{Location}: {_values.JoinToString()}";

	}

}