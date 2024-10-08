using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using XLib.BuildSystem.Exceptions;
using XLib.BuildSystem.GameDefines;
using XLib.BuildSystem.Types;

namespace XLib.BuildSystem {

	public static partial class BuildRunner {
		static BuildRunner() {
			if (Application.isBatchMode) {
				Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
				Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
				Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
				Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
			}
		}

		// ReSharper disable once UnusedMember.Global
		public static void ExternalTouchCode() {
			// method for touching code and forcing compilation before actual run
		}
		
		// ReSharper disable once UnusedMember.Global
		public static void ExternalValidation() {
			var options = LoadOptions();
			OdinValidatorRunner.Run(options);
		}

		public static void GenerateUsageAssetReport() {
			var options = LoadOptions();
			UsageAssetReport.StatLastBuildReport(options.OutputPath);
		}
		
		// ReSharper disable once UnusedMember.Global
		public static void ExternalAndroidBuild() {
			
			var options = LoadOptions();

			options.Target = BuildTarget.Android;
			options.CommandLineBuild = true;
			
			Build(options);
		}

		// ReSharper disable once UnusedMember.Global
		public static void ExternalIOSBuild() {

			var options = LoadOptions();
			
			options.Target = BuildTarget.iOS;
			options.CommandLineBuild = true;
			
			Build(options);
		}

		private static BuildRunnerOptions LoadOptions() {
			var optionsFile = BuildRunnerOptions.FromEnvironment()
				.OverrideFrom(FromCommandLine())
				.Get("optionsFile", string.Empty);
			
			var options = new BuildRunnerOptions();

			foreach (var feature in GameFeatureConfig.LoadConfig()) options.SetFeature(feature.Name, feature.DefaultOn);

			if (!optionsFile.IsNullOrEmpty()) {
				Logger.Log($"Loading options from file '{optionsFile}'");
				options.OverrideFrom(BuildRunnerOptions.FromJsonFile(optionsFile));
			}

			options
				.OverrideFrom(BuildRunnerOptions.FromEnvironment())
				.OverrideFrom(FromCommandLine());
			
			return options;
		}

		private static BuildRunnerOptions _options;

		public static void Build(BuildRunnerOptions srcOptions) {
			AppDomain.CurrentDomain.FirstChanceException += (sender, e) => LogException(e.Exception);
			
			_options = srcOptions.Clone();

			var report = new RunnerReport(Logger);

			UpdateVersion(report);

			_options.ProjectDir = DetectProjectDir(report);
			
			CheckOption(report, nameof(BuildRunnerOptions.Target));
			CheckOption(report, nameof(BuildRunnerOptions.BundleId));
			// CheckOption(report, nameof(BuildRunnerOptions.BundleMode));
			// CheckOption(report, nameof(BuildRunnerOptions.BundleTarget));
			
			_options.BuildOptions = _options.BuildOptions
				.With(BuildOptions.CompressWithLz4);

			if (_options.VersionString.IsNullOrEmpty()) _options.VersionString = VersionService.ShortVersionString;
			if (_options.VersionCode <= 0) _options.VersionCode = VersionService.VersionCode;
			
			var prevAllowDebugging = EditorUserBuildSettings.allowDebugging;
			var prevConnectProfiler = EditorUserBuildSettings.connectProfiler;
			var prevIsDevelopmentBuild = EditorUserBuildSettings.development;
			var prevCopyPdbFiles = EditorUserBuildSettings.GetPlatformSettings("Standalone", "CopyPDBFiles").ToLowerInvariant() == "true";

			
			try {
				
				Logger.Log($"---------------------------------------------\n{_options.Target} : Build STARTED");

				Logger.Log($"Build Options:\n{_options}");

				PerformBuild(report);
				
				report.ThrowOnError();

				Logger.Log($"---------------------------------------------\n{_options.Target} : Build COMPLETED");

				UsageAssetReport.StatLastBuildReport(_options.OutputPath);

				if (_options.CommandLineBuild) EditorApplication.Exit(0);
			}
			catch (BuildErrorsException) {
				Logger.LogError($"---------------------------------------------\n{_options.Target} : Build FAILED");

				report.DumpErrors();

				if (_options.CommandLineBuild) EditorApplication.Exit(1);
				
			}
			catch (Exception e) {

				Logger.LogError($"---------------------------------------------\n{_options.Target} : Build FAILED");

				Logger.LogException(e);
				report.DumpErrors();

				if (_options.CommandLineBuild) EditorApplication.Exit(1);
			}
			finally {
				EditorUserBuildSettings.allowDebugging = prevAllowDebugging;
				EditorUserBuildSettings.connectProfiler = prevConnectProfiler;
				EditorUserBuildSettings.development = prevIsDevelopmentBuild;
				EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", prevCopyPdbFiles.ToString());

				_options = null;
			}
			
		}
		private static void LogException(Exception e)
		{
			Debug.LogError("Exception during build:");
			Debug.LogError(e.Message);
			Debug.LogError(e.StackTrace);
		}

