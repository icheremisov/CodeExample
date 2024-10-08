using System;
using System.Collections.Generic;
using System.Linq;

namespace XLib.Core.Parsers.Internal {

	public class ReplaceRow : IRawTableRow {

		private readonly IRawTableRow _row;
		private readonly Dictionary<string, string> _values = new(8, StringComparer.InvariantCultureIgnoreCase);

		public ReplaceRow(IRawTableRow row, Dictionary<string, string> values) {
			_row = row;

			foreach (var keyValuePair in values) _values.Add(keyValuePair.Key, keyValuePair.Value);
		}

		public string Location => _row.Location;
		public IEnumerable<string> Headers => _row.Headers;

		public bool HasColumnInHeader(string columnName) => _row.HasColumnInHeader(columnName);

		public bool IsEmpty() => _row.Headers.All(IsEmpty);

		public bool IsEmpty(string columnName) {
			var val = RawValue(columnName)?.Trim();
			return val.IsNullOrEmpty();
		}

		public string RawValue(string columnName) => _values.TryGetValue(columnName, out var value) ? value : _row.RawValue(columnName);

		public bool RawValue(string columnName, out string value) {
			if (_values.TryGetValue(columnName, out value)) return true;

			return _row.RawValue(columnName, out value);
		}

		public Exception MakeError(string message) => _row.MakeError(message);

	}

}