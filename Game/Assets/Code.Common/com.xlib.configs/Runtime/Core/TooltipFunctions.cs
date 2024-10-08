using System;
using Newtonsoft.Json;
using XLib.Core.Utils;

namespace XLib.Configs.Core {

	public class ConfigExportInfo {
		public string Branch { get; set; }
		public string LastExportTime { get; set; }
	}

	public static class TooltipFunctions {
		public static string SetDateTimeAndBranch() {
			var info = new ConfigExportInfo {
				Branch = GitInfoUtils.GetGitBranch(), LastExportTime = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")
			};
			var branchInfo = JsonConvert.SerializeObject(info);
			return branchInfo;
		}
	}

}