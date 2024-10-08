using XLib.Configs.Sheets.Contracts;
using XLib.Configs.Sheets.Converters;
using XLib.Configs.Sheets.Serialize;

namespace XLib.Configs.Sheets.Core {

	public class SheetContext {
		private readonly bool _skipClientTypes = false;
		private readonly object _sheet;
		
		public SheetContext(object sheet, bool skipClientTypes) {
			_sheet = sheet;
			_skipClientTypes = skipClientTypes;
		}
		public object Sheet => _sheet;

		private ISheetsConverterProvider _converterProviderInternal;
		private ISheetsConverterProvider _converterProvider => _converterProviderInternal ??= new SheetsConverterProvider(_skipClientTypes);
		private SheetTypeSchemaFactory _schemaFactory = new();
		public ISheetDataExporter CreateExporter(ISheetPropertyFilter propertyFilter, IExportObjectHandler handler = null) => new SheetDataExporter(propertyFilter, _converterProvider, _schemaFactory, handler);
		public ISheetDataImporter CreateImporter(IImportObjectHandler handler = null) => new SheetDataImporter(_converterProvider, _schemaFactory, handler);
	}

}