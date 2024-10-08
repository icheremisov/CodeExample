#if UNITY_EDITOR

using XLib.BuildSystem.Types;

namespace XLib.BuildSystem {

	public interface IBeforeBuildRunner {

		// call priority - lower is earlier. Default priority is 0
		int Priority { get; }

		// called before build system started
		void OnBeforeBuild(BuildRunnerOptions options, RunnerReport report);

	}

}

#endif