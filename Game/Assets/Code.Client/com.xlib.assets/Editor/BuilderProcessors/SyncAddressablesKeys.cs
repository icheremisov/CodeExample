using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using XLib.BuildSystem;
using XLib.BuildSystem.Types;
using XLib.Unity.Utils;

namespace XLib.Assets.BuilderProcessors {
	
	// IBeforeBuildRunner disabled by design 

	// ReSharper disable once UnusedType.Global
	public class SyncAddressablesKeys /*: IBeforeBuildRunner*/ {

		public int Priority => 3;

		public void OnBeforeBuild(BuildRunnerOptions options, RunnerReport report) {
			var blacklist = new HashSet<string> { "EditorSceneList" };
			var groups = EditorUtils.LoadAssets<AddressableAssetGroup>();
			foreach (var assetGroup in groups) {
				foreach (var entry in assetGroup.entries) {
					var key = Path.GetFileNameWithoutExtension(entry.AssetPath);
					if (string.IsNullOrEmpty(key) || blacklist.Contains(key) || blacklist.Contains(entry.address)) continue;

					if (key == entry.address) continue;

					report.Logger.Log($"Asset address changed from '{entry.address}' to '{key}'");
					entry.SetAddress(key);
				}

				EditorUtility.SetDirty(assetGroup);
			}
		}

		[MenuItem("Build/Assets/Sync Addressables", false, 1000)]
		public static void BuildMenu() {
			var report = new RunnerReport(new Logger(nameof(SyncAddressablesKeys)));
			new SyncAddressablesKeys().OnBeforeBuild(new BuildRunnerOptions(), report);
			report.ThrowOnError();
		}
	}

}