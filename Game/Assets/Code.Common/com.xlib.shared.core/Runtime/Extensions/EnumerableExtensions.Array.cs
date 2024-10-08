using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using XLib.Core.Runtime.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "CheckNamespace")]
public static partial class EnumerableExtensions {
	/// <summary>
	///     return true if sequence is found at array[offset]
	/// </summary>
	public static bool IsMatch<T>(this T[] array, T[] sequence, int arrayOffset = 0) where T : IComparable {
		if (array.IsNullOrEmpty()) return false;

		if (sequence.IsNullOrEmpty()) return false;

		var c = sequence.Length;
		if (arrayOffset + c > array.Length) return false;

		for (var i = 0; i < c; i++) {
			if (array[arrayOffset + i].CompareTo(sequence[i]) != 0) return false;
		}

		return true;
	}

	/// <summary>
	///     extend array for hold at least 'atLeastCount' elements
	/// </summary>
	public static void ExtendArray<ValueT>(ref ValueT[] items, int atLeastCount) {
		if (items == null)
			items = new ValueT[atLeastCount];
		else if (items.Length < atLeastCount) Array.Resize(ref items, atLeastCount);
	}

	/// <summary>
	///     set size of array to maxCount
	/// </summary>
	public static void FixSize<ValueT>(ref ValueT[] items, int maxCount) {
		if (items == null)
			items = new ValueT[maxCount];
		else if (items.Length != maxCount) Array.Resize(ref items, maxCount);
	}

	/// <summary>
	///     set size of array to maxCount
	/// </summary>
	public static ValueT[] FixSize<ValueT>(ValueT[] items, int maxCount) {
		if (items == null)
			items = new ValueT[maxCount];
		else if (items.Length != maxCount) Array.Resize(ref items, maxCount);

		return items;
	}

	/// <summary>
	///     resize or create array
	/// </summary>
	public static T[] Resize<T>(this T[] oldArray, int size) {
		if (oldArray == null) return new T[size];

		Array.Resize(ref oldArray, size);
		return oldArray;
	}

	/// <summary>
	///     return range of items from array
	/// </summary>
	public static T[] GetRange<T>(this T[] obj, int startIndex, int length) {
		if (obj.IsNullOrEmpty()) return Array.Empty<T>();

		var result = new T[length];
		Array.Copy(obj, startIndex, result, 0, length);
		return result;
	}

	public static bool ArraysEqual<T>(T[] a1, T[] a2) {
		if (ReferenceEquals(a1, a2)) return true;

		if (a1 == null || a2 == null) return false;

		if (a1.Length != a2.Length) return false;

		var comparer = EqualityComparer<T>.Default;
		for (var i = 0; i < a1.Length; i++) {
			if (!comparer.Equals(a1[i], a2[i])) return false;
		}

		return true;
	}

	public static int[] SplitIntArray(this string sNumbers, char separator = ',') {
		if (string.IsNullOrEmpty(sNumbers)) return new int[0];

		try {
			return sNumbers.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
		}
		catch (Exception) {
			return new int[0];
		}
	}

	public static float[] SplitFloatArray(this string sNumbers, char separator = ',') {
		if (string.IsNullOrEmpty(sNumbers)) return new float[0];

		try {
			return sNumbers.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => float.Parse(x.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture))
				.ToArray();
		}
		catch (Exception) {
			return new float[0];
		}
	}

	public static long[] SplitLongArray(this string sNumbers, char separator = ',') {
		if (string.IsNullOrEmpty(sNumbers)) return new long[0];

		try {
			return sNumbers.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
		}
		catch (Exception) {
			return new long[0];
		}
	}

	/// <summary>
	///     fill entire array with specific value
	/// </summary>
	public static T[] Populate<T>(this T[] arr, T value) {
		var c = arr.Length;
		for (var i = 0; i < c; i++) arr[i] = value;

		return arr;
	}

	/// <summary>
	///     create new array and set each item to fillValue
	/// </summary>
	public static T[] CreateArray<T>(int len, T fillValue) where T : struct {
		var result = new T[len];
		for (var i = 0; i < len; i++) result[i] = fillValue;

		return result;
	}

	/// <summary>
	///     create new array and set each item to fillValue() result
	/// </summary>
	public static T[] CreateArray<T>(int len, Func<T> defaultCreator) where T : class {
		var result = new T[len];
		for (var i = 0; i < len; i++) result[i] = defaultCreator();

		return result;
	}

	public static void Sort<TSource, TKey>(this TSource[] array, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey> {
		if (array.IsNullOrEmpty()) return;

		int Comparison(TSource x, TSource y) => keySelector(x).CompareTo(keySelector(y));

		Array.Sort(array, Comparison);
	}

	public static void Sort<TSource>(this TSource[] array, int count, Comparison<TSource> comparer) {
		if (array.IsNullOrEmpty()) return;

		Array.Sort(array, 0, count, new LambdaComparer<TSource>(comparer));
	}

	public static int IndexOf<T>(this T[] array, T item) {
		if (array.IsNullOrEmpty()) return -1;

		return Array.IndexOf(array, item);
	}

	public static IEnumerable<TTo> SelectWhere<TFrom, TTo>(this IEnumerable<TFrom> from, Func<TFrom, (bool, TTo)> selector) {
		using var enumerator = from.GetEnumerator();
		for (; enumerator.MoveNext();) {
			(var where, var select) = selector(enumerator.Current);
			if (where) yield return select;
		}
	}

	public static IEnumerable<TTo> SelectWhere<TFrom, TTo>(this IEnumerable<TFrom> from, Func<TFrom, int, (bool, TTo)> selector) {
		using var enumerator = from.GetEnumerator();
		var i = 0;
		for (; enumerator.MoveNext();) {
			(var where, var select) = selector(enumerator.Current, i++);
			if (where) yield return select;
		}
	}
}