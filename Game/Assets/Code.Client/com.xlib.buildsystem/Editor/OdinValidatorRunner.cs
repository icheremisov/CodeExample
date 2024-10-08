using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.OdinValidator.Editor;
using UnityEditor;
using XLib.BuildSystem.Types;

namespace XLib.BuildSystem {

	public static class OdinValidatorRunner {
		public static readonly Logger Logger = new(nameof(OdinValidatorRunner));

		private class CategoryReport {
			private List<string> _errors = new(16);
			private List<string> _messages = new(16);

			public CategoryReport(string category) {
				Category = category;
			}

			public string Category { get; }

			public void ReportError(string error) {
				_errors.Add(error);
				Logger.LogError(error);
			}

			public void Log(string message) {
				_messages.Add(message);
				Logger.Log(message);
			}

			public int Errors => _errors.Count;

			public override string ToString() {
				var sb = new StringBuilder();
				sb.AppendLine($"--------------------------------------------- Category {Category} ---------------------------------------------");
				sb.AppendLine($"--------------------------------------------- {Errors} Errors ---------------------------------------------");
				foreach (var error in _errors) {
					sb.AppendLine(error);
				}
				sb.AppendLine($"--------------------------------------------- {_messages.Count} Messages ---------------------------------------------");
				foreach (var message in _messages) {
					sb.AppendLine(message);
				}
				return sb.ToString();
			}
		}

		[MenuItem("Build/Validator/Config")]
		public static void ConfigRun() {
			var profile = ValidationProfile.FindAll().First(validationProfile => validationProfile.name == "Config Profile");
			Logger.Log($"Validation Profile: {profile.name}");
			var categories = new Dictionary<string, CategoryReport>();
		
			using var sessionHandle = profile.ClaimSessionHandle();
			foreach (var vpResult in sessionHandle.Session.ValidateEverythingEnumeratorBatched()) {
				foreach (var result in vpResult.Explode()) {
					if (result.ResultType is ValidationResultType.IgnoreResult or ValidationResultType.Valid) continue;

					var path = result.DynamicObjectAddress.LatestAddress.AssetPath;
					var category = path.Split("/\\").At(1);

					if (!categories.ContainsKey(category)) categories[category] = new CategoryReport(category);
					var report = categories[category];

					var message = $"[{category}] {result.Message} at {path} ({result.ValidatorType.Name})";

					if (result.ResultType == ValidationResultType.Error) {
						report.ReportError(message);
					}
					else {
						report.Log(message);
					}
				}
			}
		}

		public static void Run(BuildRunnerOptions options) {
			var profile = ValidationProfile.FindAll().Last();
			// var profile = ValidationProfile.FindAll().First(validationProfile => validationProfile.name == "Config Profile");
			Logger.Log($"Validation Profile: {profile.name}");

			var categories = new Dictionary<string, CategoryReport>();

			using var sessionHandle = profile.ClaimSessionHandle();
			foreach (var vpResult in sessionHandle.Session.ValidateEverythingEnumeratorBatched()) {
				foreach (var result in vpResult.Explode()) {
					if (result.ResultType is ValidationResultType.IgnoreResult or ValidationResultType.Valid) continue;

					var path = result.DynamicObjectAddress.LatestAddress.AssetPath;
					var category = path.Split('/', '\\').At(1);

					if (!categories.ContainsKey(category)) categories[category] = new CategoryReport(category);
					var report = categories[category];

					var message = $"[{category}] {result.Message} at {path} ({result.ValidatorType.Name})";

					if (result.ResultType == ValidationResultType.Error) {
						report.ReportError(message);
					}
					else {
						report.Log(message);
					}
				}
			}

			if (!Directory.Exists(options.OutputPath)) Directory.CreateDirectory(options.OutputPath);
			
			using (var fs = File.Open(Path.Combine(options.OutputPath, "validation.log"), FileMode.Create)) {
				using (var writer = new StreamWriter(fs)) {
					writer.WriteLine($"Validation Profile: {profile.name}");
					writer.WriteLine($"TOTAL ERRORS: {categories.Values.Sum(report => report.Errors)}");

					foreach (var report in categories.Values) 
						writer.WriteLine(report.ToString());
				}
			}
			// AssetGroups.Reset();
			// AssetGroupsCollector.Reset();
			// AssetGroupsCollector.CollectInvalidAssetsInFolder();
			// AssetGroupsCollector.CreateXmlArtifacts(options.OutputPath);
		}
	}

}