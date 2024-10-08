using System;
using System.Collections.Generic;
using XLib.Configs.Sheets.Core;

namespace XLib.Configs.Sheets.Contracts {

	public interface ISheetsConverter {
		Type ToType { get; }

		object To(object obj, Type type);
		object From(string value, Type type);
		public IEnumerable<object> GetValues(SheetRowProperty property);
	}

}