using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Configs.Sheets.Core {

	[Serializable]
	public partial class SheetFormatSettings {
		public Color HeaderColor = new(0.858f, 0.858f, 0.858f);
		public Color ProtectedColor = new(0.972f, 0.972f, 0.972f);
		public int? SortByColumnIndex = null;
		public bool WithEnumColors = false;
	}

	[HideMonoScript]
	public partial class SheetsSettings : ScriptableObject {
#pragma warning disable CS0169

		private string _title;
		[SerializeField] protected SheetFormatSettings _formatSettings;
		public SheetFormatSettings FormatSettings => _formatSettings;
		[SerializeField] protected bool _skipClientTypes;
		public bool SkipClientTypes => _skipClientTypes;

#pragma warning restore CS0169
	}
}