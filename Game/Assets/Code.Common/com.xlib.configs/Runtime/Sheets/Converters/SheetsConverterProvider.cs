using System;
using System.Collections.Generic;
using XLib.Configs.Sheets.Contracts;
using XLib.Core.Reflection;
using XLib.Core.Utils;

namespace XLib.Configs.Sheets.Converters {

	public interface ISheetsConverterProvider {
		ISheetsConverter GetConverter(Type targetType);
		bool IsSimple(Type type);
		public bool IsSkipAble(Type type);
	}

	public class SheetsConverterProvider : ISheetsConverterProvider {
		private readonly bool _skipClientTypes;
		private Dictionary<Type, ISheetsConverter> _converterByType;

		public SheetsConverterProvider(bool skipClientTypes) {
			_skipClientTypes = skipClientTypes;
		}

		private void LazyInitialize() {
			_converterByType = new Dictionary<Type, ISheetsConverter>();

			foreach (var type in TypeCache<ISheetsConverter>.CachedTypes) {
				if (type.IsGenericType) continue;
				var converter = (ISheetsConverter)Activator.CreateInstance(type);

				foreach (var attribute in type.GetAttributes<SheetsConverterAttribute>()) _converterByType.Add(attribute.Type, converter);
			}
		}

		public ISheetsConverter GetConverter(Type targetType) {
			if (_converterByType == null) LazyInitialize();

			while (targetType != null) {
				if (_converterByType.TryGetValue(targetType, out var converter)) return converter;
				targetType = targetType.BaseType;
			}

			return null;
		}

		public bool IsSimple(Type type) {
			type = Nullable.GetUnderlyingType(type) ?? type;

			var converter = GetConverter(type);
			if (converter != null) type = converter.ToType;

			return type.IsPrimitive || type.IsEnum || type == typeof(string);
		}

		public bool IsSkipAble(Type type) {
			var converter = GetConverter(type);
			return _skipClientTypes && converter != null &&
				((SheetsConverterAttribute)Attribute.GetCustomAttribute(converter.GetType(), TypeOf<SheetsConverterAttribute>.Raw)).ExportCanBeSkipped;
		}
	}

}