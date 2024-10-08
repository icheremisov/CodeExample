using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace XLib.BuildSystem {

	public static class UsageAssetReport {
		public const int Kb = 1024;
		public const int Mb = 1024 * Kb;

		public class BundleInfo {
			public string Name { get; }
			public long Size { get; }

			public BundleInfo(string name, long size) {
				Name = name;
				Size = size;
			}
		}

		public class SectionInfo {
			public long TotalSize = 0;
			public List<BundleInfo> Bundles = new();
		}

		private static Dictionary<string, SectionInfo> _sections = new();

		[MenuItem("Build/Validator/Assets Stats")]
		public static void PrintStats() {
			StatAddressables();
			StatLastBuildReport("../_Output");
		}

		public static void StatLastBuildReport(string outputPath) {
			Debug.Log("#####################################");
			var sourcePath = "Library/LastBuild.buildreport";
			var targetPath = "Assets/BuildReports/build_assetstats.buildreport";

			if (!File.Exists(sourcePath)) {
				Debug.LogWarning("BuildReport отсутствует. Бюджет встроенных ресурсов недоступен. Соберите любой билд и вновь запустите команду");
				return;
			}

			if (!Directory.Exists("Assets/BuildReports/")) Directory.CreateDirectory("Assets/BuildReports/");

			File.Copy(sourcePath, targetPath, true);
			AssetDatabase.ImportAsset(targetPath);

			var report = AssetDatabase.LoadAssetAtPath<BuildReport>(targetPath);

			try {
				AnalyzeBuildReport(report, outputPath);
			}
			catch (Exception ex) {
				Debug.LogException(ex);
			}

			AssetDatabase.DeleteAsset(targetPath);
			File.Delete(targetPath);
		}

		public static void AnalyzeBuildReport(BuildReport report, string outputPath) {
			//Парсим report файл и находим все ассеты
			var totalSize = 0L;
			var assets = GetBuildAssets(report, ref totalSize);

			var result = new StringBuilder();
			result.AppendLine($"Последний билд для {report.summary.platform} весит {FormatSize((long)report.summary.totalSize)}");
			result.AppendLine($"Всего встроенно {assets.Count} асетов. Общий размер:{FormatSize(totalSize)}. Самые большие: ");

			assets.Sort(SortBundlesBySize);
			var minCount = 5;
			foreach (var asset in assets) {
				if (asset.Size < Mb && minCount < 0) break;
				--minCount;
				result.AppendLine($" - {asset.Name}: {FormatSize(asset.Size)}");
			}

			if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
			
			using (var fs = File.Open(Path.Combine(outputPath, "assets.log"), FileMode.Create)) {
				using (var writer = new StreamWriter(fs)) writer.WriteLine(result.ToString());
			}
			Debug.Log(result);
		}

		private static List<BundleInfo> GetBuildAssets(BuildReport report, ref Int64 totalSize)
		{
			var assets = new List<BundleInfo>();
			foreach (var reportPackedAsset in report.packedAssets)
			{
				foreach (var assetInfo in reportPackedAsset.contents)
				{
					assets.Add(new BundleInfo(assetInfo.sourceAssetPath, (long)assetInfo.packedSize));
					totalSize += (long)assetInfo.packedSize;
				}
			}
			return assets;
		}

		public static void StatAddressables() {
			var buildDir = new DirectoryInfo("./Library/com.unity.addressables/aa");

			if (!buildDir.Exists) {
				Debug.LogWarning("Не обнаружено не одной сборки ресурсов. Выполните команду:");
				Debug.LogWarning("Window > Asset Managment > Addressables > Groups > Build -> New Build -> Default Build Script");
				return;
			}

			foreach (var platform in buildDir.EnumerateDirectories()) StatAddressablePlatform(platform);
		}

		private static void StatAddressablePlatform(DirectoryInfo platform) {
			_sections.Clear();

			Debug.Log("#####################################");
			Debug.Log("Бюджеты ресурсов для платформы " + platform.Name);

			foreach (var dir in platform.EnumerateDirectories()) ParseSection(dir);

			// AddSingleBundle(platform, "config_*.bundle", "config");

			// foreach (var section in _sections) {
			// 	var bundles = section.Value.Bundles;
			//
			// 	var total = FormatSize(section.Value.TotalSize);
			// 	var average = FormatSize(section.Value.TotalSize / bundles.Count);
			//
			// 	var result = "Секция " + section.Key + " содержит " + bundles.Count + " бандлов. Весит: " + total + ". В среднем: " + average + ". Самые большие ассеты: \n";
			// 	bundles.Sort(SortBundlesBySize);
			// 	for (var i = 0; i < Math.Min(bundles.Count, 5); i++) result += $" - {bundles[i].Name}: {FormatSize(bundles[i].Size)}\n";
			// 	Debug.Log(result);
			// }
		}

		private static void ParseSection(DirectoryInfo dir) {
			var sectionName = dir.Name;
			foreach (var file in dir.EnumerateFiles()) AddFileToSection(file, sectionName);
		}

		private static void AddSingleBundle(DirectoryInfo dir, string pattern, string section) {
			var file = dir.EnumerateFiles(pattern).First();
			AddFileToSection(file, section);
		}

		private static void AddFileToSection(FileInfo file, string sectionName) {
			if (!file.Name.EndsWith(".bundle")) return;

			if (!_sections.ContainsKey(sectionName)) _sections.Add(sectionName, new SectionInfo());

			var section = _sections[sectionName];
			var maxBundleSize = getMaxBundleSize(sectionName);

			var bundleName = file.Name[..file.Name.LastIndexOf("_", StringComparison.InvariantCulture)];

			if (file.Length > maxBundleSize)
				Debug.LogWarning($"Бандл {sectionName}/{bundleName} ({FormatSize(file.Length)}) превышает максимально допустимый размер {FormatSize(maxBundleSize)}");

			section.TotalSize += file.Length;
			section.Bundles.Add(new BundleInfo(bundleName, file.Length));
		}

		private static int SortBundlesBySize(BundleInfo a, BundleInfo b) => a.Size == b.Size ? 0 : a.Size > b.Size ? -1 : 1;

		private static int getMaxBundleSize(string section) {
			return section == "content_scenes_screens" ? 1 * Mb : 500 * Kb;
		}

		private static string FormatSize(Int64 size) {
			if (size < 1024) return size + " B";
			if (size < 1024 * 1024) return (size / 1024.00).ToString("F2") + " KB";
			if (size < 1024 * 1024 * 1024) return (size / (1024.0 * 1024.0)).ToString("F2") + " MB";
			return (size / (1024.0 * 1024.0 * 1024.0)).ToString("F2") + " GB";
		}
	}

}