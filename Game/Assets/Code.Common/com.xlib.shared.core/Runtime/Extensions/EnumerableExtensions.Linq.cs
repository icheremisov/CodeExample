using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "CheckNamespace")]
public static partial class EnumerableExtensions {

	/// <summary>
	///     projects sequence and return non-null list
	/// </summary>
	public static List<ResultT> SelectToList<SourceT, ResultT>(this IEnumerable<SourceT> source, Func<SourceT, ResultT> selector) {
		if (source == null) return new List<ResultT>();
		return source.Select(selector).ToList();
	}

	public static ResultT[] SelectToArray<SourceT, ResultT>(this IEnumerable<SourceT> source, Func<SourceT, ResultT> selector) {
		if (source == null) return new ResultT[0];
		return source.Select(selector).ToArray();
	}

	public static List<ResultT> SelectToList<SourceT, ResultT>(this IEnumerable<SourceT> source, Func<SourceT, int, ResultT> selector) {
		if (source == null) return new List<ResultT>();
		return source.Select(selector).ToList();
	}

	public static ResultT[] SelectToArray<SourceT, ResultT>(this IEnumerable<SourceT> source, Func<SourceT, int, ResultT> selector) {
		if (source == null) return new ResultT[0];
		return source.Select(selector).ToArray();
	}

	public static async Task<List<ResultT>> SelectToListAsync<SourceT, ResultT>(this IEnumerable<SourceT> source, Func<SourceT, Task<ResultT>> selector) {
		if (source == null) return new List<ResultT>();

		var tasks = source.Select(selector).ToArray();
		await Task.WhenAll(tasks);

		return tasks.Select(x => x.Result).ToList();
	}

	public static async Task<ResultT[]> SelectToArrayAsync<SourceT, ResultT>(this IEnumerable<SourceT> source, Func<SourceT, Task<ResultT>> selector) {
		if (source == null) return new ResultT[0];

		var tasks = source.Select(selector).ToArray();
		await Task.WhenAll(tasks);

		return tasks.Select(x => x.Result).ToArray();
	}

	/// <summary>
	///     select all not null items from sequence
	/// </summary>
	public static IEnumerable<SourceT> SelectNotNull<SourceT>(this IEnumerable<SourceT> source) where SourceT : class {
		return source?.Where(x => x != null) ?? Enumerable.Empty<SourceT>();
	}

	/// <summary>
	///     select all items with condition and check them for null
	/// </summary>
	public static IEnumerable<ResultT> SelectNotNull<SourceT, ResultT>(this IEnumerable<SourceT> source, Func<SourceT, ResultT> selector) where ResultT : class {
		return source?.Select(selector).Where(x => x != null) ?? Enumerable.Empty<ResultT>();
	}

	public static async Task<List<ResultT>> SelectToListAsync<SourceT, ResultT>(this IEnumerable<SourceT> source, Func<SourceT, int, Task<ResultT>> selector) {
		if (source == null) return new List<ResultT>();

		var tasks = source.Select(selector).ToArray();
		await Task.WhenAll(tasks);

		return tasks.Select(x => x.Result).ToList();
	}

	public static async Task<ResultT[]> SelectToArrayAsync<SourceT, ResultT>(this IEnumerable<SourceT> source, Func<SourceT, int, Task<ResultT>> selector) {
		if (source == null) return new ResultT[0];

		var tasks = source.Select(selector).ToArray();
		await Task.WhenAll(tasks);

		return tasks.Select(x => x.Result).ToArray();
	}

	/// <summary>
	///     project sequence to array, using key as index in array
	/// </summary>
	public static ResultT[] MapToArray<SourceT, ResultT>(this IEnumerable<SourceT> source, int arraySize, Func<SourceT, int> getIndex,
		Func<SourceT, ResultT> selector) {
		var result = new ResultT[arraySize];
		if (source != null) {
			foreach (var item in source) {
				var index = getIndex(item);
				if (!result.IsValidIndex(index)) throw new IndexOutOfRangeException($"{index} is not valid for array [0..{result.Length - 1}]");

				result[index] = selector(item);
			}
		}

		return result;
	}

	public static IEnumerable<ResultT> SelectAs<SourceT, ResultT>(this IEnumerable<SourceT> source, Func<SourceT, ResultT> accessor)
		where ResultT : class {
		return source == null ? Array.Empty<ResultT>() : source.Select(accessor).Where(x => x != null);
	}

