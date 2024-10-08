using System.Collections;
using System.Collections.Generic;

namespace XLib.Configs.Sheets.Core {

	public class SheetColumnValues : ICollection<object> {
		private List<object> _values = new List<object>();

		public readonly bool NestedRow;
		public readonly SheetColumnData Column;
		public int Count => _values.Count;
		bool ICollection<object>.IsReadOnly => false;

		public SheetColumnValues(SheetColumnData column, bool nestedRow = false) {
			Column = column;
			NestedRow = nestedRow;
		}

		public bool ValueIsFormula(int index) => _values[index].ToString().StartsWith("=");

		public void Add(object value) => _values.Add(value);
		public void Clear() => _values.Clear();
		public bool Contains(object item) => _values.Contains(item);
		public bool Remove(object item) => _values.Remove(item);
		public void CopyTo(object[] array, int arrayIndex) {
			_values.CopyTo(array, arrayIndex);
			TrimEmpty();
		}

		public void AddRange(IEnumerable<object> collection) => _values.AddRange(collection);

		public void RemoveAt(int index) => _values.RemoveAt(index);

		public void Replace(IEnumerable<object> collection) {
			_values.Clear();
			_values.AddRange(collection);
			TrimEmpty();
		}

		private void TrimEmpty() {
			for (var i = _values.Count - 1; i >= 0; i--) {
				if (_values[i] == null)
					_values.RemoveAt(i);
				else
					break;
			}
		}

		public object this[int index] { get => _values[index]; set => _values[index] = value; }

		public SheetColumnValues Trim(int rowCount) {
			var delta = Count - rowCount;
			if (delta > 0) _values.RemoveRange(rowCount, delta);
			return this;
		}

		public IEnumerator<object> GetEnumerator() => _values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		//for debug
		public override string ToString() => $"{Column.Path} = {Count} values";

		public void CropEmpty() {
			for (var i = _values.Count-1; i >= 0; --i) {
				var value = _values[i];
				if ((value is bool b && !b) || ((value is string s && string.IsNullOrWhiteSpace(s)) || value.ToString() == "0"))
					_values.RemoveAt(i);
				else
					return;
			}
		}
	}

}