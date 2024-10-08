using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.IL2CPP.CompilerServices;
using XLib.Core.Collections;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "CheckNamespace")]
public static partial class EnumerableExtensions {
	/// <summary>
	///     check if index is valid
	/// </summary>
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValidIndexList<SourceT>(this IList<SourceT> source, int index) => source != null && index >= 0 && index < source.Count;

	/// <summary>
	///     check if index is valid
	/// </summary>
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValidIndex<SourceT>(this IReadOnlyList<SourceT> source, int index) => source != null && index >= 0 && index < source.Count;

	/// <summary>
	///     check if index is valid
	/// </summary>
	public static bool IsValidIndex<SourceT>(this SourceT[] source, int index) => source != null && index >= 0 && index < source.Length;

	// /// <summary>
	// ///     return index in range [0..source.Length - 1]
	// /// </summary>
	// public static int ClampIndex<SourceT>(this IList<SourceT> source, int index) {
	// 	if (source == null) return index;
	// 	if (index < 0) return 0;
	// 	if (index >= source.Count) return source.Count - 1;
	// 	return index;
	// }

	/// <summary>
	///     return index in range [0..source.Length - 1]
	/// </summary>
	public static int ClampIndex<SourceT>(this IReadOnlyList<SourceT> source, int index) {
		if (source == null) return index;
		if (index < 0) return 0;
		if (index >= source.Count) return source.Count - 1;
		return index;
	}

	/// <summary>
	///     return index in range [0..source.Length - 1]
	/// </summary>
	public static int ClampIndex<SourceT>(this SourceT[] source, int index) {
		if (source == null) return index;
		if (index < 0) return 0;
		if (index >= source.Length) return source.Length - 1;
		return index;
	}

	/// <summary>
	///     return value, contained in dictionary, or create, or add new value
	/// </summary>
	public static ValueT GetOrAddValue<KeyT, ValueT>(this IDictionary<KeyT, ValueT> obj, KeyT key) where ValueT : class, new() {
		if (obj.TryGetValue(key, out var result)) return result;

		result = new ValueT();

		obj.Add(key, result);

		return result;
	}

	/// <summary>
	///     return value, contained in dictionary, or create, or add new value
	/// </summary>
	public static ValueT GetOrAddValue<KeyT, ValueT>(this IDictionary<KeyT, ValueT> obj, KeyT key, Func<ValueT> creator) where ValueT : class {
		if (obj.TryGetValue(key, out var result)) return result;

		result = creator();

		obj.Add(key, result);

		return result;
	}

	/// <summary>
	///     return value, contained in dictionary, or create, or add new value
	/// </summary>
	public static ValueT GetOrAddValue<KeyT, ValueT>(this ConcurrentDictionary<KeyT, ValueT> obj, KeyT key, Func<ValueT> creator) where ValueT : class {
		if (obj.TryGetValue(key, out var result)) return result;

		result = creator();

		return obj.TryAdd(key, result) ? result : obj[key];
	}

	/// <summary>
	///     return value, contained in dictionary, or create, or add new value
	/// </summary>
	public static ValueT GetOrAddValue<ValueT>(this ICollection<ValueT> obj, Func<ValueT, bool> selector) where ValueT : class, new() {
		var result = obj.FirstOrDefault(selector);
		if (result == null) {
			result = new ValueT();
			obj.Add(result);
		}

		return result;
	}

	public static ValueT GetOrAddValue<ValueT>(this ICollection<ValueT> obj, Func<ValueT, bool> selector, Func<ValueT> creator) where ValueT : class {
		var result = obj.FirstOrDefault(selector);
		if (result == null) {
			result = creator();
			obj.Add(result);
		}

		return result;
	}

	/// <summary>
	///     get value by index or return default if index is invalid
	/// </summary>
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ValueT GetOrDefault<ValueT>(this IReadOnlyList<ValueT> obj, int index, ValueT defValue = default) => obj.IsValidIndex(index) ? obj[index] : defValue;

	/// <summary>
	///     get value by index or return default if index is invalid
	/// </summary>
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ValueT GetOrDefaultList<ValueT>(this IList<ValueT> obj, int index, ValueT defValue = default) => obj.IsValidIndexList(index) ? obj[index] : defValue;

	/// <summary>
	///     return value by clamped index or return default if source is empty
	/// </summary>
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SourceT At<SourceT>(this SourceT[] source, int index) => source is not { Length: > 0 } ? default : source[source.ClampIndex(index)];

