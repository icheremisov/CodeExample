#if UNITY_EDITOR

using XLib.BuildSystem.Types;

namespace XLib.BuildSystem {

	public interface IAfterBuildRunner {

		// call priority - lower is earlier. Default priority is 0
		int Priority { get; }

		// called before build system started
		void OnAfterBuild(BuildRunnerOptions options, RunnerReport report);

	}

}

#endif