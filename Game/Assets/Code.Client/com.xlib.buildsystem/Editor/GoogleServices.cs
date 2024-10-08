using System.IO;
using UnityEditor;

namespace XLib.BuildSystem {

	public static class GoogleServices {
		private static readonly string SrcFolder = Path.Combine("..", "Build", "google-services");
		private static readonly string TargetJsonFolder = Path.Combine("Assets", "Settings", "GoogleServices");
		private static readonly string TargetXmlFolder = Path.Combine("Assets", "Plugins", "Android", "FirebaseApp.androidlib", "res", "values");
		
		[MenuItem("Build/Firebase/Set Internal Environment")]
		public static void SetDevEnvironment() {
			BuildRunner.Logger.Log($"GoogleServices: Internal");
			CopyFiles(Path.Combine(SrcFolder, "dev"), TargetJsonFolder, "*.plist");
			CopyFiles(Path.Combine(SrcFolder, "dev"), TargetJsonFolder, "*.json");
			CopyFiles(Path.Combine(SrcFolder, "dev"), TargetXmlFolder, "*.xml");
		}

		[MenuItem("Build/Firebase/Set Public Environment")]
		public static void SetProdEnvironment() {
			BuildRunner.Logger.Log($"GoogleServices: Public");
			CopyFiles(Path.Combine(SrcFolder, "public"), TargetJsonFolder, "*.plist");
			CopyFiles(Path.Combine(SrcFolder, "public"), TargetJsonFolder, "*.json");
			CopyFiles(Path.Combine(SrcFolder, "public"), TargetXmlFolder, "*.xml");
		}
		
		private static void CopyFiles(string srcFolder, string targetFolder, string mask) {
			foreach (var srcFile in Directory.GetFiles(srcFolder, mask, SearchOption.TopDirectoryOnly)) {
				var targetFile = Path.Combine(targetFolder, Path.GetFileName(srcFile));
				
				BuildRunner.Logger.Log($"GoogleServices: Copying: {targetFile}");
				File.Copy(srcFile, targetFile, true);
			}

			AssetDatabase.Refresh();
			BuildRunner.Logger.Log($"GoogleServices: Finished");
		}
	}

}