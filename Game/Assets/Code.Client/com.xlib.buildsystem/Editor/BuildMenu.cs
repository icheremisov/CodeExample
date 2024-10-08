using UnityEditor;

namespace XLib.BuildSystem {

	public static class BuildMenu {

		[MenuItem("Build/Change Game Version Code", false, 350)]
		public static void ChangeGameVersionCode() {
			VersionWindow.ShowWindow();
		}

		[MenuItem("Build/Run Build", false, 365)]
		public static void Build() {
			RunBuildWindow.ShowWindow();
		}

	}

}