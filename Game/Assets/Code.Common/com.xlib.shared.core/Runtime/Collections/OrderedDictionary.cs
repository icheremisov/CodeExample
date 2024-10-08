using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.Core.Collections {

	public interface IOrderedDictionary {
		Type KeyType { get; }
		Type ValueType { get; }

		IEnumerable Keys { get; }
		IEnumerable Values { get; }
		void SetValues(IEnumerable keys, IEnumerable values);
	}

	[Serializable]
	public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>,
												   IReadOnlyDictionary<TKey, TValue>,
												   IList<KeyValuePair<TKey, TValue>>,
												   ISerializationCallbackReceiver,
												   IOrderedDictionary, IDictionary {
		private List<KeyValuePair<TKey, TValue>> _list;
		private Dictionary<TKey, TValue> _innerDictionary;
		private IEqualityComparer<TKey> _comparer;

		public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer) {
			_innerDictionary = new Dictionary<TKey, TValue>(capacity, comparer);
			_list = new List<KeyValuePair<TKey, TValue>>(capacity);
			_comparer = comparer;
		}

		public OrderedDictionary(int capacity) {
			_innerDictionary = new Dictionary<TKey, TValue>(capacity);
			_list = new List<KeyValuePair<TKey, TValue>>(capacity);
		}

		public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) {
			_innerDictionary = new Dictionary<TKey, TValue>(comparer);
			_list = new List<KeyValuePair<TKey, TValue>>();
			_AddValues(collection);
			_comparer = comparer;
		}

		public OrderedDictionary(IEqualityComparer<TKey> comparer) {
			_innerDictionary = new Dictionary<TKey, TValue>(comparer);
			_list = new List<KeyValuePair<TKey, TValue>>();
			_comparer = comparer;
		}

		public OrderedDictionary() {
			_innerDictionary = new Dictionary<TKey, TValue>();
			_list = new List<KeyValuePair<TKey, TValue>>();
		}

		public TValue GetAt(int index) => _list[index].Value;
		public TKey GetKeyAt(int index) => _list[index].Key;

		public void SetAt(int index, TValue value) {
			var key = _list[index].Key;
			_list[index] = new KeyValuePair<TKey, TValue>(key, value);
			_innerDictionary[key] = value;
		}

		public void Insert(int index, TKey key, TValue value) => (this as IList<KeyValuePair<TKey, TValue>>).Insert(index, new KeyValuePair<TKey, TValue>(key, value));

		private void _AddValues(IEnumerable<KeyValuePair<TKey, TValue>> collection) {
			foreach (var kvp in collection) {
				_innerDictionary.Add(kvp.Key, kvp.Value);
				_list.Add(kvp);
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#region ICollection

		public int Count => _list.Count;

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
			_innerDictionary.Add(item.Key, item.Value);
			_list.Add(item);
		}

		public void Clear() {
			_innerDictionary.Clear();
			_list.Clear();
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
			if (null == _comparer) return _list.Contains(item);
			for (int ic = _list.Count, i = 0; i < ic; ++i) {
				var kvp = _list[i];
				if (!_comparer.Equals(item.Key, kvp.Key)) continue;
				if (Equals(item.Value, kvp.Value)) return true;
			}

			return false;
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) =>
			(_innerDictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item) && _list.Remove(item);

#endregion

#region IDictionary

		public void Add(TKey key, TValue value) => (this as ICollection<KeyValuePair<TKey, TValue>>).Add(new KeyValuePair<TKey, TValue>(key, value));

		public bool ContainsKey(TKey key) => _innerDictionary.ContainsKey(key);

		public bool Remove(TKey key) {
			TValue value;
			if (!_innerDictionary.TryGetValue(key, out value)) return false;
			_innerDictionary.Remove(key);
			_list.Remove(new KeyValuePair<TKey, TValue>(key, value));
			return true;
		}

		public bool TryGetValue(TKey key, out TValue value) => _innerDictionary.TryGetValue(key, out value);

		public TValue this[TKey key] {
			get => _innerDictionary[key];
			set {
				TValue v;
				if (_innerDictionary.TryGetValue(key, out v)) {
					// change an existing key
					_innerDictionary[key] = value;
					if (_comparer != null) {
						for (var index = 0; index < _list.Count; ++index) {
							if (_comparer.Equals(_list[index].Key, key)) _list[index] = new KeyValuePair<TKey, TValue>(key, value);
						}
					}
					else {
						_list[_list.IndexOf(new KeyValuePair<TKey, TValue>(key, v))] = new KeyValuePair<TKey, TValue>(key, value);
					}
				}
				else {
					_innerDictionary.Add(key, value);
					_list.Add(new KeyValuePair<TKey, TValue>(key, value));
				}
			}
		}
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
		public ICollection<TKey> Keys => new KeysCollection(this);
		public ICollection<TValue> Values => new ValuesCollection(this);

		bool IDictionary.IsFixedSize => false;
		bool IDictionary.IsReadOnly => false;
		object IDictionary.this[object key] {
			get => this[(TKey)key];
			set => this[(TKey)key] = (TValue)value;
		}
		ICollection IDictionary.Keys => new KeysCollection(this);
		ICollection IDictionary.Values => new ValuesCollection(this);
		void IDictionary.Add(object key, object value) => Add((TKey)key, (TValue)value);
		void IDictionary.Clear() => Clear();
		bool IDictionary.Contains(object key) => ContainsKey((TKey)key);
		IDictionaryEnumerator IDictionary.GetEnumerator() => _innerDictionary.GetEnumerator();
		void IDictionary.Remove(object key) => Remove((TKey)key);

		int ICollection.Count => Count;
		bool ICollection.IsSynchronized => ((ICollection)_list).IsSynchronized;
		object ICollection.SyncRoot => ((ICollection)_list).SyncRoot;
		void ICollection.CopyTo(Array array, int index) => ((ICollection)_list).CopyTo(array, index);

#endregion

#region IList

		public void RemoveAt(int index) {
			var key = _list[index].Key;
			_innerDictionary.Remove(key);
			_list.RemoveAt(index);
		}

		int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item) => _list.IndexOf(item);

		void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item) {
			_innerDictionary.Add(item.Key, item.Value);
			_list.Insert(index, item);
		}

		KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index] { get => _list[index]; set => _list[index] = value; }

