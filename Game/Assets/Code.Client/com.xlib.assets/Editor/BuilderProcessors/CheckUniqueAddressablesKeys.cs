using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using XLib.Assets.Configs;
using XLib.BuildSystem;
using XLib.BuildSystem.Types;
using XLib.Core.Utils;
using XLib.Unity.Utils;

namespace XLib.Assets.BuilderProcessors {

	internal class CheckUniqueAddressablesKeys : IBeforeBuildRunner {

		public int Priority => 2;

		public void OnBeforeBuild(BuildRunnerOptions options, RunnerReport report) {
			var blacklist = new HashSet<string> { "Resources", "EditorSceneList" };

			var usedKeys = new Dictionary<string, List<string>>();
			var groups = EditorUtils.LoadAssets<AddressableAssetGroup>();
			var atlasConfig = EditorUtils.LoadExistingAsset<AtlasInfoConfig>();

			var errors = new List<string>();

			foreach (var entry in groups.SelectMany(assetGroup => assetGroup.entries)) {
				if (blacklist.Contains(entry.address)) continue;

				if (!usedKeys.TryGetValue(entry.address, out var list)) {
					list = new List<string>();
					usedKeys.Add(entry.address, list);
				}

				list.Add($"key={entry.address}, path={entry.AssetPath}");

				if (entry.MainAsset == null) errors.Add($"key={entry.address}, path={entry.AssetPath} has no asset!");
			}

			foreach (var atlasEntry in atlasConfig.SpriteInfo) {
				foreach (var sprite in atlasEntry.sprites) {
					if (!usedKeys.TryGetValue(sprite, out var list)) {
						list = new List<string>();
						usedKeys.Add(sprite, list);
					}

					list.Add(sprite);
				}
			}

			if (errors.Count > 0) {
				report.ReportError($"Errors found: \n{errors.Print()}", false);

				foreach (var list in usedKeys.Values.Where(list => list.Count > 1)) {
					report.ReportError($"Duplicate keys found in files: \n{list.Print()}", false);
				}
			}

			report.ThrowOnError();
			
			report.Logger.Log("Check OK");
		}

		[MenuItem("Build/Assets/Check Addressables", false, 1000)]
		public static void CheckAddressablesKeysMenu() {
			var report = new RunnerReport(new Logger(nameof(CheckUniqueAddressablesKeys)));
			new CheckUniqueAddressablesKeys().OnBeforeBuild(new BuildRunnerOptions(), report);
			report.ThrowOnError();
		}

	}

}