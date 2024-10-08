using System.Collections.Generic;
using XLib.Configs.Sheets.Core;

namespace XLib.Configs.Sheets {

	public interface ISheetData {
		string Title { get; }
		DirectionType Direction { get; }
		public bool WithFilter { get; }
		public SheetFormatSettings Settings { get; }
		bool CheckBranch { get; }
		IEnumerable<SheetColumnValues> Export(SheetContext context);
		void Import(IEnumerable<SheetRowValues> values, SheetContext context);
	}

}