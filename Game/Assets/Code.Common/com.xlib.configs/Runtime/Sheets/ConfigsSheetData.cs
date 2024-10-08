using XLib.Configs.Contracts;
using XLib.Unity.Utils;

namespace XLib.Configs.Sheets {

	public abstract class ConfigsSheetData<T> : SheetData<T> where T : GameItemOrComponent, IOrderBy {
		protected override void PostImport(T[] rows) => rows.ForEach(row => row.SetObjectDirty());
	}

}