	/// <summary>
	///     take last N items from collection preserving theirs order
	/// </summary>
	public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int n) =>
		// ReSharper disable once PossibleMultipleEnumeration
		source.Skip(Math.Max(0, source.Count() - n));

	/// <summary>
	///     take last N items from collection preserving theirs order
	/// </summary>
	public static IEnumerable<T> TakeLast<T>(this IList<T> source, int n) => source.Skip(Math.Max(0, source.Count - n));

	/// <summary>
	///     take last N items from collection preserving theirs order
	/// </summary>
	public static IEnumerable<T> TakeLast<T>(this T[] source, int n) => source.Skip(Math.Max(0, source.Length - n));

	public static void ForEach<T>(this IEnumerable<T> value, Action<T> action) {
		if (value == null) return;

		foreach (var item in value) action(item);
	}

	public static void ForEach<T>(this IEnumerable<T> value, Action<T, int> action) {
		if (value == null) return;

		var index = 0;
		foreach (var item in value) {
			action(item, index);
			++index;
		}
	}

	public static ValueT FirstOrDefault<KeyT, ValueT>(this IDictionary<KeyT, ValueT> obj, KeyT key) =>
		obj != null ? obj.TryGetValue(key, out var result) ? result : default : default;

	/// <summary>
	///     return first or default item of specific type
	/// </summary>
	public static ResultT FirstOrDefaultType<ResultT>(this IEnumerable obj) => obj.OfType<ResultT>().FirstOrDefault();

	/// <summary>
	///     return first or default item of specific type and condition
	/// </summary>
	public static ResultT FirstOrDefaultType<ResultT>(this IEnumerable obj, Func<ResultT, bool> predicate) => obj.OfType<ResultT>().FirstOrDefault(predicate);

	/// <summary>Adds a single element to the end of an IEnumerable.</summary>
	/// <typeparam name="T">Type of enumerable to return.</typeparam>
	/// <returns>
	///     IEnumerable containing all the input elements, followed by the
	///     specified additional element.
	/// </returns>
	public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T element) {
		if (source == null) throw new ArgumentNullException(nameof(source));
		return ConcatIterator(element, source, false);
	}

	/// <summary>Adds a single element to the start of an IEnumerable.</summary>
	/// <typeparam name="T">Type of enumerable to return.</typeparam>
	/// <returns>
	///     IEnumerable containing the specified additional element, followed by
	///     all the input elements.
	/// </returns>
	public static IEnumerable<T> Prepend<T>(this IEnumerable<T> tail, T head) {
		if (tail == null) throw new ArgumentNullException(nameof(tail));
		return ConcatIterator(head, tail, true);
	}

	private static IEnumerable<T> ConcatIterator<T>(T extraElement,
		IEnumerable<T> source, bool insertAtStart) {
		if (insertAtStart) yield return extraElement;
		foreach (var e in source) yield return e;
		if (!insertAtStart) yield return extraElement;
	}

	/// <summary>
	///     remove all items from collection
	/// </summary>
	public static int RemoveAll<T>(this ICollection<T> source, Predicate<T> predicate) {
		if (source is List<T> list) return list.RemoveAll(predicate);

		var count = 0;
		foreach (var item in source.Where(new Func<T, bool>(predicate)).ToArray()) {
			source.Remove(item);
			++count;
		}

		return count;
	}

	public static double Multiply<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) {
		if (source == null) throw new NullReferenceException(nameof(source));

		if (selector == null) throw new NullReferenceException(nameof(selector));

		return source.Aggregate(1.0, (current, item) => current * selector(item));
	}

	public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
		Func<TSource, TKey> selector) =>
		source.MinBy(selector, null);

	public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
		Func<TSource, TKey> selector, IComparer<TKey> comparer) {
		if (source == null) throw new ArgumentNullException("source");

		if (selector == null) throw new ArgumentNullException("selector");

		comparer ??= Comparer<TKey>.Default;

		using var sourceIterator = source.GetEnumerator();
		if (!sourceIterator.MoveNext()) throw new InvalidOperationException("Sequence contains no elements");

		var min = sourceIterator.Current;
		var minKey = selector(min);
		while (sourceIterator.MoveNext()) {
			var candidate = sourceIterator.Current;
			var candidateProjected = selector(candidate);
			if (comparer.Compare(candidateProjected, minKey) >= 0) continue;

			min = candidate;
			minKey = candidateProjected;
		}

		return min;
	}
	
	public static TSource MaxByOrDefault<TSource, TKey>(this IEnumerable<TSource> source,
		Func<TSource, TKey> selector) =>
		source.MaxByOrDefault(selector, null);

	public static TSource MaxByOrDefault<TSource, TKey>(this IEnumerable<TSource> source,
		Func<TSource, TKey> selector, IComparer<TKey> comparer) {
		if (source == null) return default;

		if (selector == null) throw new ArgumentNullException(nameof(selector));

		comparer ??= Comparer<TKey>.Default;

		using var sourceIterator = source.GetEnumerator();
		if (!sourceIterator.MoveNext()) return default;

		var max = sourceIterator.Current;
		var maxKey = selector(max);
		while (sourceIterator.MoveNext()) {
			var candidate = sourceIterator.Current;
			var candidateProjected = selector(candidate);
			if (comparer.Compare(candidateProjected, maxKey) <= 0) continue;

			max = candidate;
			maxKey = candidateProjected;
		}

		return max;
	}
	
	public static IOrderedEnumerable<T> OrderByAlphaNumeric<T>(this IEnumerable<T> source, Func<T, string> selector)
	{
		var max = source
			.SelectMany(i => Regex.Matches(selector(i), @"\d+").Select(m => (int?)m.Value.Length))
			.Max() ?? 0;

		return source.OrderBy(i => Regex.Replace(selector(i), @"\d+", m => m.Value.PadLeft(max, '0')));
	}

}