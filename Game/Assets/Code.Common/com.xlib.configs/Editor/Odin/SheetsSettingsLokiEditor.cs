using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using XLib.Configs.Platforms;
using XLib.Configs.Sheets;
using XLib.Configs.Sheets.Core;
using XLib.Unity.Extensions;

namespace XLib.Configs.Odin {

	[UsedImplicitly]
	internal class SheetsSettingsLokiEditor : LokiEditor<SheetsSettings> {
		private class Properties : LokiProperties {
			public override void ProcessMemberProperties(List<InspectorPropertyInfo> props) {
				props.AddMethod(ImportAll);
				props.AddMethod(ExportAll);
			}

			[EnableIf("@SheetId.IsEnable")]
			[Button("Download from Google", ButtonSizes.Large, Icon = SdfIconType.Download), ButtonGroup("Action", -1), GUIColor(0.4f, 0.8f, 1)]
			private void ImportAll() {
				var config = ValueEntry.SmartValue;
				SheetsImport.Import(config.SheetId.ToString(), config.Sheets.OfType<ISheetData>().ToArray());
			}

			[EnableIf("@SheetId.IsEnable")]
			[Button("Upload to Google", ButtonSizes.Large, Icon = SdfIconType.Upload), ButtonGroup("Action", -1), GUIColor(0f, 1f, 0.4f)]
			private void ExportAll() {
				var config = ValueEntry.SmartValue;
				SheetsExport.Export(config.SheetId.ToString(), config.SkipClientTypes, SheetsExport.SheetExportMode.Merge, config.Sheets.OfType<ISheetData>().ToArray());
			}
		}
	}

}