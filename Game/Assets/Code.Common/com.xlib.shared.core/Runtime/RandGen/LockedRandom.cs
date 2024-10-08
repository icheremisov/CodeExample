namespace XLib.Core.RandGen {

	public class LockedRandom : IRandom {
		public static LockedRandom Default => new(0);

		private readonly int _seed = 0;

		private LockedRandom() { }

		public LockedRandom(int seed) => _seed = seed;

		public int Next(int min, int maxExclusive) => min + _seed % (maxExclusive - min);

		public int Next(int maxExclusive) => _seed % maxExclusive;

		public double NextDouble() => _seed;
	}

}