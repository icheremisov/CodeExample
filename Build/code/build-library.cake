//////////////////////////////////////////////////////////////////////
// Utility Methods for Library folder
//////////////////////////////////////////////////////////////////////

#pragma warning disable 649 

using Path = System.IO.Path;
using SysDirectory = System.IO.Directory;
using SysFile = System.IO.File;

#addin nuget:?package=Cake.Unity&version=0.9.0

#load const.cake
#load git.cake
#load utils.cake
#load client-common.cake
#load build-config.cake
#load build-version.cake

Task("Build-Library-Remove").Does(() =>
{
	if (string.IsNullOrEmpty(Global.LibraryCacheDir)) {
		Information($"LibraryCacheDir does not set - library removing disabled!");
		return;
	}

	if (Argument("keepLibrary", 0) == 1 || BuildConfig.Var("keepLibrary", "").Trim() == "1") {
		Information("keepLibrary=1: Skip library removal");
		return;
	}

	var libraryFolder = Path.Join(Global.ProjectDir, "Library");
	if (SysDirectory.Exists(libraryFolder)) SysDirectory.Delete(libraryFolder, true);
});

Task("Build-Library-ClearAssembles").Does(async () =>
{
	var libraryFolder = Path.Join(Global.ProjectDir, "Library", "ScriptAssemblies");
	await RecreateDirectory(libraryFolder); 	
});


Task("Build-Library-LinkCache").Does(() =>
{
	if (string.IsNullOrEmpty(Global.LibraryCacheDir)) {
		Information($"LibraryCacheDir does not set - library cache disabled!");
		return;
	}

	(var major, var minor, var patch) = DetectUnityProjectVersion();
	var unity = $"{major}{minor}-{patch}";

	var branch = GetGitBranchName();
	if (branch.Contains("feature/")) branch = "feature";
	else if (branch.Contains("release/") || branch.Contains("master") || branch.Contains("version-")) branch = "release";
	else if (branch.Contains("develop")) branch = "develop";
	else throw new Exception($"Unsupported branch name '{branch}'");

	var ver = BuildVersion.VersionString.Replace(".", "-");

	var target = Global.Target.ToLowerInvariant();

	var srcDir = Path.Combine(Global.LibraryCacheDir, $"{unity}-{branch}-{ver}-{target}");

	if (srcDir.StartsWith("~/")) srcDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), srcDir[1..]);
	Information($"LibraryCacheDir={srcDir}");

	if (!SysDirectory.Exists(srcDir)) SysDirectory.CreateDirectory(srcDir);

	var libraryFolder = Path.Join(Global.ProjectDir, "Library");
	if (SysDirectory.Exists(libraryFolder)) SysDirectory.Delete(libraryFolder, true);
	
	CreateFolderSymlink(libraryFolder, srcDir);
});

