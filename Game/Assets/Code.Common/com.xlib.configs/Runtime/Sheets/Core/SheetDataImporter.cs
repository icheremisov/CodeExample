using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Sheets.Contracts;
using XLib.Configs.Sheets.Converters;
using XLib.Configs.Sheets.Serialize;
using XLib.Configs.Utils;
using XLib.Core.Collections;
using XLib.Core.Utils;
using Object = UnityEngine.Object;

namespace XLib.Configs.Sheets.Core {

	public class SheetDataImporter : ISheetDataImporter {
		private readonly ISheetsConverterProvider _converterProvider;
		private readonly SheetTypeSchemaFactory _sheetTypeSchemaFactory;
		private readonly IImportObjectHandler _handler;

		public SheetDataImporter(ISheetsConverterProvider converterProvider, SheetTypeSchemaFactory sheetTypeSchemaFactory, IImportObjectHandler handler) {
			_converterProvider = converterProvider;
			_sheetTypeSchemaFactory = sheetTypeSchemaFactory;
			_handler = handler;
		}

		public IEnumerable<T> CreateRows<T>(IEnumerable<SheetRowValues> values) => (IEnumerable<T>)CreateArray(values.ToList(), typeof(T), null, null);

		public void SetValues<T>(IEnumerable<T> rows, IEnumerable<SheetRowValues> values) {
			foreach (var row in rows) {
				var properties = _sheetTypeSchemaFactory.GetProperties(row);
				var keyProperty = properties.KeyProperty;
				if (keyProperty == null) throw new Exception($"No key property for row {row}");

				var keyValue = keyProperty.GetValue(row);

				var rowValues = values.Where(item => ContainsEqualValue(item, keyProperty, keyValue)).ToArray();
				if (rowValues.Length == 0) {
					if (keyValue is ItemId itemId) keyValue = itemId.ToKeyString();
					Debug.LogWarning($"No values for row with key {keyProperty.Name} = {keyValue}");
					continue;
				}

				foreach (var rowValue in rowValues) SetValues(row, rowValue, null, false);

				switch (row) {
					case ISheetRowsContainer rowsContainer:
						SetValues(rowsContainer.SheetRows, rowValues);
						break;

					case ISheetRowsList rowsList:
						SetValues(rowsList, rowValues);
						break;
				}
			}
		}

		public IEnumerable<T> SetValues<T>(IEnumerable<SheetRowValues> values, Func<object, int, T> select) {
			var properties = _sheetTypeSchemaFactory.GetProperties<T>();
			var keyProperty = properties.KeyProperty;
			if (keyProperty == null) throw new Exception($"No key property for row {TypeOf<T>.Name}");

			var row = 2;
			foreach (var value in values) {
				var obj = select(GetValue(value, keyProperty), row);
				SetValues(obj, value, null, false);
				++row;
				yield return obj;
			}
		}

		private void SetValues(ISheetRowsList rowsList, IEnumerable<SheetRowValues> values) {
			var propertyType = typeof(int);
			var propertyName = rowsList.SheetRowsIndexName;
			var orderedValues = values
				.Where(item => item.TryGetValue(propertyName, out var value) && value.Values.Count > 0)
				.OrderBy(item => (int)ConvertValue(item[propertyName].Values[0], propertyType))
				.ToArray();

			if (orderedValues.Length > 0) rowsList.SheetRows = (IEnumerable<object>)CreateOrModifyArray(rowsList.SheetRows, orderedValues, rowsList.SheetRowsType, null);
		}

		public void SetValues(object obj, SheetRowValues values, SheetRowProperty property = null, bool strong = true) {
			try {
				_handler?.OnBeforeImport(obj, property);
				var properties = _sheetTypeSchemaFactory.GetProperties(obj);
				var inlineProps = new Dictionary<string, (SheetRowProperty, object)>();

				foreach (var rowProperty in properties.Properties) {
					var inlineProperty = rowProperty.InlineProperty;
					if (string.IsNullOrEmpty(inlineProperty)) continue;
					if (!values.ContainsKey(inlineProperty)) continue;

					var propValue = values[inlineProperty].Values.FirstOrDefault();

					if (string.IsNullOrEmpty(propValue)) continue;
					var findProperty = properties.FindProperty(propValue);
					var propertyValue = findProperty?.GetValue(obj);
					if (propertyValue == null) continue;

					var schema = _sheetTypeSchemaFactory.GetProperties(propertyValue);
					foreach (var sheetRowProperty in schema.Properties) inlineProps[sheetRowProperty.Name] = (sheetRowProperty, propertyValue);
				}

				foreach ((var name, var propertyValue) in values) {
					SheetRowProperty prop;
					object curObject;
					if (inlineProps.TryGetValue(name, out var inlineProp)) {
						(prop, curObject) = inlineProp;
					}
					else {
						prop = properties.FindProperty(name);
						curObject = obj;
					}

					if (prop is not { CanWrite: true }) {
						if (strong) {
							if (prop == null || !prop.OnlyWrite && propertyValue.Values != null && propertyValue.Values.Any(s => !string.IsNullOrEmpty(s) && s != "FALSE")) {
								Debug.LogError($"Can't set value {propertyValue.Values.JoinToString()} for property {name} {obj} {obj.GetType().Name}", obj as Object);
							}
						}

						continue;
					}

					var value = GetValue(prop, propertyValue, curObject);
					if (value == null) continue;

					try {
						prop.SetValue(curObject, value);
					}
					catch (NotImplementedException) {
						throw;
					}
					catch (Exception ex) {
						Debug.LogError($"Can't set value {value} for property {name} of {obj} {obj.GetType().Name}", obj as Object);
						// var value1 = GetValue(prop, propertyValue);

						Debug.LogException(ex);
						// throw;
					}
				}
			}
			finally {
				_handler?.OnAfterImport(obj, property);
			}
		}

