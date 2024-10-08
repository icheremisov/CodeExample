using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XLib.BuildSystem.Types;
using XLib.Core.Utils;
using XLib.Unity.Utils;

namespace XLib.BuildSystem {

	public static partial class BuildRunner {
		public static readonly Logger Logger = new(nameof(BuildRunner));

		private const string DefaultFolderName = "Bin";
		private static string BaseFolderName => $"{Application.dataPath.Replace("/Assets", string.Empty)}/{DefaultFolderName}";
		public static string BinPath => BaseFolderName;
		public static BuildRunnerOptions FromCommandLine() {
			var result = new BuildRunnerOptions();
			foreach (var args in EditorCommandLine.Args) {
				result.Set(args.Key, args.Value);
			}

			return result;
		}

		private static void CheckOption(RunnerReport buildReport, string optionName) {
			if (!_options.Has(optionName)) buildReport.ReportError($"Expected required parameter {optionName}");
		}

		private static void RunBeforeBuild(RunnerReport report) {
			var hookType = TypeOf<IBeforeBuildRunner>.Raw;

			var hooks = TypeUtils.EnumerateAll(x => x.IsClass && !x.IsAbstract && hookType.IsAssignableFrom(x))
				.SelectToArray(x => (IBeforeBuildRunner)Activator.CreateInstance(x));

			foreach (var hook in hooks.OrderBy(x => x.Priority)) {
				Logger.Log($"Executing pre-build hook: {hook.GetType().FullName}");

				try {
					hook.OnBeforeBuild(_options, report);
				}
				catch (Exception e) {
					report.ReportError($"Error executing {hook.GetType().Name}: {e.Message}", false);
					report.ReportError(e, false);
				}
				
				report.ThrowOnError();
			}
		}

		private static void RunAfterBuild(RunnerReport report) {
			var hookType = TypeOf<IAfterBuildRunner>.Raw;

			var hooks = TypeUtils.EnumerateAll(x => x.IsClass && !x.IsAbstract && hookType.IsAssignableFrom(x))
				.SelectToArray(x => (IAfterBuildRunner)Activator.CreateInstance(x));

			foreach (var hook in hooks.OrderBy(x => x.Priority)) {
				Logger.Log($"Executing post-build hook: {hook.GetType().FullName}");

				hook.OnAfterBuild(_options, report);
				report.ThrowOnError();
			}
		}

		private static string GetDefaultOutputPath() {
			string folderName;

			switch (_options.Target) {
				case BuildTarget.StandaloneWindows:
					folderName = "Win32";
					break;

				case BuildTarget.StandaloneWindows64:
					folderName = "Win64";
					break;

				case BuildTarget.WebGL:
					folderName = "WebGL";
					break;

				case BuildTarget.Android: {
					if (_options.CommandLineBuild)
						folderName = "Android";
					else {
						folderName = $"Android{(_options.AppBundle ? "ApkBundle" : "ApkOnly")}";
						if (_options.ExportAndroidStudio) folderName += "Proj";
					}
				}
					break;

				case BuildTarget.iOS:
					folderName = "iOS";
					break;

				case BuildTarget.StandaloneLinux64:
					folderName = "Linux64";
					break;

				case BuildTarget.StandaloneOSX:
					folderName = "OSXUniversal";
					break;

				default:
					folderName = "Unknown";
					break;
			}

			return $"{BaseFolderName}/{folderName}";
		}

		private static string GetDefaultOutputFileName(string bundleId) => $"{bundleId}{GetExtension()}";

		private static string GetExtension() {
			switch (_options.Target) {
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64: return ".exe";

				case BuildTarget.StandaloneLinux64: return string.Empty;

				case BuildTarget.StandaloneOSX: return ".app";

				case BuildTarget.Android: return _options.AppBundle ? ".aab" : ".apk";

				case BuildTarget.iOS: return string.Empty;

				default: return ".UNDEFINED";
			}
		}

		private static string DetectKeystorePath(RunnerReport report) {
			var path = _options.KeystorePath;
			if (File.Exists(path)) return path;
			path = Path.Join(_options.ProjectDir, path);
			if (File.Exists(path)) return path;

			report.ReportError($"Cannot find keystore file for Android: '{_options.KeystorePath}' or '{path}'");
			return null;
		}

		private static string DetectProjectDir(RunnerReport report) {
			var dir = Directory.GetCurrentDirectory();

			while (!dir.IsNullOrEmpty()) {
				if (Directory.Exists(Path.Join(dir, "Assets"))) return dir;

				dir = Directory.GetParent(dir)?.FullName;
			}
			
			report.ReportError($"Cannot detect project dir");
			return null;
		}
	}

}