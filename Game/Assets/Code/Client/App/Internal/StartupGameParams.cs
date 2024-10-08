using Newtonsoft.Json;

namespace Client.App.Internal {

	public class StartupGameParams {
		
		public bool UseCustomProfileId { get; set; }
		public string CustomProfileId { get; set; }

		public bool UseCustomBuildVersion { get; set; }
		public int CustomBuildVersion { get; set; }
		public bool UseCustomBundleVersion { get; set; }
		public string CustomBundleClientVersion { get; set; }
		public int CustomBundleBuildNumber { get; set; }

		// ignore these properties from storing  

		[JsonIgnore]
		public bool ResetPlayerProfile { get; set; }

		[JsonIgnore]
		public bool SkipTutorial { get; set; }
		
		[JsonIgnore]
		public bool FullProfile { get; set; }
}

}