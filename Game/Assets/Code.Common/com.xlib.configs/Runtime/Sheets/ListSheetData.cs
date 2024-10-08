using System.Collections.Generic;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Sheets.Core;

namespace XLib.Configs.Sheets {

	public abstract class ListSheetData<T> : SheetData<T> where T : Object, IOrderBy, new() {
		public override void Import(IEnumerable<SheetRowValues> values, SheetContext context) {
			var importer = context.CreateImporter(ImportObjectHandler);
			SetRows(importer.CreateRows<T>(values));
		}

		protected abstract void SetRows(IEnumerable<T> rows);
	}

}