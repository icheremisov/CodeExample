namespace XLib.Core.RandGen {

	public interface IRandom {

		int Next(int min, int maxExclusive);
		int Next(int maxExclusive);
		double NextDouble();

	}

}