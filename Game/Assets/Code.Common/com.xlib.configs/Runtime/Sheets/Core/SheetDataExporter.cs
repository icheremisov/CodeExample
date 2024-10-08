using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using XLib.Configs.Sheets.Contracts;
using XLib.Configs.Sheets.Converters;
using XLib.Configs.Sheets.Serialize;

namespace XLib.Configs.Sheets.Core {

	public class SheetDataExporter : ISheetDataExporter {
		private readonly ISheetPropertyFilter _propertyFilter;
		private readonly ISheetsConverterProvider _converterProvider;
		private readonly SheetTypeSchemaFactory _sheetTypeSchemaFactory;
		private readonly IExportObjectHandler _handler;

		private int _rowsCount;
		private Dictionary<string, SheetColumnValues> _valuesByPath;

		public SheetDataExporter(ISheetPropertyFilter propertyFilter, ISheetsConverterProvider converterProvider, SheetTypeSchemaFactory sheetTypeSchemaFactory,
			IExportObjectHandler handler) {
			_propertyFilter = propertyFilter;
			_converterProvider = converterProvider;
			_sheetTypeSchemaFactory = sheetTypeSchemaFactory;
			_handler = handler;
		}

		public IEnumerable<SheetColumnValues> GetValues<T>(IEnumerable<T> rows) {
			_rowsCount = 0;
			_valuesByPath = new Dictionary<string, SheetColumnValues>();
			var inlinePropsMap = new Dictionary<string, SheetColumnData>();

			foreach (var row in rows) {
				switch (row) {
					case ISheetRowsContainer rowsContainer: {
						foreach (var nestedRow in rowsContainer.SheetRows) {
							AddValues(GetValuesInternal(row, null));
							AddValues(GetValuesInternal(nestedRow, null), true);
							_rowsCount++;
						}

						break;
					}

					case ISheetRowsList rowsList: {
						throw new NotImplementedException();
						// var indexColumn = new SheetColumnData(new SheetKeyColumnAttribute(rowsList));
						//
						// foreach (var nestedRow in rowsList.SheetRows) {
						// 	AddValues(rowValues);
						// 	AddValues(GetValuesInternal(nestedRow, null)
						// 		.Append(new SheetRowColumnValue(indexColumn, ++i)), true);
						// 	_rowsCount++;
						// }
					}

					default: {
						var allValues = GetValuesInternal(row, null).ToArray();
						var keyValue = allValues.FirstOrDefault(value => value.Column.Property.IsKey);
						foreach (var valuesGroup in
								 allValues.GroupBy(value => $"{value.Column.RootInlineProperty}:{value.Column.InlineKeyValue}")
									 .OrderBy(group => group.FirstOrDefault().Column.Index)) {

							var column = valuesGroup.FirstOrDefault().Column;
							var prop = column.RootInlineProperty;
							if (string.IsNullOrEmpty(prop))
								AddValues(valuesGroup);
							else {
								if (!inlinePropsMap.TryGetValue(prop, out var indexColumn)) {
									var indexProperty = SheetRowProperty.Create<string>(prop, new ShtProtectedAttribute(), 
										new ShtBackgroundAttribute(0.8f));
									
									indexColumn = new SheetColumnData(indexProperty);
									indexColumn.Index.CopyFrom(keyValue.Column.Index);
									indexColumn.Index.Increment();
									inlinePropsMap[prop] = indexColumn;
								}

								AddValues(valuesGroup
									.Append(new SheetRowColumnValue(indexColumn, column.InlineKeyValue))
									.Append(keyValue));
							}

							_rowsCount++;
						}

						break;
					}
				}
			}

			return _valuesByPath.Values
				.GroupBy(values => values.NestedRow)
				.OrderBy(group => group.Key)
				.SelectMany(group => group.OrderBy(values => values.Column.Index));
		}

		private void AddValues(IEnumerable<SheetRowColumnValue> rowValues, bool nestedRow = false) {
			foreach (var columnValue in rowValues) {
				var column = columnValue.Column;

				var path = column.Path;
				if (!_valuesByPath.TryGetValue(path, out var values)) _valuesByPath.Add(path, values = new SheetColumnValues(column, nestedRow));

				var delta = _rowsCount - values.Count;
				while (delta > 0) {
					values.Add(null);
					delta--;
				}

				values.Add(columnValue.Value);
			}
		}

		private IEnumerable<SheetRowColumnValue> GetValuesInternal(object obj, SheetColumnData parent) {
			try {
				_handler?.OnBeforeExport(obj, parent?.Property);

				var properties = _sheetTypeSchemaFactory.GetProperties(obj);

				foreach (var property in properties.Properties) {
					if (!_propertyFilter.IsFilterProperty(property, obj, parent?.Property)) continue;

					var value = property.GetValue(obj);
					if (value == null && !property.PreserveEmptyElements) continue;

					if (property.IsEnumerable && value != null) {
						var i = 0;
						foreach (var item in (IEnumerable)value) {
							foreach (var columnValue in GetValues(property, item, parent, i)) yield return columnValue;
							i++;
						}

						continue;
					}

					foreach (var columnValue in GetValues(property, value, parent, null)) yield return columnValue;
				}
			}
			finally {
				_handler?.OnAfterExport(obj, parent?.Property);
			}
		}

		private IEnumerable<SheetRowColumnValue> GetValues(SheetRowProperty property, object value, SheetColumnData parent, int? index) {
			var type = property.ElementType ?? property.Type;
			if (_converterProvider.IsSkipAble(type)) {
				yield break;
			}

			if (!_converterProvider.IsSimple(type)) {
				var column = new SheetColumnData(property, parent, index);
				if (value == null) yield break;
				foreach (var columnValue in GetValuesInternal(value, column)) yield return columnValue;
			}
			else {
				type = Nullable.GetUnderlyingType(type) ?? type;

				var converter = _converterProvider.GetConverter(type);
				if (converter != null) {
					type = converter.ToType;
					value = converter.To(value, type);
					var values = converter.GetValues(property);
					if (values != null) {
						property.SetValidationRule(values);
					}
				}

				if (type.IsEnum) value = value.ToString();

				yield return new SheetRowColumnValue(new SheetColumnData(property, parent, index), value);
			}
		}

		private readonly struct SheetRowColumnValue {
			public readonly object Value;
			public readonly SheetColumnData Column;

			public SheetRowColumnValue(SheetColumnData column, object value) : this() {
				if (value is string s && s.Length > 0) {
					if (IsFormula(s[0])) value = $"'{s}";
				}

				Value = value;
				Column = column;
			}

			private bool IsFormula(char c) => c is '=' or '+';

			//for debug
			public override string ToString() => $"{Column.Path} = {Value}";
		}
	}

}