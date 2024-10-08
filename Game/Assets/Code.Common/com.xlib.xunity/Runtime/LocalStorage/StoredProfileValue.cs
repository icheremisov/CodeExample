using System;
using System.Collections.Generic;
using XLib.Core.Reflection;
using XLib.Core.Utils;

namespace XLib.Unity.LocalStorage {

	public interface IStoredProfileValue {
		void Clear();
	}
	
	/// <summary>
	///     store value in local profile data on device (will removed when profile deleted by cheats)
	/// </summary>
	public class StoredProfileValue<T> : IStoredProfileValue {

		private readonly T _defaultValue;
		private readonly string _keyName;
		private bool _isLoaded;

		private T _value;

		public bool IsAvailable => LocalProfileStorage.S.IsLoaded;
		
		public StoredProfileValue(string keyName, T defaultValue) {
			// check for unsupported types. if you want to add support this types you must support it in ValueEquals method
			var type = TypeOf<T>.Raw;

			if (type == TypeOf<Enum>.Raw || type.IsGenericDictionary() || type == TypeOf<object>.Raw) throw new ArgumentException($"Unsupported type {type.FullName}");

			_keyName = keyName;
			_defaultValue = defaultValue;

			LocalProfileStorage.S.Register(this);
		}

		public T Value {
			get {
				if (!_isLoaded) LoadValue();

				return _value;
			}
			set {
				if (_isLoaded && (TypeOf<T>.Raw.IsValueType || TypeOf<T>.Raw == TypeOf<string>.Raw) && ValueEquals(value, _value)) return;

				_value = value;
				_isLoaded = true;
				SaveValue();
			}
		}

		public static implicit operator T(StoredProfileValue<T> v) => v.Value;

		private static bool ValueEquals(T value1, T value2) {
			var type = TypeOf<T>.Raw;
			if (type.IsArray || type.IsGenericList()) return false;

			return EqualityComparer<T>.Default.Equals(value1, value2);
		}

		public void Clear() {
			_isLoaded = false;
			LocalProfileStorage.S.DeleteValue(_keyName);
		}

		private void SaveValue(bool force = false) {
			if (!force && !_isLoaded) return;
			LocalProfileStorage.S.SetValue(_keyName, _value);
		}

		private void LoadValue() {
			_value = LocalProfileStorage.S.GetValue(_keyName, _defaultValue);
		}

		public override string ToString() => Value.ToString();

		public void Save() {
			SaveValue(true);
		}
	}

}