		private static void UpdateVersion(RunnerReport report) {
			var version = VersionService.LoadInEditor(false);

			var versionSrcFile = Path.GetFullPath(Path.Join("..", "version.txt"));

			if (!File.Exists(versionSrcFile)) report.ReportError($"Cannot read version from '{versionSrcFile}'");

			version.versionString = File.ReadAllLines(versionSrcFile).First().Trim();
			if (string.IsNullOrEmpty(version.versionString)) report.ReportError($"Cannot read version from '{versionSrcFile}'");

			version.description = _options.DevelopmentBuild ? "dev" : "";
			version.bundleTarget = _options.BundleTarget;
			if (!_options.CommandLineBuild) version.env = "(manual build)"; 
			
			VersionService.SaveInEditor(version);
		}

		private static void PerformBuild(RunnerReport report) {
			
			report.ThrowOnError();
			
			if (EditorApplication.isPlaying) report.ReportError($"You must stop player before build!");
			if (EditorUserBuildSettings.activeBuildTarget != _options.Target) report.ReportError($"You must switch target before build: active={EditorUserBuildSettings.activeBuildTarget}; required={_options.Target}");
			if (_options.Target != BuildTarget.Android
				&& _options.Target != BuildTarget.iOS
				&& _options.Target != BuildTarget.StandaloneWindows64) {
				report.ReportError($"Target {_options.Target} not supported");
			}

			var bundleId = _options.BundleId ?? PlayerSettings.applicationIdentifier;
			var outputPath = _options.OutputPath ?? GetDefaultOutputPath();
			var outputFileName = _options.OutputFileName ?? GetDefaultOutputFileName(bundleId);
			
			Logger.Log($"bundleId={bundleId}\noutputPath={outputPath}\noutputFileName={outputFileName}");
			
			if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

			var options = _options.BuildOptions;
			options = options
				.With(BuildOptions.Development, _options.DevelopmentBuild);

			// if (_options.Target == BuildTarget.Android) {
			// 	options = options
			// 	.With(BuildOptions.DetailedBuildReport);
			// }
			
			var targetGroup = BuildPipeline.GetBuildTargetGroup(_options.Target);
			SetupParameters(targetGroup, report);
			
			RunBeforeBuild(report);

			var buildPlayerOptions = new BuildPlayerOptions {
				scenes = GetScenes(),
				locationPathName = $"{outputPath}/{outputFileName}",
				targetGroup = targetGroup,
				target = _options.Target,
				options = options
			};

			// if (_options.IsOnlyAsset) {
			// 	report.Logger.Log("Skipping build, only updating bundles");
			// }
			// else {
				report.Logger.Log("Building player...");
				var unityReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
				var summary = unityReport.summary;
				if (summary.result != BuildResult.Succeeded) report.ReportError($"Error building player: {summary.result}");
			// }

			RunAfterBuild(report);
		}

