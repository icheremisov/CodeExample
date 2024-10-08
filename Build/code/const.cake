//////////////////////////////////////////////////////////////////////
// Global Constants
//////////////////////////////////////////////////////////////////////

using Path = System.IO.Path;

static class Global {
    public static ICakeContext Context;

    public static string RootDir => Path.Join("..");
    public static string ProjectDir => Path.Join(RootDir, "Game");
    public static string ConfigDir => "config";

    public static string StagingDir => Path.GetFullPath(Path.Join(RootDir, "_Staging"));
    public static string ArtefactsDir => Path.GetFullPath(Path.Join(RootDir, "_Output"));
    public static string Target;
    public static string Platform;
    public static long BuildSize = 0;

    public static string LibraryCacheDir;

    public static int CustomVersionCode;

    public static class Discord
    {
        public const string WebHookUrl = "Discord.WebHookUrl";
    }
}

Global.Context = Context;
Global.CustomVersionCode = Argument<int>("versionCode", -1);

Global.Target = Argument("target", "internal");

Global.Platform = Argument<string>("platform");
Global.LibraryCacheDir = Argument<string>("LibraryCacheDir", "");
if (string.IsNullOrEmpty(Global.LibraryCacheDir)) Global.LibraryCacheDir = EnvironmentVariable<string>("LibraryCacheDir", "");

if (Global.CustomVersionCode < 0) {
    if (BuildSystem.GitLabCI.IsRunningOnGitLabCI) {
        Global.CustomVersionCode = BuildSystem.GitLabCI.Environment.Build.PipelineIId;
    } else {
        Global.CustomVersionCode = 1;
    }
}
