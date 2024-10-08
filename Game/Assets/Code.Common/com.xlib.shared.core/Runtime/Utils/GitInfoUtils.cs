using System.IO;
using System.Linq;

namespace XLib.Core.Utils {

	public static class GitInfoUtils {
		private static readonly string Root = Path.GetFullPath(Path.Combine(UnityAppConstants.dataPath, "..", ".."));

		public static string GetGitBranch()
		{
			var gitRoot = Path.GetFullPath(Path.Combine(Root, ".git"));
			return !Directory.Exists(gitRoot) ? string.Empty : File.ReadAllText($"{gitRoot}/HEAD").Split(':').Last().Trim().Replace("ref: ", "").Replace("refs/heads/", "");
		}

	}

}