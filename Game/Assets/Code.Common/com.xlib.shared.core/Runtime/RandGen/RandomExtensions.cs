using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using XLib.Core.RandGen;
using XLib.Core.Utils;

[SuppressMessage("ReSharper", "CheckNamespace")]
public static class RandomExtensions {
	/// <summary>
	///     return [0..1] - inclusive
	/// </summary>
	public static float RandomK(this IRandom rand) => (float)Math.Round(rand.NextDouble(), 8);

	/// <summary>
	///     return [0..100] - inclusive
	/// </summary>
	public static float RandomPercent(this IRandom rand) => rand.RandomK() * 100.0f;

	/// <summary>
	///     return [min..max] - inclusive
	/// </summary>
	public static int NextInclusive(this IRandom rand, int min, int max) {
		if (min == max) return min;

		if (min > max) MathEx.Swap(ref min, ref max);

		return MathEx.RoundToInt(MathEx.Lerp(min, max, rand.RandomK()));
	}
	
	public static int NextInclusive(this IRandom rand, RangeInt range) {
		if (range.start == range.end) return range.start;
		return MathEx.RoundToInt(MathEx.Lerp(range.start, range.end, rand.RandomK()));
	}

	public static int NextInclusive(this IRandom rand, XLib.Core.CommonTypes.Range range) {
		return range.min == range.max ? range.min : MathEx.RoundToInt(MathEx.Lerp(range.min, range.max, rand.RandomK()));
	}

	/// <summary>
	///     return [min..max] - inclusive
	/// </summary>
	public static long NextInclusive(this IRandom rand, long min, long max) {
		if (min == max) return min;

		if (min > max) MathEx.Swap(ref min, ref max);

		return MathEx.RoundToLong(MathEx.Lerp(min, max, rand.RandomK()));
	}

	/// <summary>
	///     return random number [min, max] with minimal step between values. for ex. [10, 100] step=10 returns 10, 20, 50, ...
	/// </summary>
	public static int NextInclusive(this IRandom rand, int min, int max, int step) {
		if (min == max) return min;

		if (step <= 1) return NextInclusive(rand, min, max);

		if (min > max) MathEx.Swap(ref min, ref max);

		min /= step;
		max /= step;

		return MathEx.RoundToInt(MathEx.Lerp(min, max, rand.RandomK()) * step);
	}

	/// <summary>
	///     return [min..max] - inclusive
	/// </summary>
	public static float NextInclusive(this IRandom rand, float min, float max) {
		if (min > max) MathEx.Swap(ref min, ref max);

		return rand.RandomK() * (max - min) + min;
	}

	/// <summary>
	///     return [min..max] - inclusive
	/// </summary>
	public static double NextInclusive(this IRandom rand, double min, double max) {
		if (min > max) MathEx.Swap(ref min, ref max);

		return rand.RandomK() * (max - min) + min;
	}

	public static IEnumerable<T> RandomItems<T>(this IRandom rand, IReadOnlyList<T> obj, int count) => obj.RandomItems(count, rand);
	public static T RandomItem<T>(this IRandom rand, IReadOnlyList<T> list) => list.RandomItem(rand);
}