	// /// <summary>
	// ///     return value by clamped index or return default if source is empty
	// /// </summary>
	// [Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	// public static SourceT At<SourceT>(this IList<SourceT> source, int index) => source is not { Count: > 0 } ? default : source[source.ClampIndex(index)];

	/// <summary>
	///     return value by clamped index or return default if source is empty
	/// </summary>
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false), MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SourceT At<SourceT>(this IReadOnlyList<SourceT> source, int index) => source is not { Count: > 0 } ? default : source[source.ClampIndex(index)];

	/// <summary>
	///     add to list only unique items
	/// </summary>
	public static ISet<T> AddOnce<T>(this ISet<T> obj, T item) {
		if (!obj.Contains(item)) obj.Add(item);

		return obj;
	}

	/// <summary>
	///     add to list only unique items
	/// </summary>
	public static ISet<T> AddOnce<T>(this ISet<T> obj, IEnumerable<T> items) {
		foreach (var item in items) obj.AddOnce(item);

		return obj;
	}

	/// <summary>
	///     add to list only unique items
	/// </summary>
	public static IList<T> AddOnce<T>(this IList<T> obj, T item) {
		if (obj.IndexOf(item) < 0) obj.Add(item);

		return obj;
	}

	/// <summary>
	///     add to list only unique items from other List
	/// </summary>
	public static IList<T> AddOnce<T>(this IList<T> obj, IEnumerable<T> other) {
		if (other == null) return obj;

		foreach (var t in other) obj.AddOnce(t);

		return obj;
	}

	public static T RemoveFirst<T>(this IList<T> obj) {
		if (obj.Count == 0) throw new IndexOutOfRangeException("List is empty!");

		var result = obj[0];
		obj.RemoveAt(0);

		return result;
	}

	public static T RemoveFirst<T>(this IList<T> obj, Func<T, bool> predicate) {
		if (obj.Count == 0) throw new IndexOutOfRangeException("List is empty!");

		for (var i = 0; i < obj.Count; i++) {
			var result = obj[i];
			if (predicate(result)) {
				obj.RemoveAt(i);
				return result;
			}
		}

		return default;
	}

	public static T RemoveLast<T>(this IList<T> obj) {
		var c = obj.Count;
		if (c == 0) throw new IndexOutOfRangeException("List is empty!");

		var result = obj[c - 1];
		obj.RemoveAt(c - 1);

		return result;
	}

	public static T RemoveLast<T>(this IList<T> obj, Func<T, bool> predicate) {
		if (obj.Count == 0) throw new IndexOutOfRangeException("List is empty!");

		for (var i = obj.Count - 1; i >= 0; i--) {
			var result = obj[i];
			if (predicate(result)) {
				obj.RemoveAt(i);
				return result;
			}
		}

		return default;
	}

	public static void Swap<T>(this IList<T> obj, int i1, int i2) {
		(obj[i1], obj[i2]) = (obj[i2], obj[i1]);
	}

	[ContractAnnotation("obj:null => true")]
	public static bool IsNullOrEmpty<K, T>(this Dictionary<K, T> obj) => obj == null || obj.Count == 0;

	[ContractAnnotation("obj:null => true")]
	public static bool IsNullOrEmpty<T>(this ICollection<T> obj) => obj == null || obj.Count == 0;

	[ContractAnnotation("obj:null => true")]
	public static bool IsNullOrEmpty<T>(this T[] obj) => obj == null || obj.Length == 0;

	[ContractAnnotation("obj:null => true")]
	public static bool IsNullOrEmpty<T>(this IEnumerable<T> obj) => obj == null || !obj.Any();

	[ContractAnnotation("obj:notnull=>true")]
	public static bool NotEmpty<K, T>(this Dictionary<K, T> obj) => obj != null && obj.Count > 0;

	[ContractAnnotation("obj:notnull=>true")]
	public static bool NotEmpty<T>(this ICollection<T> obj) => obj != null && obj.Count > 0;

	[ContractAnnotation("obj:notnull=>true")]
	public static bool NotEmpty<T>(this T[] obj) => obj != null && obj.Length > 0;

	/// <summary>
	///     return index of specific item or -1, if not found
	/// </summary>
	public static int LastIndexOf<T>(this IList<T> obj, Func<T, bool> predicate) {
		if (obj.IsNullOrEmpty()) return -1;

		var c = obj.Count;
		for (var i = c - 1; i >= 0; i--)
			if (predicate(obj[i]))
				return i;

		return -1;
	}

	/// <summary>
	///     return index of specific item or -1, if not found
	/// </summary>
	public static int IndexOf<T>(this IEnumerable<T> obj, Func<T, bool> predicate) {
		if (obj.IsNullOrEmpty()) return -1;

		var index = 0;
		foreach (var item in obj) {
			if (predicate(item)) return index;

			++index;
		}

		return -1;
	}

	public static string JoinToString<T>(this T[] array, string separator = ",") {
		return array != null && array.Length > 0 ? string.Join(separator, array.Select(x => x?.ToString())) : string.Empty;
	}

	public static string JoinToString<T>(this IEnumerable<T> array, string separator = ",") {
		return array != null ? string.Join(separator, array.Select(x => x?.ToString())) : string.Empty;
	}

	public static string JoinToString<T>(this T[] array, char separator) {
		return array != null && array.Length > 0 ? string.Join(separator, array.Select(x => x?.ToString())) : string.Empty;
	}

	public static string JoinToString<T>(this IEnumerable<T> array, char separator) {
		return array != null ? string.Join(separator, array.Select(x => x?.ToString())) : string.Empty;
	}

	/// <summary>
	///     dump types of instances (for debugging)
	/// </summary>
	public static string DumpTypes<T>(this IEnumerable<T> array) {
		return "[" + (array != null ? string.Join(",", array.SelectToArray(x => x.GetType().Name)) : string.Empty) + "]";
	}

	public static bool TryDequeue<T>(this Queue<T> queue, out T result) {
		if (queue.Count == 0) {
			result = default;
			return false;
		}

		result = queue.Dequeue();
		return true;
	}

	/// <summary>Adds a collection to a hashset.</summary>
	/// <param name="hashSet">The hashset.</param>
	/// <param name="range">The collection.</param>
	public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range) {
		foreach (var obj in range) hashSet.Add(obj);
	}

	/// <summary>Convert a colletion to a HashSet.</summary>
	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => source != null ? new HashSet<T>(source) : new HashSet<T>();
	public static HashSet<TKey> ToHashSet<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) => source != null ? new HashSet<TKey>(source.Select(keySelector)) : new HashSet<TKey>();

	/// <summary>Convert a colletion to a HashSet.</summary>
	public static HashSet<T> ToHashSet<T>(
		this IEnumerable<T> source,
		IEqualityComparer<T> comparer) =>
		source != null ? new HashSet<T>(source, comparer) : new HashSet<T>(comparer);

	public static IEnumerable<T> ToEnumerableIfNotNull<T>(this T element) => element == null ? Enumerable.Empty<T>() : new Single<T>(element);
	public static IEnumerable<T> ToEnumerable<T>(this T element) => new Single<T>(element);
	
	public static IEnumerable<T> AsSingle<T>(this T element) => new Single<T>(element);
	public static T[] AsArray<T>(this T element) => new []{element};

	private struct Single<T> : IEnumerable<T>, IEnumerator<T> {
		private bool _stepped;

		public Single(T element, bool none = false) {
			Current = element;
			_stepped = none;
		}

		public IEnumerator<T> GetEnumerator() => this;
		IEnumerator IEnumerable.GetEnumerator() => this;

		public bool MoveNext() {
			if (!_stepped) {
				_stepped = true;
				return true;
			}

			return false;
		}

		public void Reset() => throw new NotImplementedException();

		public T Current { get; }

		object IEnumerator.Current => Current;

		public void Dispose() { }
	}

	public static OrderedDictionary<TKey, TSource> ToOrderedDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
		var ordered = new OrderedDictionary<TKey, TSource>();
		foreach (var v in source) ordered.Add(keySelector(v), v);
		return ordered;
	}

	public static OrderedDictionary<TKey, TSource> ToOrderedDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
		IEqualityComparer<TKey> comparer) {
		var ordered = new OrderedDictionary<TKey, TSource>(comparer);
		foreach (var v in source) ordered.Add(keySelector(v), v);
		return ordered;
	}

	public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) {
		var ordered = new OrderedDictionary<TKey, TElement>();
		foreach (var v in source) ordered.Add(keySelector(v), elementSelector(v));
		return ordered;
	}

	public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
		var ordered = new OrderedDictionary<TKey, TElement>(comparer);
		foreach (var v in source) ordered.Add(keySelector(v), elementSelector(v));
		return ordered;
	}
}