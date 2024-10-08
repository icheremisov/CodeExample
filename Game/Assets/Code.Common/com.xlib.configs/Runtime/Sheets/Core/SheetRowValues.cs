using System.Collections.Generic;

namespace XLib.Configs.Sheets.Core {

	public class SheetRowPropertyValue {
		public List<string> Values;
		public List<SheetRowValues> Items;

		private readonly string _name;
		public string Name => _name;

		public SheetRowPropertyValue(string name) => _name = name;

		//for debug
		public override string ToString() => $"{_name} = {(Items != null ? $"{Items.Count} Items" : Values.JoinToString(", "))}";
	}
	
	public class SheetRowValues : Dictionary<string, SheetRowPropertyValue> { }

}