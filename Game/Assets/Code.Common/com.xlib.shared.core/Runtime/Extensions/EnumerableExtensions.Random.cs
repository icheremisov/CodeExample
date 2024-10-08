using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using XLib.Core.RandGen;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "CheckNamespace")]
public static partial class EnumerableExtensions {

	public static int RandomIndex<T>(this IList<T> obj, IRandom rand) {
		if (obj.IsNullOrEmpty()) return -1;
		var c = obj.Count;
		return c == 1 ? 0 : c > 0 ? rand.Next(c) : -1;
	}

	public static T RandomItem<T>(this IReadOnlyList<T> obj, IRandom rand) {
		if (obj.IsNullOrEmpty()) return default;
		var c = obj.Count;
		return c == 1 ? obj[0] : c > 0 ? obj[rand.Next(c)] : default;
	}
	
	public static T RandomItem<T>(this IReadOnlyList<T> obj, int seed) {
		if (obj.IsNullOrEmpty()) return default;
		var c = obj.Count;
		return c > 0 ? obj[seed % c] : default;
	}

	public static IEnumerable<T> RandomItems<T>(this IReadOnlyList<T> obj, int count, IRandom rand) {
		if (obj.IsNullOrEmpty()) return Enumerable.Empty<T>();

		var c = obj.Count;
		if (c == 0) return Enumerable.Empty<T>();

		count = Math.Min(c, count);

		var result = new List<T>(obj);
		result.PartialShuffle(count, rand);
		return result.Take(count);
	}

	public static T GetWeightedRandom<T>(this IList<T> obj, Func<T, int, float> getWeight, IRandom random, bool vbLog = false) {
		if (obj.IsNullOrEmpty()) throw new Exception($"Couldn't retrieve a weighted random value. {obj} is empty!");

		var c = obj.Count;
		var sum = 0.0f;

		for (var index = 0; index < c; index++) {
			var value = obj[index];
			sum += getWeight(value, index);
		}

		var randomNum = random.NextInclusive(0, sum);

		if (vbLog) Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(new Color(0.51f, 1f, 0.05f))}>[VB] Random weight {randomNum}</color>");

		for (var index = 0; index < c; index++) {
			var value = obj[index];
			var weight = getWeight(value, index);
			if (randomNum < weight) return value;

			randomNum -= weight;
		}

		return obj.Last();
	}

	/// <summary>
	///     shuffle first N items in array
	/// </summary>
	public static void PartialShuffle<T>(this IList<T> source, int count, IRandom random) {

		if (source == null || source.Count <= 1) return; 
		
		count = Math.Min(count, source.Count);

		for (var i = 0; i < count; i++) {
			var index = i + random.Next(source.Count - i);
			(source[index], source[i]) = (source[i], source[index]);
		}
	}

	/// <summary>
	///     shuffle all items in array
	/// </summary>
	public static void Shuffle<T>(this IList<T> source, IRandom random) {
		if (source == null || source.Count <= 1) return; 
		
		for (var i = 0; i < source.Count; i++) {
			var index = i + random.Next(source.Count - i);
			(source[index], source[i]) = (source[i], source[index]);
		}
	}

	public static IEnumerable<T> RandomItems<T>(this IList<T> obj, int count, IRandom random) {
		if (obj.IsNullOrEmpty()) return Enumerable.Empty<T>();

		var c = obj.Count;
		if (c == 0) return Enumerable.Empty<T>();

		count = Math.Min(c, count);

		var result = new List<T>(obj);
		result.PartialShuffle(count, random);
		return result.Take(count);
	}
}