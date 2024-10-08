using System;
using System.Collections;
using System.Collections.Generic;
using Entitas.CodeGeneration.Attributes;
using XLib.Core.Utils;

namespace Client.Core.Ecs.Components {

	public abstract class PrimaryValueComponent<T> : ValueComponentCore<T>, IValueComponent<T> {
		[PrimaryEntityIndex]
		public T Value { get => _value; set => _value = value; }
	}

	public abstract class ValueComponent<T> : ValueComponentCore<T>, IValueComponent<T> {
		public T Value { get => _value; set => _value = value; }
	}

	public abstract class ValueListComponent<T> : ValueComponent<List<T>>, IEnumerable<T> {
		public T this[int index] { get => _value[index]; set => _value[index] = value; }
		public IEnumerator<T> GetEnumerator() => _value.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public int Count => _value.Count;
	}

	public abstract class ValueDictionaryComponent<T1, T2> : ValueComponent<Dictionary<T1, T2>>, IEnumerable<KeyValuePair<T1,T2>> {
		public T2 this[T1 key] { get => _value[key]; set => _value[key] = value; }
		public IEnumerator<T1> Keys() => _value.Keys.GetEnumerator();
		public IEnumerator<T2> Values() => _value.Values.GetEnumerator();
		public int Count => _value.Count;
		public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() => _value.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public abstract class ValueEnumerableComponent<TEnumerator, TElement> : ValueComponent<TEnumerator>, IEnumerable<TElement> where TEnumerator : IEnumerable<TElement> {
		public IEnumerator<TElement> GetEnumerator() => _value.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	/// <summary>
	/// base class for all components with one value
	/// </summary>
	public abstract class ValueComponentCore<T> :
		IComparable<ValueComponentCore<T>>,
		IComparable<T>,
		IEquatable<ValueComponentCore<T>>,
		IEquatable<T> {
		private T ValueInternal => _value;

		protected T _value;
		public static implicit operator T(ValueComponentCore<T> component) => component != null ? component.ValueInternal : default;

		public override string ToString() {
			var name = GetType().Name;
			if (name.EndsWith("Component")) name = name[..^9];
			return $"{name}({ValueInternal})";
		}

		int IComparable<ValueComponentCore<T>>.CompareTo(ValueComponentCore<T> other) {
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			if (ValueInternal is IComparable<T> comparableT) return comparableT.CompareTo(other.ValueInternal);
			throw new InvalidOperationException($"{GetType().FullName} cannot compare value of type {TypeOf<T>.Name} - value must be IComparable<{TypeOf<T>.Name}>");
		}

		int IComparable<T>.CompareTo(T other) {
			if (ValueInternal is IComparable<T> comparableT) return comparableT.CompareTo(other);
			throw new InvalidOperationException($"{GetType().FullName} cannot compare value of type {TypeOf<T>.Name} - value must be IComparable<{TypeOf<T>.Name}>");
		}

		bool IEquatable<ValueComponentCore<T>>.Equals(ValueComponentCore<T> other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return EqualityComparer<T>.Default.Equals(ValueInternal, other.ValueInternal);
		}

		bool IEquatable<T>.Equals(T other) => EqualityComparer<T>.Default.Equals(ValueInternal, other);

		public override bool Equals(object obj) {
			if (obj is T other) return EqualityComparer<T>.Default.Equals(ValueInternal, other);
			if (obj is ValueComponentCore<T> otherBase) return ((IEquatable<ValueComponentCore<T>>)this).Equals(otherBase);
			if (obj == null) return false;
			throw new InvalidOperationException($"{GetType().FullName} cannot compare value of type {obj.GetType().Name}");
		}

		public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(ValueInternal);

		public static bool operator ==(ValueComponentCore<T> a, ValueComponentCore<T> b) =>
			((object)a) == null || ((object)b) == null ? ReferenceEquals(a, b) : ((IEquatable<ValueComponentCore<T>>)a).Equals(b);

		public static bool operator !=(ValueComponentCore<T> a, ValueComponentCore<T> b) => !(a == b);
		public static bool operator ==(ValueComponentCore<T> a, T b) => ((IEquatable<T>)a)?.Equals(b) ?? b == null;
		public static bool operator !=(ValueComponentCore<T> a, T b) => !(a == b);
		public static T operator ~(ValueComponentCore<T> a) => a.ValueInternal;
	}

}