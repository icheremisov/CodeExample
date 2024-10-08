using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Bson;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.Core.Collections {

	public interface IOrderedHashSet : IEnumerable {
		Type ElementType { get; }
		
		void SetValues(IEnumerable values);
	}

	[Serializable]
	public class OrderedHashSet<T> : ISet<T>, IList<T>, ISerializationCallbackReceiver, IOrderedHashSet, IReadOnlyList<T> {

		[SerializeField, HideInInspector]
		private readonly List<T> _list = new();
		private readonly HashSet<T> _hashSet = new();

		public OrderedHashSet() { }

		public OrderedHashSet(IEnumerable<T> collection) {
			foreach (var value in collection) Add(value);
			_list.Sort();
		}

#region ICollection<T>

		public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		void ICollection<T>.Add(T item) => Add(item);

		public void Clear() {
			_hashSet.Clear();
			_list.Clear();
		}

		public bool Contains(T item) => _hashSet.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

		public bool Remove(T item) {
			var result = _hashSet.Remove(item);
			if (result) _list.Remove(item);
			return result;
		}

		public int Count => _hashSet.Count;
		public bool IsReadOnly => false;

#endregion

#region ISet<T>

		public bool Add(T item) {
			var isAdded = _hashSet.Add(item);
			if (isAdded) _list.Add(item);

			return isAdded;
		}

		public void UnionWith(IEnumerable<T> other) {
			if (other == null) throw new ArgumentNullException();

			var needSort = false;
			foreach (var item in other) needSort |= Add(item);

			if (needSort) _list.Sort();
		}

		public void ExceptWith(IEnumerable<T> other) {
			if (other == null) throw new ArgumentNullException();

			foreach (var item in other) Remove(item);
		}

		public void IntersectWith(IEnumerable<T> other) {
			if (other == null) throw new ArgumentNullException();

			var otherHash = new OrderedHashSet<T>(other);
			foreach (var item in _hashSet) {
				if (otherHash.Contains(item))
					otherHash.Remove(item);
				else
					Remove(item);
			}

			var needSort = false;
			foreach (var item in otherHash) needSort |= Add(item);

			if (needSort) _list.Sort();
		}

		public void SymmetricExceptWith(IEnumerable<T> other) {
			if (other == null) throw new ArgumentNullException();

			var needSort = false;
			foreach (var item in other) {
				if (Contains(item))
					Remove(item);
				else
					needSort |= Add(item);
			}

			if (needSort) _list.Sort();
		}

		public bool IsProperSubsetOf(IEnumerable<T> other) => _hashSet.IsProperSubsetOf(other);
		public bool IsProperSupersetOf(IEnumerable<T> other) => _hashSet.IsProperSupersetOf(other);
		public bool IsSubsetOf(IEnumerable<T> other) => _hashSet.IsSubsetOf(other);
		public bool IsSupersetOf(IEnumerable<T> other) => _hashSet.IsSupersetOf(other);
		public bool Overlaps(IEnumerable<T> other) => _hashSet.Overlaps(other);
		public bool SetEquals(IEnumerable<T> other) => _hashSet.SetEquals(other);

#endregion
	
#region ISerializationCallbackReceiver

		void ISerializationCallbackReceiver.OnAfterDeserialize() {
			_hashSet.Clear();
			_hashSet.AddRange(_list);
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize() {}

#endregion

#region IList<T>

		public int IndexOf(T item) => _list.IndexOf(item);

		public void Insert(int index, T item) {
			if (_hashSet.Add(item))
				_list.Insert(index, item);
		}

		public void RemoveAt(int index) {
			var item = _list[index];
			_hashSet.Remove(item);
			_list.RemoveAt(index);
		}

		public T this[int index] {
			get => _list[index];
			set {
				var item = _list[index];
				_hashSet.Remove(item);
				_list[index] = value;
				_hashSet.Add(value);
			}
		}

#endregion

		Type IOrderedHashSet.ElementType => TypeOf<T>.Raw;
		void IOrderedHashSet.SetValues(IEnumerable values) {
			_list.Clear();
			_list.AddRange(values.OfType<T>());
			((ISerializationCallbackReceiver)this).OnAfterDeserialize();
		}
	}

}