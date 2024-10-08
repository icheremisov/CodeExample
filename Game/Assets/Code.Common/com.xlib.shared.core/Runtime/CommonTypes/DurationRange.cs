using System;
using XLib.Core.RandGen;

namespace XLib.Core.CommonTypes {

	[Serializable]
	public class DurationRange {
		public Duration min;
		public Duration max;
		
		public Duration Delta => max - min;
		
		public DurationRange() {}
		public DurationRange(int min, int max) {
			this.min = new Duration(min);
			this.max = new Duration(max);
		}
		public DurationRange(Duration min, Duration max) {
			this.min = min;
			this.max = max;
		}
		
		public Duration GetRandom(IRandom random) {
			return min + Delta * random.NextInclusive(0.0f, 1.0f);
		}
	}

}