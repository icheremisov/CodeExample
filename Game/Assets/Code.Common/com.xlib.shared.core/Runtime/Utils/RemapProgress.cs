using System;
using XLib.Core.CommonTypes;
using XLib.Core.Utils;

namespace XLib.Core.Utils {

	public class RemapProgress : IProgress<float> {

		private readonly IProgress<float> _progress;
		private RangeF _range;

		public RemapProgress(RangeF range, IProgress<float> progress) {
			_range = range;
			_progress = progress;
		}

		public void Report(float value) {
			_progress?.Report(_range.Lerp(value));
		}

	}

}

public static class RemapProgressExtensions {

	public static RemapProgress Remap(this IProgress<float> src, float from, float to) => new(new RangeF(from, to), src);

}