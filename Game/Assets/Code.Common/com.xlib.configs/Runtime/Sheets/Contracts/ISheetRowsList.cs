using System;
using System.Collections.Generic;

namespace XLib.Configs.Sheets.Contracts
{
    public interface ISheetRowsList
    {
        IEnumerable<object> SheetRows { get; set; }
        
        string SheetRowsIndexName { get; }
        
        Type SheetRowsType { get; }
    }
}