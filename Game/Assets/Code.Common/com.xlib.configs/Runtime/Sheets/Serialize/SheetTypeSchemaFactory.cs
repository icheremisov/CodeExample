using System;
using System.Collections.Generic;
using XLib.Configs.Sheets.Contracts;
using XLib.Core.Utils;

namespace XLib.Configs.Sheets.Serialize {

	public class SheetTypeSchemaFactory {
		private Dictionary<Type, ISheetTypeSchema> _propertiesByType = new();
		public ISheetTypeSchema GetProperties(object obj) {
			var type = obj.GetType();
			if (!_propertiesByType.TryGetValue(type, out var properties)) _propertiesByType[type] = properties = new SheetTypeSchema(type);
			return obj is IDynamicSheetRow dynamicRow ? new DynamicSheetTypeRowProperties(dynamicRow, properties) : properties;
		}
		public ISheetTypeSchema GetProperties<T>() {
			var type = TypeOf<T>.Raw;
			if (!_propertiesByType.TryGetValue(type, out var properties)) _propertiesByType[type] = properties = new SheetTypeSchema(type);
			return properties;
		}
	}

}