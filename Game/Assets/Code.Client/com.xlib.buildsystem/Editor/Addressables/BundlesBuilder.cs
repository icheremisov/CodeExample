using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using XLib.BuildSystem.Types;
using XLib.Configs.Utils;

namespace XLib.BuildSystem.Addressables {

	public static class BundlesBuilder {
		private static string GetBundleVersions(BuildRunnerOptions options) {
			if (!options.Has("Target")) options.Target = EditorUserBuildSettings.activeBuildTarget;
			return options.Target switch {
				BuildTarget.Android => $"{PlayerSettings.bundleVersion}_{PlayerSettings.Android.bundleVersionCode}",
				BuildTarget.iOS     => $"{PlayerSettings.bundleVersion}_{PlayerSettings.iOS.buildNumber}",
				_                   => $"{PlayerSettings.bundleVersion}_{options.VersionCode}"
			};
		}

		// [MenuItem("Build/Assets/Build BuiltIn Bundles", false, 1000)]
		// private static void BuildBundlesMenu() {
		// 	var report = new RunnerReport(new Logger("Build Bundles"));
		// 	BuildBundles(new BuildRunnerOptions() { BundleMode = BuildBundleMode.BuiltInBundles }, report);
		// 	report.ThrowOnError();
		// }
		//
		// [MenuItem("Build/Assets/Build Remote Bundles", false, 1000)]
		// private static void BuildRemoteBundlesMenu() {
		// 	var report = new RunnerReport(new Logger("Build Bundles"));
		// 	BuildBundles(new BuildRunnerOptions() { BundleMode = BuildBundleMode.RemoteBundles }, report);
		// 	report.ThrowOnError();
		// }
		//
		// public static void BuildBundles(BuildRunnerOptions options, RunnerReport report) {
		// 	var builtInBundles = options.BundleMode == BuildBundleMode.BuiltInBundles;
		// 	var settings = AddressableAssetSettingsDefaultObject.Settings;
		// 	var lastProfileId = settings.activeProfileId;
		// 	var configHash = ConfigUtils.CalculateConfigHash("Assets/Configs");
		// 	
		// 	try {
		// 		var version = GetBundleVersions(options);
		//
		// 		settings.activeProfileId = settings.profileSettings.GetProfileId(builtInBundles ? "Default" : "Remote");
		// 		settings.BuildRemoteCatalog = !builtInBundles;
		// 		settings.OverridePlayerVersion = version;
		// 		AddressableAssetSettingsDefaultObject.Settings = settings;
		// 		AssetDatabase.SaveAssetIfDirty(settings);
		// 		
		// 		var remoteCatalogPath = settings.RemoteCatalogBuildPath.GetValue(settings);
		// 		Debug.Log("RemoteCatalogPath: " + remoteCatalogPath);
		// 		Debug.Log("BundleMode: " + options.BundleMode);
		//
		// 		var catalogPath = Path.Combine(remoteCatalogPath, "..");
		// 		if (Directory.Exists(catalogPath))
		// 			Directory.Delete(catalogPath, true);
		//
		// 		AddressableAssetSettings.CleanPlayerContent();
		// 		AddressableAssetSettings.BuildPlayerContent(out var result);
		//
		// 		// var tempPath = Path.GetDirectoryName(Application.dataPath) + "/" + Addressables.LibraryPath + PlatformMappingService.GetPlatformPathSubFolder() + "/addressables_content_state.bin";
		// 		// var contentState = ContentUpdateScript.LoadContentState(tempPath);
		// 		// var buildOp = ContentUpdateScript.BuildContentUpdate(Settings, tempPath);
		//
		//
		// 		foreach (var buildResult in result.AssetBundleBuildResults) {
		// 			report.Logger.Log($"{buildResult.InternalBundleName}: {buildResult.FilePath} {buildResult.Hash}");
		// 		}
		//
		// 		foreach (var file in result.FileRegistry.GetFilePaths()) {
		// 			report.Logger.Log(file);
		// 		}
		//
		// 		if (!result.Error.IsNullOrEmpty())
		// 			report.ReportError($"{nameof(BundlesBuilder)} error: '{result.Error}'");
		// 		else
		// 			report.Logger.Log($"{nameof(BundlesBuilder)} built in {result.Duration:0.00} sec");
		//
		// 		if (builtInBundles) return;
		//
		// 		File.Copy(result.ContentStateFilePath, $"{remoteCatalogPath}/content_state.{version}.bin", true);
		//
		// 		var bundleInfo = new BundleVersionInfo() {
		// 			version = version,
		// 			config_hash = configHash,
		// 			rev = options.VersionCode
		// 		};
		//
		// 		var versionStorage = VersionService.LoadInEditor(false);
		// 		versionStorage.configHash = configHash;
		// 		VersionService.SaveInEditor(versionStorage);
		// 		
		// 		File.WriteAllText(Path.Combine(remoteCatalogPath, "..", $"bundle-{VersionService.BundleTarget}.json"), JsonConvert.SerializeObject(bundleInfo, Formatting.Indented));
		// 	}
		// 	finally {
		// 		settings.OverridePlayerVersion = string.Empty;
		// 		settings.activeProfileId = lastProfileId;
		// 		AddressableAssetSettingsDefaultObject.Settings = settings;
		// 		AssetDatabase.SaveAssetIfDirty(settings);
		// 	}
		// }
	}

}