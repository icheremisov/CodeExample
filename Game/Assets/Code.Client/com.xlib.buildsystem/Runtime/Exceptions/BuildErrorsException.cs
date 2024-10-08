#if UNITY_EDITOR

using System;
using XLib.BuildSystem.Types;

namespace XLib.BuildSystem.Exceptions {

	public class BuildErrorsException : Exception {
		public BuildErrorsException(RunnerReport report) : base(report.GetErrorString()) {
		}
	}

}

#endif