		private object GetValue(SheetRowProperty property, SheetRowPropertyValue propertyValue, object curObject) {
			var type = property.IsEnumerable ? property.ElementType : property.Type;

			if (!_converterProvider.IsSimple(type)) {
				var items = propertyValue.Items;
				if (!property.IsEnumerable) return items.Count != 0 ? CreateInstance(items[0], type, property) : null;
				if (!property.PreserveEmptyElements) items = items.Where(item => item != null).ToList();
				return CreateArray(items, type, property, curObject);
			}

			var values = propertyValue.Values;
			if (property.IsEnumerable) return CreateArray(values, type, property, curObject);

			return values.Count != 0 ? ConvertValue(values[0], type) : null;
		}

		private IEnumerable CreateArray(IList<string> values, Type type, SheetRowProperty property, object curObject) {
			var arrayType = property.Type;
			if (arrayType.IsArray) {
				if (property.SkipFirstCount == 0) {
					var array = Array.CreateInstance(type, values.Count);
					for (var i = 0; i < values.Count; i++) array.SetValue(ConvertValue(values[i], type), i);
					return array;
				}
				else {
					var skipArr = Array.CreateInstance(type, property.SkipFirstCount);
					var skipArrEnumerator = ((IEnumerable)property.GetValue(curObject)).GetEnumerator();
					var skipLength = 0;
					while (skipArrEnumerator.MoveNext() && skipLength < property.SkipFirstCount) {
						skipArr.SetValue(skipArrEnumerator.Current, skipLength);
						skipLength++;
					}

					var array = Array.CreateInstance(type, skipLength + values.Count);
					for (var i = 0; i < skipLength; i++) array.SetValue(skipArr.GetValue(i), i);
					for (var i = 0; i < values.Count; i++) array.SetValue(ConvertValue(values[i], type), i + skipLength);
					return array;
				}
			}

			if (arrayType.GetGenericTypeDefinition() != typeof(List<>)) throw new NotImplementedException($"Property {property.Name} has unsupport type {property.Type.Name}");

			if (Activator.CreateInstance(arrayType) is not IList list) return null;
			if (property.SkipFirstCount != 0) {
				var skipArrEnumerator = ((IEnumerable)property.GetValue(curObject)).GetEnumerator();
				var skipLength = 0;
				while (skipArrEnumerator.MoveNext() && skipLength < property.SkipFirstCount) {
					list.Add(skipArrEnumerator.Current);
					skipLength++;
				}
			}

			foreach (var t in values) list.Add(ConvertValue(t, type));
			return list;
		}

		private interface IPair {
			public object KeyO { get; }
			public object ValueO { get; }
		}

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class Pair<TKey, TValue> : IPair {
			public TKey Key { get; set; }
			public TValue Value { get; set; }
			public object KeyO => Key;
			public object ValueO => Value;
		}

