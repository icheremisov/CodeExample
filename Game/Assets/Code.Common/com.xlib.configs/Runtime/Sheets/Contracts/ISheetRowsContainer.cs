using System.Collections.Generic;

namespace XLib.Configs.Sheets.Contracts
{
    public interface ISheetRowsContainer
    {
        IEnumerable<object> SheetRows { get; }
    }
}