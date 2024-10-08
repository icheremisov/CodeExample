//////////////////////////////////////////////////////////////////////
// GIT Unitilies
//////////////////////////////////////////////////////////////////////

#load const.cake
using Path = System.IO.Path;

string GetGitBranchName() {
    if (BuildSystem.GitLabCI.IsRunningOnGitLabCI) return BuildSystem.GitLabCI.Environment.Build.RefName;
	else {
        var gitRoot = Path.GetFullPath(Path.Combine(Global.RootDir, ".git"));
        return !System.IO.Directory.Exists(gitRoot) ? string.Empty : System.IO.File.ReadAllText($"{gitRoot}/HEAD").Split(':').Last().Trim().Replace("ref: ", "").Replace("refs/heads/", "");
    }
}