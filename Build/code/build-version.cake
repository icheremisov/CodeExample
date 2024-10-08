//////////////////////////////////////////////////////////////////////
// Game Version
//////////////////////////////////////////////////////////////////////

#addin nuget:?package=Cake.Json&version=7.0.1

#load const.cake
#load git.cake
#load build-config.cake

using Path = System.IO.Path;

static class BuildVersion {

    private class VersionStorage {
        public int versionCode;
        public string versionString;
        public string description;
        public string env;
        public string branch;

    }

    private static string VersionSrcFile => Path.GetFullPath(Path.Join(Global.RootDir, "version.txt")); 

    private static string VersionPath => Path.GetFullPath(Path.Join(Global.ProjectDir, "Assets/Resources/version.json")); 
    public static string VersionString { get => _storage.versionString; set => _storage.versionString = value; }
    public static int VersionCode { get => _storage.versionCode; set => _storage.versionCode = value; }
    public static string EnvName { get => _storage.env; set => _storage.env = value; }

    static VersionStorage _storage;

    public static string GetFullVersionString() {
        var result = $"{_storage.versionString} ({_storage.versionCode})";
        if (!string.IsNullOrEmpty(_storage.description)) result += $" {_storage.description}";
        return result;
    } 

    public static void Load() {
        Global.Context.Information("Loading version from storage");
        if (!System.IO.File.Exists(VersionSrcFile)) Global.Context.Error($"Cannot read version from '{VersionSrcFile}'");

        _storage = Global.Context.DeserializeJsonFromFile<VersionStorage>(VersionPath);
        _storage.description = "";
        _storage.versionString = System.IO.File.ReadAllLines(VersionSrcFile).First().Trim();
        if (string.IsNullOrEmpty( _storage.versionString)) Global.Context.Error($"Cannot read version from '{VersionSrcFile}'");

    	var target = Global.Target.ToLowerInvariant();
        if (target == "staging") _storage.description += "[staging]";
        else if (target == "internal") _storage.description += "[dev]";
    }

    public static void Save() {
        Global.Context.Information($"Saving version to storage: {GetFullVersionString()}; env={EnvName}");
        Global.Context.SerializeJsonToFile(VersionPath, _storage);
    }

}


BuildVersion.Load();

BuildVersion.EnvName = GetGitBranchName();
BuildVersion.VersionCode = Global.CustomVersionCode;