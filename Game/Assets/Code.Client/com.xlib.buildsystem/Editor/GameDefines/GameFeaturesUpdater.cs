using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

namespace XLib.BuildSystem.GameDefines {

	public static class GameFeaturesUpdater {
		private const string FeaturesDataPath = @"Assets/Code.Client/com.xlib.buildsystem/Runtime/GameFeature.cs";

		private const string FeatureDocTemplate = @"//////////////////////////////////////////////////
///// GENERATED FILE
//////////////////////////////////////////////////
// use 'Build/Tools/Generate Game Features' for rebuild this file


namespace XLib.BuildSystem {
	public static class GameFeature {
		
		/// <summary>
		/// Development build 
		/// </summary>
		public const bool DevelopmentBuild 
#if DEVELOPMENT_BUILD
		 = true;
#else		
		 = false;
#endif

		$(Features)
	}
}";

		private const string FeatureTemplate = @"
		/// <summary>
		/// $(Comment) 
		/// </summary>
		public const bool $(Name) 
#if $(Define)
		 = true;
#else		
		 = false;
#endif

";

		[MenuItem("Build/Defines/Generate Game Features", false, 300)]
		public static void UpdateFeatures() {
			var defineList = CustomDefineManager.GetDirectivesFromXmlFile();

			var sb = new StringBuilder(1024);
			foreach (var configEntry in GameFeatureConfig.LoadConfig()) {
				var define = configEntry.Define.ToUpperInvariant();
				if (defineList.All(x => x._name != define)) {
					defineList.Add(new Directive() {
						_enabled = configEntry.DefaultOn,
						_name = configEntry.Define.ToUpperInvariant(),
						_targets =
							CustomDefineManager.CdmBuildTargetGroup.Android | CustomDefineManager.CdmBuildTargetGroup.iOS | CustomDefineManager.CdmBuildTargetGroup.Standalone,
					});
				}

				sb.Append(FeatureTemplate
					.Replace("$(Name)", configEntry.Name)
					.Replace("$(Comment)", configEntry.Comment ?? string.Empty)
					.Replace("$(Define)", configEntry.Define));
			}

			for (var i = 0; i < defineList.Count; i++) defineList[i]._sortOrder = i;

			var csFile = FeatureDocTemplate
				.Replace("$(Features)", sb.ToString());

			File.WriteAllText(FeaturesDataPath, csFile, Encoding.UTF8);

			// save defines and recompile
			CustomDefineManager.SaveDirectives(defineList);
		}
	}

}