#endregion

		private struct KeysCollection : ICollection<TKey>, ICollection {
			OrderedDictionary<TKey, TValue> _outer;
			public KeysCollection(OrderedDictionary<TKey, TValue> outer) => _outer = outer;

			void ICollection.CopyTo(Array array, int index) {
				var count = _outer.Count;
				if (null == array) throw new ArgumentNullException(nameof(array));
				if (1 != array.Rank || 0 != array.GetLowerBound(0)) throw new ArgumentException("The array is not an SZArray", nameof(array));
				if (0 > index) throw new ArgumentOutOfRangeException(nameof(index), "The index cannot be less than zero.");
				if (array.Length <= index)
					throw new ArgumentOutOfRangeException(nameof(index),
						"The index cannot be greater than the length of the array.");
				if (count > array.Length + index) throw new ArgumentException("The array is not big enough to hold the collection entries.", nameof(array));
				for (var i = 0; i < count; ++i) array.SetValue(_outer._list[i].Key, i + index);
			}
			public int Count => _outer.Count;
			bool ICollection.IsSynchronized => true;
			object ICollection.SyncRoot => null;
			public bool Contains(TKey key) => _outer.ContainsKey(key);

			public void CopyTo(TKey[] array, int index) {
				var count = _outer.Count;
				if (null == array) throw new ArgumentNullException(nameof(array));
				if (1 != array.Rank || 0 != array.GetLowerBound(0)) throw new ArgumentException("The array is not an SZArray", nameof(array));
				if (0 > index) throw new ArgumentOutOfRangeException(nameof(index), "The index cannot be less than zero.");
				if (array.Length <= index)
					throw new ArgumentOutOfRangeException(nameof(index),
						"The index cannot be greater than the length of the array.");
				if (count > array.Length + index) throw new ArgumentException("The array is not big enough to hold the collection entries.", nameof(array));
				for (var i = 0; i < count; ++i) array[i + index] = _outer._list[i].Key;
			}

			public IEnumerator<TKey> GetEnumerator() => new Enumerator(_outer);
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			void ICollection<TKey>.Add(TKey key) => throw new NotSupportedException("The collection is read only.");
			bool ICollection<TKey>.Remove(TKey key) => throw new NotSupportedException("The collection is read only.");
			void ICollection<TKey>.Clear() => throw new NotSupportedException("The collection is read only.");
			bool ICollection<TKey>.IsReadOnly => true;

			struct Enumerator : IEnumerator<TKey> {
				IEnumerator<KeyValuePair<TKey, TValue>> _inner;
				public Enumerator(OrderedDictionary<TKey, TValue> outer) => _inner = outer.GetEnumerator();

				public void Reset() => _inner.Reset();
				void IDisposable.Dispose() => _inner.Dispose();
				public bool MoveNext() => _inner.MoveNext();
				public TKey Current => _inner.Current.Key;
				object IEnumerator.Current => Current;
			}
		}

		private struct ValuesCollection : ICollection<TValue>, ICollection {
			OrderedDictionary<TKey, TValue> _outer;
			public ValuesCollection(OrderedDictionary<TKey, TValue> outer) => _outer = outer;

			void ICollection.CopyTo(Array array, int index) {
				var count = _outer.Count;
				// check our parameters for validity
				if (null == array) throw new ArgumentNullException(nameof(array));
				if (1 != array.Rank || 0 != array.GetLowerBound(0)) throw new ArgumentException("The array is not an SZArray", nameof(array));
				if (0 > index) throw new ArgumentOutOfRangeException(nameof(index), "The index cannot be less than zero.");
				if (array.Length <= index) throw new ArgumentOutOfRangeException(nameof(index), "The index cannot be greater than the length of the array.");
				if (count > array.Length + index) throw new ArgumentException("The array is not big enough to hold the collection entries.", nameof(array));
				for (var i = 0; i < count; ++i) array.SetValue(_outer._list[i].Value, i + index);
			}

			public int Count => _outer.Count;
			bool ICollection.IsSynchronized => true;
			object ICollection.SyncRoot => null;

			public bool Contains(TValue value) {
				for (int ic = Count, i = 0; i < ic; ++i)
					if (Equals(_outer._list[i].Value, value))
						return true;
				return false;
			}

			public void CopyTo(TValue[] array, int index) {
				var count = _outer.Count;
				// check our parameters for validity
				if (null == array) throw new ArgumentNullException(nameof(array));
				if (1 != array.Rank || 0 != array.GetLowerBound(0)) throw new ArgumentException("The array is not an SZArray", nameof(array));
				if (0 > index) throw new ArgumentOutOfRangeException(nameof(index), "The index cannot be less than zero.");
				if (array.Length <= index) throw new ArgumentOutOfRangeException(nameof(index), "The index cannot be greater than the length of the array.");
				if (count > array.Length + index) throw new ArgumentException("The array is not big enough to hold the collection entries.", nameof(array));
				for (var i = 0; i < count; ++i) array[i + index] = _outer._list[i].Value;
			}

			public IEnumerator<TValue> GetEnumerator() => new Enumerator(_outer);
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			void ICollection<TValue>.Add(TValue value) => throw new NotSupportedException("The collection is read only.");
			bool ICollection<TValue>.Remove(TValue value) => throw new NotSupportedException("The collection is read only.");
			void ICollection<TValue>.Clear() => throw new NotSupportedException("The collection is read only.");
			bool ICollection<TValue>.IsReadOnly => true;

			struct Enumerator : IEnumerator<TValue> {
				IEnumerator<KeyValuePair<TKey, TValue>> _inner;
				public Enumerator(OrderedDictionary<TKey, TValue> outer) => _inner = outer.GetEnumerator();

				public void Reset() => _inner.Reset();
				void IDisposable.Dispose() => _inner.Dispose();
				public bool MoveNext() => _inner.MoveNext();
				public TValue Current => _inner.Current.Value;
				object IEnumerator.Current => Current;
			}
		}

		[SerializeField, HideInInspector]
		private List<TKey> _keys = new List<TKey>();

		[SerializeField, HideInInspector]
		private List<TValue> _values = new List<TValue>();

		void ISerializationCallbackReceiver.OnAfterDeserialize() {
			Clear();
			for (var i = 0; i < _keys.Count && i < _values.Count; i++) {
				var pair = new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
				_innerDictionary.Add(pair.Key, pair.Value);
				_list.Add(pair);
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize() {
			_keys.Clear();
			_values.Clear();

			foreach (var item in this) {
				_keys.Add(item.Key);
				_values.Add(item.Value);
			}
		}

		Type IOrderedDictionary.KeyType => TypeOf<TKey>.Raw;
		Type IOrderedDictionary.ValueType => TypeOf<TValue>.Raw;
		IEnumerable IOrderedDictionary.Values => Values;
		IEnumerable IOrderedDictionary.Keys => Keys;

		void IOrderedDictionary.SetValues(IEnumerable keys, IEnumerable values) {
			_keys.Clear();
			_values.Clear();
			_keys.AddRange(keys.OfType<TKey>());
			_values.AddRange(values.OfType<TValue>());
			((ISerializationCallbackReceiver)this).OnAfterDeserialize();
		}
	}

}