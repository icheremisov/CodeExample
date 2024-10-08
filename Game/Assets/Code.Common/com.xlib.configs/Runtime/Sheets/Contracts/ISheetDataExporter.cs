using System.Collections.Generic;
using XLib.Configs.Sheets.Core;

namespace XLib.Configs.Sheets.Contracts {

	public interface IExportObjectHandler {
		void OnBeforeExport(object obj, SheetRowProperty property);
		void OnAfterExport(object obj, SheetRowProperty property);
	}

	public interface ISheetDataExporter {
		IEnumerable<SheetColumnValues> GetValues<T>(IEnumerable<T> rows);
	}

}