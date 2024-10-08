using System;
using System.Collections.Generic;
using System.Linq;

namespace XLib.Core.Parsers.Internal {

	public class FilteredRow : IRawTableRow {

		private readonly Dictionary<string, string> _columnRemap = new(8, StringComparer.InvariantCultureIgnoreCase);
		private readonly List<string> _headers = new(8);
		private readonly IRawTableRow _row;

		public FilteredRow(IRawTableRow row, string prefix, bool removePrefix) {
			_row = row;

			var prefixLen = prefix.Length;
			foreach (var oldName in _row.Headers.Where(x => x.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))) {
				var newName = removePrefix ? oldName.Substring(prefixLen) : oldName;
				_columnRemap.Add(newName, oldName);
				_headers.Add(newName);
			}
		}

		public string Location => _row.Location;
		public IEnumerable<string> Headers => _headers;

		public bool HasColumnInHeader(string columnName) => _columnRemap.ContainsKey(columnName);

		public bool IsEmpty() {
			return _columnRemap.Values.All(x => _row.IsEmpty(x));
		}

		public bool IsEmpty(string columnName) => !_columnRemap.TryGetValue(columnName, out var realName) || _row.IsEmpty(realName);

		public string RawValue(string columnName) => _columnRemap.TryGetValue(columnName, out var realName) ? _row.RawValue(realName) : null;

		public bool RawValue(string columnName, out string value) {
			if (!_columnRemap.TryGetValue(columnName, out var realName)) {
				value = null;
				return false;
			}

			return _row.RawValue(realName, out value);
		}

		public Exception MakeError(string message) => _row.MakeError(message);

	}

}