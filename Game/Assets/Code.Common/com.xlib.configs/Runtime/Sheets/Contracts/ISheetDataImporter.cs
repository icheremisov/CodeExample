using System;
using System.Collections.Generic;
using XLib.Configs.Sheets.Core;

namespace XLib.Configs.Sheets.Contracts {

	public interface IImportObjectHandler {
		void OnBeforeImport(object obj, SheetRowProperty property);
		void OnAfterImport(object obj, SheetRowProperty property);
	}
	
	public interface ISheetDataImporter {
		void SetValues<T>(IEnumerable<T> rows, IEnumerable<SheetRowValues> values);
		
		IEnumerable<T> SetValues<T>(IEnumerable<SheetRowValues> values, Func<object, int, T> select);

		IEnumerable<T> CreateRows<T>(IEnumerable<SheetRowValues> values);
	}

}