		private IEnumerable CreateArray(IList<SheetRowValues> values, Type type, SheetRowProperty property, object curObject) {
			var arrayType = property.Type;
			if (arrayType.IsArray) {
				if (property.SkipFirstCount == 0) {
					var array = Array.CreateInstance(type, values.Count);
					for (var i = 0; i < values.Count; i++) array.SetValue(CreateInstance(values[i], type, property), i);
					return array;
				}
				else {
					var skipArr = Array.CreateInstance(type, property.SkipFirstCount);
					var skipArrEnumerator = ((IEnumerable)property.GetValue(curObject)).GetEnumerator();
					var skipLength = 0;
					while (skipArrEnumerator.MoveNext() && skipLength < property.SkipFirstCount) {
						skipArr.SetValue(skipArrEnumerator.Current, skipLength);
						skipLength++;
					}

					var array = Array.CreateInstance(type, skipLength + values.Count);
					for (var i = 0; i < skipLength; i++) array.SetValue(skipArr.GetValue(i), i);
					for (var i = 0; i < values.Count; i++) array.SetValue(CreateInstance(values[i], type, property), i + skipLength);
					return array;
				}
			}

			var genericTypeDefinition = arrayType.GetGenericTypeDefinition();
			if (genericTypeDefinition == typeof(List<>)) {
				var list = Activator.CreateInstance(arrayType) as IList;
				if (list == null) return null;
				if (property.SkipFirstCount != 0) {
					var skipArrEnumerator = ((IEnumerable)property.GetValue(curObject)).GetEnumerator();
					var skipLength = 0;
					while (skipArrEnumerator.MoveNext() && skipLength < property.SkipFirstCount) {
						list.Add(skipArrEnumerator.Current);
						skipLength++;
					}
				}

				foreach (var t in values) list.Add(CreateInstance(t, type, property));
				return list;
			}

			if (genericTypeDefinition != typeof(Dictionary<,>) && genericTypeDefinition != typeof(OrderedDictionary<,>))
				throw new NotImplementedException($"Property {property.Name} has unsupport type {property.Type.Name}");

			var dictionary = Activator.CreateInstance(arrayType) as IDictionary;
			if (dictionary == null) return null;
			var genericType = typeof(Pair<,>).MakeGenericType(type.GetGenericArguments());
			foreach (var t in values) {
				var pair = (IPair)CreateInstance(t, genericType, property);
				dictionary.Add(pair.KeyO, pair.ValueO);
			}

			return dictionary;
		}

		private IEnumerable CreateOrModifyArray(IEnumerable<object> sheetRows, IList<SheetRowValues> values, Type type, SheetRowProperty property) {
			var oldArray = sheetRows.ToArray();
			var array = Array.CreateInstance(type, values.Count);

			for (var i = 0; i < values.Count; i++) {
				if (i < oldArray.Length) {
					SetValues(oldArray[i], values[i], property);
					array.SetValue(oldArray[i], i);
				}
				else {
					array.SetValue(CreateInstance(values[i], type, property), i);
				}
			}

			return array;
		}

		private object CreateInstance(SheetRowValues values, Type type, SheetRowProperty property) {
			if (values == null) return null;

			var instance = Activator.CreateInstance(type);
			SetValues(instance, values, property);

			return instance;
		}

		private object ConvertValue(string value, Type type) {
			try {
				type = Nullable.GetUnderlyingType(type) ?? type;

				var converter = _converterProvider.GetConverter(type);
				if (converter != null) return converter.From(value, type);

				if (type.IsEnum) return Enum.Parse(type, value);
				if (type == TypeOf<string>.Raw) return value;

				if (
					type == TypeOf<long>.Raw ||
					type == TypeOf<ulong>.Raw ||
					type == TypeOf<int>.Raw ||
					type == TypeOf<uint>.Raw ||
					type == TypeOf<short>.Raw ||
					type == TypeOf<ushort>.Raw ||
					type == TypeOf<float>.Raw ||
					type == TypeOf<double>.Raw
				) // parse any format 123 1232,00 
				{
					if (value.Contains('%')) {
						value = value.ReplaceAll('%');
						try {
							var v = Convert.ChangeType(value, type);
							return Convert.ChangeType((double)v * 0.01, type);
						}
						catch {
							// ignore
						}

						return Convert.ChangeType(double.Parse(value) * 0.01, type);
					}

					try {
						return Convert.ChangeType(value, type);
					}
					catch (Exception) {
						// ignore
					}

					return Convert.ChangeType(double.Parse(value), type);
				}

				return Convert.ChangeType(value, type);
			}
			catch (Exception) {
				Debug.LogError($"Can't convert {value} to {type}");
				throw;
			}
		}

		private bool ContainsEqualValue(SheetRowValues values, SheetRowProperty property, object value) {
			foreach (var pair in values) {
				var name = ConfigUtils.PrettyName(pair.Key);
				if (property.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
					var propertyValue = pair.Value;
					return propertyValue.Values.Count != 0 &&
						ConvertValue(propertyValue.Values[0], property.Type).Equals(value);
				}
			}

			return false;
		}

		private object GetValue(SheetRowValues values, SheetRowProperty property) {
			foreach (var pair in values) {
				var name = ConfigUtils.PrettyName(pair.Key);
				if (!property.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) continue;

				var propertyValue = pair.Value;
				return propertyValue.Values.Count != 0 ? ConvertValue(propertyValue.Values[0], property.Type) : default;
			}

			return default;
		}
	}

}