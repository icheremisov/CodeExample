using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using XLib.Core.Reflection;
using XLib.Core.Utils;

namespace XLib.Unity.LocalStorage {

	/// <summary>
	///     store value in local db on device
	/// </summary>
	public class StoredValue<T> {

		private readonly object _defaultValue;
		private readonly string _keyName;
		private bool _isLoaded;

		private T _value;

		public StoredValue(string keyName, T defaultValue) {
			// check for unsupported types. if you want to add support this types you must support it in ValueEquals method
			var type = TypeOf<T>.Raw;

			if (type == TypeOf<Enum>.Raw || type.IsGenericDictionary() || type == TypeOf<object>.Raw) throw new ArgumentException($"Unsupported type {type.FullName}");

			_keyName = keyName;
			_defaultValue = defaultValue;
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

		public static implicit operator T(StoredValue<T> v) => v != null ? v.Value : default(T);

		private static bool ValueEquals(T value1, T value2) {
			var type = TypeOf<T>.Raw;
			if (type.IsArray || type.IsGenericList()) return false;

			return EqualityComparer<T>.Default.Equals(value1, value2);
		}

		public void Clear() {
			_isLoaded = false;
			PlayerPrefs.DeleteKey(_keyName);
		}

		private void SaveValue(bool force = false) {
			if (!force && !_isLoaded) return;

			try {
				var objValue = (object)_value;

				if (TypeOf<T>.Raw == TypeOf<int>.Raw)
					PlayerPrefs.SetInt(_keyName, (int)objValue);
				else if (TypeOf<T>.Raw.IsEnum)
					PlayerPrefs.SetString(_keyName, objValue.ToString());
				else if (TypeOf<T>.Raw == TypeOf<bool>.Raw)
					PlayerPrefs.SetInt(_keyName, (bool)objValue ? 1 : 0);
				else if (TypeOf<T>.Raw == TypeOf<float>.Raw)
					PlayerPrefs.SetFloat(_keyName, (float)objValue);
				else if (TypeOf<T>.Raw == TypeOf<long>.Raw)
					PlayerPrefs.SetString(_keyName, objValue.ToString());
				else if (TypeOf<T>.Raw == TypeOf<string>.Raw)
					PlayerPrefs.SetString(_keyName, (string)objValue);
				else {
					var str = objValue != null ? JsonConvert.SerializeObject(objValue) : "$null";
					PlayerPrefs.SetString(_keyName, str);
				}
			}
			catch (Exception ex) {
				Debug.LogError($"Error saving prefs '{_keyName}': " + ex.Message);
			}
		}

		private void LoadValue() {
			try {
				if (TypeOf<T>.Raw == TypeOf<int>.Raw)
					_value = (T)(object)PlayerPrefs.GetInt(_keyName, (int)_defaultValue);
				else if (TypeOf<T>.Raw.IsEnum) {
					var s = PlayerPrefs.GetString(_keyName, _defaultValue.ToString());
					_value = (T)Enums.ToEnum(TypeOf<T>.Raw, s);
				}
				else if (TypeOf<T>.Raw == TypeOf<bool>.Raw)
					_value = (T)(object)(PlayerPrefs.GetInt(_keyName, (bool)_defaultValue ? 1 : 0) == 1);
				else if (TypeOf<T>.Raw == TypeOf<float>.Raw)
					_value = (T)(object)PlayerPrefs.GetFloat(_keyName, (float)_defaultValue);
				else if (TypeOf<T>.Raw == TypeOf<long>.Raw) {
					var str = PlayerPrefs.GetString(_keyName, string.Empty);
					var val = !str.IsNullOrEmpty() && long.TryParse(str, out var v) ? v : (long)_defaultValue;
					_value = (T)(object)val;
				}
				else if (TypeOf<T>.Raw == TypeOf<string>.Raw)
					_value = (T)(object)PlayerPrefs.GetString(_keyName, (string)_defaultValue);
				else {
					var str = PlayerPrefs.GetString(_keyName, string.Empty);
					if (!str.IsNullOrEmpty())
						_value = str != "$null" ? JsonConvert.DeserializeObject<T>(str) : (T)(object)null;
					else
						_value = (T)_defaultValue;
				}

				_isLoaded = true;
			}
			catch (Exception ex) {
				Debug.LogError($"Error loading StoredValue<{typeof(T).FullName}>('{_keyName}'): {ex.Message}");
				_value = (T)_defaultValue;
			}
		}

		public override string ToString() => Value.ToString();

		public void Save() {
			SaveValue(true);
		}
	}

}