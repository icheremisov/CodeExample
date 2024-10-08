using System.IO;

namespace XLib.Core.Runtime.Files {
	
	public static class FileHelper {

		public static string FindDirectory(string targetDirectory) {
			
			var localDir = Directory.GetCurrentDirectory();

			string targetDir = null;

			while (!localDir.IsNullOrEmpty()) {
				var dir = Path.Combine(localDir, targetDirectory);
				if (Directory.Exists(dir)) {
					targetDir = dir;
					break;
				}

				localDir = Directory.GetParent(localDir)?.FullName;
			}

			return targetDir;
		}

	}

}