		private static void SetupParameters(BuildTargetGroup targetGroup, RunnerReport report) {
			if (_options.Target == BuildTarget.Android) EditorUserBuildSettings.exportAsGoogleAndroidProject = _options.ExportAndroidStudio;

			var allDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			var defines = allDefines.ToUpperInvariant().Split(';', StringSplitOptions.RemoveEmptyEntries).Distinct().ToHashSet();

			{
				var sb = new StringBuilder(1024);
				
				foreach (var feature in GameFeatureConfig.LoadConfig()) {
					var enabled = _options.HasFeature(feature.Name);
					SetDefineState(feature.Define, enabled);

					sb.Append($"{feature.Name}={enabled} ");
				}
				
				Logger.Log($"GameFeatures: {sb}");
			}

			if (_options.CustomDefines.Length > 0) {
				var customDefines = allDefines.ToUpperInvariant().Split(';', StringSplitOptions.RemoveEmptyEntries).Distinct();

				foreach (var customDefine in customDefines) {
					if (customDefine.StartsWith('!'))
						SetDefineState(customDefine[1..], false);
					else
						SetDefineState(customDefine, true);
				}
			}

			if (Application.HasProLicense()) {
				PlayerSettings.SplashScreen.showUnityLogo = false;
				Logger.Log("Unity logo: disabled");
			}
			else {
				Logger.Log("Unity logo: CANNOT DISABLE (No License Found)");
			}


			var isDebugMode = _options.DevelopmentBuild;
			EditorUserBuildSettings.allowDebugging = isDebugMode;
			EditorUserBuildSettings.connectProfiler = isDebugMode;
			EditorUserBuildSettings.development = isDebugMode;
			EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", isDebugMode.ToString());
			PlayerSettings.applicationIdentifier = _options.BundleId;
			if (!_options.ProductName.IsNullOrEmpty()) PlayerSettings.productName = _options.ProductName;
			
			SetDefineState("DEVELOPMENT_BUILD", isDebugMode);

			PlayerSettings.bundleVersion = _options.VersionString;
					
			switch (_options.Target) {
				case BuildTarget.Android:
					PlayerSettings.Android.bundleVersionCode = _options.VersionCode;

					PlayerSettings.Android.useCustomKeystore = true;
					EditorUserBuildSettings.androidCreateSymbols = _options.AndroidCreateSymbols;
					Logger.Log($"AndroidCreateSymbols: {EditorUserBuildSettings.androidCreateSymbols}");

					CheckOption(report, nameof(BuildRunnerOptions.KeystorePath));
					CheckOption(report, nameof(BuildRunnerOptions.KeystorePass));
					CheckOption(report, nameof(BuildRunnerOptions.KeyaliasName));
					CheckOption(report, nameof(BuildRunnerOptions.KeyaliasPass));
					
					PlayerSettings.Android.keystoreName = DetectKeystorePath(report);

					PlayerSettings.Android.keystorePass = _options.KeystorePass;
					PlayerSettings.Android.keyaliasName = _options.KeyaliasName;
					PlayerSettings.Android.keyaliasPass = _options.KeyaliasPass;

					EditorUserBuildSettings.buildAppBundle = _options.AppBundle;
					PlayerSettings.Android.useAPKExpansionFiles = _options.AppBundle;
					Logger.Log($"Android build mode: {(EditorUserBuildSettings.buildAppBundle ? "apk + bundle" : "single apk")}");
					break;

				case BuildTarget.iOS:
					PlayerSettings.iOS.buildNumber = _options.VersionCode.ToString();
					break;
				
				case BuildTarget.StandaloneWindows64:
					break;
			}
			
			PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines.JoinToString(';'));
			
			void SetDefineState(string define, bool enabled) {
				define = define.ToUpperInvariant();
				if (enabled && !defines.Contains(define)) {
					defines.Add(define);
				}
				else if (!enabled && defines.Contains(define)) {
					defines.Remove(define);
				}
			}
			
			PlayerSettings.SetAdditionalIl2CppArgs(_options.AdditionalIl2CppArgs ?? string.Empty);
			
			SetupGoogleServices();
		}

		private static void SetupGoogleServices() {
			if (_options.UseDevGoogleServices) GoogleServices.SetDevEnvironment();
			else GoogleServices.SetProdEnvironment();
		}

		private static string[] GetScenes() => EditorBuildSettings.scenes.Where(x => x.enabled).SelectToArray(x => x.path);
	}

}