//////////////////////////////////////////////////////////////////////
// Utility Methods for Client
//////////////////////////////////////////////////////////////////////

#pragma warning disable 649 

#addin nuget:?package=Cake.FileHelpers&version=7.0.0

using Path = System.IO.Path;
using SysFile = System.IO.File;

#load const.cake
#load git.cake
#load build-config.cake
#load build-version.cake

(int major, int minor, int patch) DetectUnityProjectVersion() {
	var edtiorVersionStr = FileReadLines(Path.Join(Global.ProjectDir, "ProjectSettings", "ProjectVersion.txt"))[0].Replace("m_EditorVersion: ", "");
	var edtiorVersion = edtiorVersionStr.Split('.', StringSplitOptions.RemoveEmptyEntries);
	var patch = edtiorVersion[2].Split('f')[0];

	return (int.Parse(edtiorVersion[0]), int.Parse(edtiorVersion[1]), int.Parse(patch));
}

string BuildArtefactFileName() {
	switch (Global.Platform) {
		case "Android": return $"{BuildConfig.Var("BundleId")}{(BuildConfig.Var("AppBundle") == "true" ? ".aab" : ".apk")}";
		case "iOS": return BuildConfig.Var("BundleId");
		case "Linux": return BuildConfig.Var("BundleId");
		default: throw new Exception("Unsupported target");
	}
}

string ResultFileName() {
	switch (Global.Platform) {
		case "Android": return BuildArtefactFileName();
		case "iOS": return $"{BuildConfig.Var("BundleId")}.ipa";
		case "Linux": return BuildConfig.Var("BundleId");
		default: throw new Exception("Unsupported target");
	}
}

async Task DumpUnityErrors(string logFile, int retryCount = 2, int timeoutMs = 1000) {
	if (!SysFile.Exists(logFile)) return;

	Regex[] patterns = new[] {
		new Regex(@"Assets\\.*error CS.*$", RegexOptions.Compiled),
		new Regex(@".*fail.*$", RegexOptions.Compiled),
		new Regex(@".*Fail.*$", RegexOptions.Compiled),
		new Regex(@".*FAIL.*$", RegexOptions.Compiled),
		new Regex(@".*error.*$", RegexOptions.Compiled),
		new Regex(@".*Error.*$", RegexOptions.Compiled),
		new Regex(@".*ERROR.*$", RegexOptions.Compiled),
		new Regex(@".*Assembly has duplicate references.*$", RegexOptions.Compiled),
		new Regex(@".*Scene file not found.*$", RegexOptions.Compiled),
	};

	Regex[] antiPatterns = new[] {
		new Regex(@"\[.*feature\/.*\]", RegexOptions.Compiled),
		new Regex(@"LogError.", RegexOptions.Compiled),
		new Regex(@"\[Licensing::IpcConnector\]", RegexOptions.Compiled),
		new Regex(@"LicensingClient", RegexOptions.Compiled),
		new Regex(@"Licensing::Client", RegexOptions.Compiled),
		new Regex(@"Licensing::Module.", RegexOptions.Compiled),
		new Regex(@"SBP Error", RegexOptions.Compiled),
		new Regex(@"Curl error 42: Callback aborted", RegexOptions.Compiled),
		new Regex(@"Assets/Editor/SpineSettings.asset failed", RegexOptions.Compiled),
		new Regex(@"--continue-on-failure", RegexOptions.Compiled),
		new Regex(@"shader.*Hidden", RegexOptions.Compiled),
		new Regex(@"kb.*shader", RegexOptions.Compiled),
		new Regex(@"kb.*\.cs", RegexOptions.Compiled),
		new Regex(@"LogAssemblyErrors", RegexOptions.Compiled),
		new Regex(@"Error.cs", RegexOptions.Compiled),
		new Regex(@"Exception.cs", RegexOptions.Compiled),
		new Regex(@"DumpErrors", RegexOptions.Compiled),
		new Regex(@"'CI_", RegexOptions.Compiled),
		new Regex(@"ReportError", RegexOptions.Compiled),
		new Regex(@"Tundra build failed", RegexOptions.Compiled),
		new Regex(@"abort_threads: Failed aborting id:", RegexOptions.Compiled),
		new Regex(@"Generator 'SerializationGenerator' failed to generate source", RegexOptions.Compiled),
		new Regex(@"\[usbmuxd\]", RegexOptions.Compiled),
		new Regex(@"FF_CMD_DISABLE_DELAYED_ERROR_LEVEL_EXPANSION", RegexOptions.Compiled),
		new Regex(@"DetectErrorsAndWarnings", RegexOptions.Compiled),
		new Regex(@"EntitlementsErrorChecker.OpenFirstEntitlementError", RegexOptions.Compiled),
		new Regex(@"Many text editors can fix this using Convert Line Endings menu commands", RegexOptions.Compiled),
		new Regex(@"FullyQualifiedErrorId", RegexOptions.Compiled),
		new Regex(@"-warnaserror", RegexOptions.Compiled)
	};


	--retryCount;

	string[] logLines = null;
	for (int i = 0; i < retryCount; ++i) {
		try {
			logLines = SysFile.ReadAllLines(logFile, Encoding.UTF8);
			break;
		} 
		catch (Exception ex) {
			Information($"\tError reading log file, retrying. Message={ex.Message}");
			await System.Threading.Tasks.Task.Delay(timeoutMs);
		}
	}

	if (logLines == null) return;

	var errors = logLines
		.Where(x => patterns.Any(pattern => pattern.IsMatch(x)))
		.Where(x => antiPatterns.All(pattern => !pattern.IsMatch(x)))
		.ToArray();

	if (errors.Length > 0) {
		Error($"-------------------------------------------------------");
		Error($"{errors.Length} Potential error line(s) found:");
		foreach (var err in errors) {
			Error($"\t{err}");
		}
		Error($"-------------------------------------------------------");
	}
}

string MakeDstFileName(string targetDir, string fn) => Path.Join(targetDir, $"{Global.Target}-{Global.CustomVersionCode}-{fn}");

