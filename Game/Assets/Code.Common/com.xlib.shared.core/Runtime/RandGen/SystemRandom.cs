using System;

namespace XLib.Core.RandGen {

	public class SystemRandom : IRandom {
		
		private readonly Random _random;

		public SystemRandom() {
			_random = new Random();
		}
		
		public SystemRandom(int seed) {
			_random = new Random(seed);
		}

		public int Next(int min, int maxExclusive) => _random.Next(min, maxExclusive);

		public int Next(int maxExclusive) => _random.Next(maxExclusive);

		public double NextDouble() => _random.NextDouble();
	}

}