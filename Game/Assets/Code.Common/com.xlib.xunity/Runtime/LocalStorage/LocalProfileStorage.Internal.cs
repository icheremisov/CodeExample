using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using XLib.Core.Parsers.Base;
using XLib.Core.Utils;

namespace XLib.Unity.LocalStorage {

	public partial class LocalProfileStorage {
		public const string ProfileKey = "ProfileStorage";
		internal static LocalProfileStorage S { get; set; } = new(ProfileKey);

		private struct ItemData {
			public ItemData(string key, Type t, string v) {
				Key = key;
				Type = t == null ? string.Empty : t.FullName;
				Value = v;
			}

			public string Key { get; set; }
			public string Type { get; set; }
			public string Value { get; set; }
		}

		private class StoredData {
			public string Id { get; set; }
			public List<ItemData> Items { get; set; } = new(32);
		}

		private string _profileId;
		private string _profileKey;
		private readonly Dictionary<string, object> _data = new(32);
		private readonly List<WeakReference> _storedValues = new();

		internal bool IsLoaded => !_profileId.IsNullOrEmpty();

		public LocalProfileStorage(string profileKey) {
			_profileKey = profileKey;
		}

		public void Load(string profileId, string overrideKey) {
			if (profileId.IsNullOrEmpty()) throw new ArgumentNullException(nameof(profileId));

			if (overrideKey != null) _profileKey = overrideKey;

			Debug.Log($"[ProfileStorage] Open '{profileId}' from '{_profileKey}'");

			_profileId = profileId;
			LoadData();
		}

		public void Close() {
			Debug.Log($"[ProfileStorage] Close");

			_profileId = null;
			ClearInMemory();
		}

		public void Clear(string overrideKey) {
			Debug.Log($"[ProfileStorage] Clear");

			PlayerPrefs.DeleteKey(overrideKey ?? _profileKey);
			ClearInMemory();
		}

		private void ClearInMemory() {
			_data.Clear();
			_storedValues.RemoveAll(x => !x.IsAlive);
			foreach (var weakReference in _storedValues) ((IStoredProfileValue)weakReference.Target)?.Clear();
		}

		private void LoadData() {
			_data.Clear();

			var json = PlayerPrefs.GetString(_profileKey, string.Empty);
			if (json.IsNullOrEmpty()) return;

			StoredData data;
			try {
				data = JsonConvert.DeserializeObject<StoredData>(json);
			}
			catch (Exception e) {
				Debug.LogError($"[ProfileStorage] Error loading data for '{_profileId}': reset to default! Error: {e}");
				throw;
			}

			if (data.Id != _profileId) {
				Debug.LogWarning($"[ProfileStorage] Data loaded for profile '{data.Id}' but actual is '{_profileId}': reset data");
				DeleteAllKeys();
				return;
			}

			foreach (var item in data.Items) {
				try {
					if (item.Type.IsNullOrEmpty()) {
						_data.Add(item.Key, null);
						continue;
					}

					var type = TypeUtils.GetExistingType(item.Type);
					if (type == TypeOf<int>.Raw)
						_data.Add(item.Key, int.Parse(item.Value));
					else if (type == TypeOf<float>.Raw)
						_data.Add(item.Key, float.Parse(item.Value, CultureInfo.InvariantCulture));
					else if (type == TypeOf<long>.Raw)
						_data.Add(item.Key, long.Parse(item.Value));
					else if (type == TypeOf<string>.Raw)
						_data.Add(item.Key, item.Value ?? string.Empty);
					else if (type.IsEnum)
						_data.Add(item.Key, Enums.ToEnum(type, item.Value));
					else if (type == TypeOf<bool>.Raw)
						_data.Add(item.Key, item.Value == "1");
					else {
						var innerJson = Encoding.UTF8.GetString(Base64Encoder.FromBase64String(item.Value));
						_data.Add(item.Key, JsonConvert.DeserializeObject(innerJson, type));
					}
				}
				catch (Exception e) {
					Debug.LogError($"[ProfileStorage] Error loading data from '{_profileId}'/{item.Key}: '{e}' - skip item");
				}
			}
		}

		private void SaveData() {
			if (!IsLoaded) return;

			var data = new StoredData() { Id = _profileId };

			foreach (var item in _data) {
				try {
					if (item.Value == null) {
						data.Items.Add(new ItemData(item.Key, null, string.Empty));
						continue;
					}

					var type = item.Value.GetType();
					if (type == TypeOf<int>.Raw || type == TypeOf<long>.Raw || type == TypeOf<string>.Raw || type.IsEnum)
						data.Items.Add(new ItemData(item.Key, type, item.Value.ToString()));
					else if (type == TypeOf<bool>.Raw)
						data.Items.Add(new ItemData(item.Key, type, (bool)item.Value ? "1" : "0"));
					else if (type == TypeOf<float>.Raw)
						data.Items.Add(new ItemData(item.Key, type, ((float)item.Value).ToString(CultureInfo.InvariantCulture)));
					else {
						var base64 = Base64Encoder.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item.Value)));
						data.Items.Add(new ItemData(item.Key, type, base64));
					}
				}
				catch (Exception e) {
					Debug.LogError($"[ProfileStorage] Error writing data to '{_profileId}'/{item.Key}: '{e}' - skip item");
				}
			}

			try {
				var json = JsonConvert.SerializeObject(data);
				PlayerPrefs.SetString(_profileKey, json);
			}
			catch (Exception e) {
				Debug.LogError($"[ProfileStorage] Error writing data to '{_profileId}': '{e}'");
			}
		}

		private void ThrowIfNotLoaded() {
			if (!IsLoaded) throw new Exception($"No profile loaded");
		}

		public void SetValue<T>(string keyName, T value) {
			ThrowIfNotLoaded();
			_data[keyName] = value;
			SaveData();
		}

		public T GetValue<T>(string keyName, T defaultValue) {
			ThrowIfNotLoaded();
			if (!_data.TryGetValue(keyName, out var result)) return defaultValue;
			if (result == null) return default;

			return result is T value
				? value
				: throw new Exception($"Trying to get '{_profileId}'/{keyName} of type {TypeOf<T>.Raw} but actual type is '{result?.GetType().FullName ?? "null"}'");
		}

		public void DeleteValue(string keyName) {
			if (!IsLoaded) return;
			if (_data.Remove(keyName)) SaveData();
		}

		public void Register(IStoredProfileValue storedProfileValue) {
			_storedValues.RemoveAll(x => !x.IsAlive);
			_storedValues.Add(new WeakReference(storedProfileValue));
		}
	}

}