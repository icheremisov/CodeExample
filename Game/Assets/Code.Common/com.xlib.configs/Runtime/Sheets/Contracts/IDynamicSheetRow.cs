using System.Collections.Generic;
using XLib.Configs.Sheets.Core;

namespace XLib.Configs.Sheets.Contracts {

	public interface IDynamicSheetRow {
		IEnumerable<SheetRowProperty> GetColumns();

		object GetValue(string columnName);

		void SetValue(string columnName, object value);
	}

}