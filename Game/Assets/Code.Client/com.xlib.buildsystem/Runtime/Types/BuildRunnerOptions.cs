#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build;
using XLib.Core.Parsers;

namespace XLib.BuildSystem.Types {
	
	public enum BuildBundleMode {
		BuiltInBundles,	// собрать билд со встроенными бандлами
		RemoteBundles, // собрать билд с внешними бандлами
	}

	public class BuildRunnerOptions : DynamicOptions<BuildRunnerOptions> {
		private const string FeatureKey = "Feature";

		public BuildRunnerOptions() { }

		private BuildRunnerOptions(Dictionary<string, string> data) : base(data) { }

		public BuildTarget Target { get => Get<BuildTarget>(nameof(Target)); set => Set(nameof(Target), value); }

		public BuildOptions BuildOptions { get => (BuildOptions)Get<long>(nameof(BuildOptions), 0); set => Set(nameof(BuildOptions), (long)value); }

		public string BundleTarget { get => Get<string>(nameof(BundleTarget), string.Empty); set => Set(nameof(BundleTarget), value); }
		
		public string ProjectDir { get => Get<string>(nameof(ProjectDir), string.Empty); set => Set(nameof(ProjectDir), value); }
		
		public string AdditionalIl2CppArgs { get => Get<string>(nameof(AdditionalIl2CppArgs), string.Empty); set => Set(nameof(AdditionalIl2CppArgs), value); }

		// public BuildBundleMode BundleMode { get => Get<BuildBundleMode>(nameof(BundleMode)); set => Set(nameof(BundleMode), value); }
		public bool AppBundle { get => Get<bool>(nameof(AppBundle), false); set => Set(nameof(AppBundle), value); }
		// public bool IsOnlyAsset { get => Get<bool>(nameof(IsOnlyAsset), false); set => Set(nameof(IsOnlyAsset), value); }

		public bool CommandLineBuild { get => Get<bool>(nameof(CommandLineBuild), false); set => Set(nameof(CommandLineBuild), value); }

		public bool ExportAndroidStudio { get => Get<bool>(nameof(ExportAndroidStudio), false); set => Set(nameof(ExportAndroidStudio), value); }
		
		public AndroidCreateSymbols AndroidCreateSymbols { get => Get(nameof(AndroidCreateSymbols), AndroidCreateSymbols.Disabled); set => Set(nameof(AndroidCreateSymbols), value); }

		public bool UseDevGoogleServices { get => Get<bool>(nameof(UseDevGoogleServices), false); set => Set(nameof(UseDevGoogleServices), value); }

		public string OutputPath { get => Get<string>(nameof(OutputPath), null); set => Set(nameof(OutputPath), value); }

		public string OutputFileName { get => Get<string>(nameof(OutputFileName), null); set => Set(nameof(OutputFileName), value); }

		public string BundleId { get => Get<string>(nameof(BundleId), null); set => Set(nameof(BundleId), value); }

		public bool DevelopmentBuild { get => Get<bool>(nameof(DevelopmentBuild), false); set => Set(nameof(DevelopmentBuild), value); }

		public string KeystorePath { get => Get<string>(nameof(KeystorePath), string.Empty); set => Set(nameof(KeystorePath), value); }

		public string CustomDefines { get => Get<string>(nameof(CustomDefines), string.Empty); set => Set(nameof(CustomDefines), value); }

		public string KeystorePass { get => Get<string>(nameof(KeystorePass), string.Empty); set => Set(nameof(KeystorePass), value); }
		public string KeyaliasName { get => Get<string>(nameof(KeyaliasName), string.Empty); set => Set(nameof(KeyaliasName), value); }
		public string KeyaliasPass { get => Get<string>(nameof(KeyaliasPass), string.Empty); set => Set(nameof(KeyaliasPass), value); }

		public string VersionString { get => Get<string>(nameof(VersionString), string.Empty); set => Set(nameof(VersionString), value); }
		public int VersionCode { get => Get<int>(nameof(VersionCode), 0); set => Set(nameof(VersionCode), value); }

		public string ProductName { get => Get<string>(nameof(ProductName), string.Empty); set => Set(nameof(ProductName), value); }

		public bool HasFeature(string key) => Get<bool>($"{FeatureKey}.{key}", false);
		public void SetFeature(string key, bool enabled) => Set($"{FeatureKey}.{key}", enabled);

		public IEnumerable<string> ActiveFeatures =>
			Options
				.Where(x => x.Key.StartsWith(FeatureKey) && x.Value is string s && s.Parse<bool>())
				.Select(x => x.Key[FeatureKey.Length..]);

		public BuildRunnerOptions Clone() => FromJson(ToJson());

		public static BuildRunnerOptions FromJson(string json) {
			try {
				return new BuildRunnerOptions(JsonConvert.DeserializeObject<Dictionary<string, string>>(json));
			}
			catch (Exception) {
				return new BuildRunnerOptions();
			}
		}

		public static BuildRunnerOptions FromEnvironment() {
			var result = new BuildRunnerOptions();
			foreach (DictionaryEntry args in Environment.GetEnvironmentVariables()) {
				var key = (string)args.Key;
				if (key.ToLowerInvariant().StartsWith("feature_")) key = $"Feature.{key["feature_".Length..]}";
				
				result.Set(key, (string)args.Value);
			}
			return result;
		}

		public static BuildRunnerOptions FromJsonFile(string jsonFileName) {
			if (jsonFileName.IsNullOrEmpty()) return new BuildRunnerOptions();

			var path = Path.GetFullPath(jsonFileName);
			var baseDir = Path.GetDirectoryName(path);
			var fileName = Path.GetFileName(path);
			var options = new BuildRunnerOptions();
			RecursiveLoadConfig(options, baseDir, fileName);

			return options;
		}

		private static void RecursiveLoadConfig(BuildRunnerOptions options, string basePath, string jsonFileName) {
			var fullName = Path.Combine(basePath, jsonFileName);
			if (!File.Exists(fullName)) throw new BuildFailedException($"Cannot open file with options: '{fullName}'");

			try {
				var json = File.ReadAllText(fullName, Encoding.UTF8);
				var loadedConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
				loadedConfig.TryGetValue("$base", out var baseFileName);

				if (!string.IsNullOrEmpty(baseFileName)) {
					foreach (var baseFn in baseFileName.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())) {
						RecursiveLoadConfig(options, basePath, baseFn);
					}
				}

				loadedConfig.Remove("$base");
				options.OverrideFrom(new BuildRunnerOptions(loadedConfig));
			}
			catch (BuildFailedException) {
				throw;
			} 
			catch (Exception e) {
				throw new BuildFailedException($"Error loading JSON from file '{jsonFileName}': {e.Message}");
			}
		}
	}

}

#endif