using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XLib.Configs.Sheets.Core {

	public class SheetColumnData {
		private const string HeaderPattern = @"^(?<extra>!)?\s*(?<name>\w+(\s\w+)*)\s*(#(?<index>\d+))?\s*(\((?<description>.*)\))?$";
		public const string HeaderRangeDescription = "Header";

		public readonly string Header;
		public readonly string Path;
		public readonly SheetColumnData Parent;
		public readonly SheetColumnIndex Index;
		public readonly bool IsExtra;
		public bool IsInline => !InlineProperty.IsNullOrEmpty();
		public readonly string InlineProperty;
		public string RootInlineProperty => IsInline ? InlineProperty : Parent?.RootInlineProperty;
		public string InlineKeyValue => Parent != null ? (Parent.IsInline ? Parent.Path : Parent.InlineKeyValue) : string.Empty;

		public SheetColumnData Top => Parent?.Top ?? this;

		public SheetRowProperty Property { get; set; }

		public SheetColumnData(SheetRowProperty property, SheetColumnData parent = null, int? index = null)
			: this(property.Name, property.Description, parent, index, false) {
			Property = property;
			InlineProperty = property.InlineProperty;
			Index = new SheetColumnIndex(property.Index, index, parent?.Index);
		}

		private SheetColumnData(string name, string description, SheetColumnData parent, int? index, bool isExtra) {
			Parent = parent;
			IsExtra = isExtra;
			Header = FormatHeader(name, description, index, isExtra);

			var pathHeader = FormatHeader(name, null, index, isExtra); //no description in path!
			if (Parent != null) {
				Path = Parent.IsInline ? pathHeader : $"{Parent.Path}.{pathHeader}";
			} else {
				Path = pathHeader;
			}
		}

		public static SheetColumnData Parse(string header, SheetColumnData parent = null) {
			ParseHeader(header, out var name, out var description, out var index, out var isExtra);
			return new SheetColumnData(name, description, parent, index, isExtra);
		}

		public static void ParseHeader(string header, out string name, out string description, out int? index, out bool isExtra) {
			var match = new Regex(HeaderPattern).Match(header);

			var nameGroup = match.Groups["name"];
			var indexGroup = match.Groups["index"];
			var extraGroup = match.Groups["extra"];
			var descriptionGroup = match.Groups["description"];

			if (!nameGroup.Success) throw new Exception($"Can't parse header {header}");

			name = nameGroup.Value;
			index = indexGroup.Success ? (int?)int.Parse(indexGroup.Value) - 1 : null;
			isExtra = extraGroup.Success;
			description = descriptionGroup.Success ? descriptionGroup.Value : null;
		}

		private string FormatHeader(string name, string description, int? index, bool isExtra) {
			var header = index.HasValue ? $"{name} #{index + 1}" : name;
			if (!string.IsNullOrEmpty(description)) header = $"{header} ({description})";
			return isExtra ? $"! {header}" : header;
		}
	}

	public class SheetColumnIndex : IComparable<SheetColumnIndex> {
		private int _propIndex;
		private int _additionalIndex;
		private int? _arrayIndex;
		private SheetColumnIndex _parentIndex;

		public List<SheetColumnIndex> Hierarchy { get; }

		public int? ArrayIndex => _arrayIndex;

		public SheetColumnIndex(int propIndex, int? arrayIndex = null, SheetColumnIndex parentIndex = null) {
			_propIndex = propIndex;
			_arrayIndex = arrayIndex;
			_parentIndex = parentIndex;

			Hierarchy = new List<SheetColumnIndex> { this };
			while (parentIndex != null) {
				Hierarchy.Add(parentIndex);
				parentIndex = parentIndex._parentIndex;
			}

			Hierarchy.Reverse();
		}

		public void CopyFrom(SheetColumnIndex other) {
			_parentIndex = other._parentIndex;
			_propIndex = other._propIndex;
			_arrayIndex = other._arrayIndex;
			_additionalIndex = other._additionalIndex;
		}

		public void Increment() => ++_additionalIndex;
		public void IncrementArrayIndex() => ++_arrayIndex;

		public int CompareTo(SheetColumnIndex other) {
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;

			var result = 0;
			for (var i = 0; i < Hierarchy.Count; i++) {
				if (other.Hierarchy.Count <= i) break;

				var x = Hierarchy[i];
				var y = other.Hierarchy[i];

				result = x._propIndex.CompareTo(y._propIndex);
				if (result == 0) result = x._additionalIndex.CompareTo(y._additionalIndex);
				if (result == 0) result = Nullable.Compare(x._arrayIndex, y._arrayIndex);

				if (result != 0) break;
			}

			return result;
		}

		public override string ToString() {
			var value = _propIndex.ToString();
			if (_arrayIndex.HasValue) value += $" ({_arrayIndex})";
			return value;
		}
	}

}