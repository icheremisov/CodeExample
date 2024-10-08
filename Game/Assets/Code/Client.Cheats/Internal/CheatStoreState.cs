using System.Collections.Generic;
using UnityEngine;

namespace Client.Cheats.Internal {

	public class CheatStoreState {
		public Vector2 ScrollPosition { get; set; }
		public CheatPluginData SelectedPlugin { get; set; }
		public List<CheatPluginData> FilteredPluginList { get; set; }
		public string SearchQuery { get; set; }
		public int Category { get; set; }
		public object Args { get; set; }
	}

}