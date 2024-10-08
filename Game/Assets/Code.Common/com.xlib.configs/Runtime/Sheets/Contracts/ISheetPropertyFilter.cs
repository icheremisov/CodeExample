using XLib.Configs.Sheets.Core;

namespace XLib.Configs.Sheets.Contracts {

	public interface ISheetPropertyFilter {
		public bool IsFilterProperty(SheetRowProperty property, object obj, SheetRowProperty parent);
	}

}