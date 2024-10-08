using System.IO;
using UnityEditor;
using UnityEngine;

namespace XLib.Unity.Tools {

	public static class AsmdefGenerator {

		[MenuItem("Tools/Generate asmdef files")]
		public static void UpdateAsmdef() {
			string[] thirdPartyFolders = { "Assets/ThirdParty" };

			var asmrefTemplate = @"{
    ""reference"": ""ThirdParty.Editor""
}";

			foreach (var thirdPartyFolder in thirdPartyFolders) ResolveReferences(thirdPartyFolder, asmrefTemplate);

			AssetDatabase.Refresh();
		}

		private static void ResolveReferences(string thirdPartyFolder, string asmrefTemplate) {
			foreach (var d in Directory.GetDirectories(thirdPartyFolder, "Editor", SearchOption.AllDirectories)) {
				var relativeDir = d.Substring(thirdPartyFolder.Length + 1).Replace('\\', '/');
				var moduleName = relativeDir.Substring(0, relativeDir.IndexOf('/'));

				var moduleDir = $"{thirdPartyFolder}/{moduleName}";

				//Debug.Log($"{moduleName}|{relativeDir}|{moduleDir}");

				if (Directory.GetFiles(moduleDir, "*.asmdef", SearchOption.AllDirectories).Length > 0) {
					Debug.Log($"Skip {moduleName}: asmdef files found in module!");
					continue;
				}

				if (Directory.GetFiles(d, "*.asmref", SearchOption.TopDirectoryOnly).Length > 0) {
					Debug.Log($"Skip {moduleName}: already processed");
					continue;
				}

				var fn = $"{d}/{moduleName}.Editor.asmref";
				Debug.Log($"Processing {moduleName}: {fn}");

				File.WriteAllText(fn, asmrefTemplate);
			}
		}

	}

}