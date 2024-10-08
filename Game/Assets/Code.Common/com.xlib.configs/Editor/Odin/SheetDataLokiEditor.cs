using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using XLib.Configs.Platforms;
using XLib.Configs.Sheets;
using XLib.Unity.Extensions;

namespace XLib.Configs.Odin {

	[UsedImplicitly]
	internal class SheetDataLokiEditor : LokiEditor<SheetData> {
		private class Properties : LokiProperties {
			private SheetData Self => ValueEntry.SmartValue;

			public override void ProcessMemberProperties(List<InspectorPropertyInfo> properties) {
				properties.AddMethod(Horizontal);
				properties.AddMethod(Vertical);
				properties.AddMethod(ImportSheet);
				properties.AddMethod(ExportSheet);
				properties.AddMethod(SelectAssets);
			}

			[HorizontalGroup(30), PropertyTooltip("Enable horizontal table mode"), Button("", ButtonSizes.Small, Icon = SdfIconType.BoxArrowRight),
			 ShowIf("@Direction != DirectionType.Horizontal"), GUIColor(1, 0.8f, 0.4f)]
			private void Horizontal() => Self.Direction = DirectionType.Horizontal;

			[HorizontalGroup(30), PropertyTooltip("Enable vertical table mode"), Button("", ButtonSizes.Small, Icon = SdfIconType.BoxArrowDown),
			 ShowIf("@Direction != DirectionType.Vertical"), GUIColor(1, 0.8f, 0.4f)]
			private void Vertical() => Self.Direction = DirectionType.Vertical;

			[HorizontalGroup(30), PropertyTooltip("Download from Google"), Button("", ButtonSizes.Small, Icon = SdfIconType.Download), GUIColor(0.4f, 0.8f, 1)]
			private void ImportSheet() => SheetsImport.Import(Self.Owner.SheetId.ToString(), Self);

			[HorizontalGroup(30), PropertyTooltip("Upload to Google"), Button("", ButtonSizes.Small, Icon = SdfIconType.Upload), GUIColor(0f, 1f, 0.4f)]
			private void ExportSheet() => SheetsExport.Export(Self.Owner.SheetId.ToString(), Self.Owner.SkipClientTypes, SheetsExport.SheetExportMode.Merge, Self);

			[HorizontalGroup(30), PropertyTooltip("List of assets"), Button("", ButtonSizes.Small, Icon = SdfIconType.ChevronDoubleDown), GUIColor(0.57f, 0.99f, 1f)]
			private void SelectAssets() => Self.SelectAssets();
		}
	}

}