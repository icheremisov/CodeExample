using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace XLib.BuildSystem.GameDefines {

	public static class GameFeatureConfig {
		
		private const string FeaturesConfig = @"../build/config/_features.json";
		
		[UsedImplicitly]
		public class ConfigEntry {
			public string Name { get; set; }
			public string Define { get; set; }
			public string Comment { get; set; }
			public bool DefaultOn { get; set; }
		}

		public static ConfigEntry[] LoadConfig() {
			var json = File.ReadAllText(FeaturesConfig);
			return JsonConvert.DeserializeObject<ConfigEntry[]>(json);
		}
	}

}