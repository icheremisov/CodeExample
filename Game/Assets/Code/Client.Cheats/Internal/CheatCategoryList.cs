namespace Client.Cheats.Internal {

	internal struct CheatCategoryList {
		public CheatCategoryList(string[] names, CheatPluginData[] plugins) {
			Names = names;
			Plugins = plugins;
		}

		public string[] Names { get; set; }
		public CheatPluginData[] Plugins { get; set; }
	}

}