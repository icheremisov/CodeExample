using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;
using XLib.Core.Collections;
using XLib.Core.Utils;

namespace XLib.Configs {

	public class GameDatabase : IGameDatabase {
#if UNITY3D
		public static IGameDatabase Instance => _instance;
		private static GameDatabase _instance;
#endif

		private readonly OrderedDictionary<ItemId, IGameItem> _items = new(EnumComparer<ItemId>.Default);
		private readonly OrderedDictionary<Type, IGameItem> _singleton = new();
		private readonly OrderedDictionary<Type, List<IGameItem>> _precomputedMap = new();
		private int _version = 0;
		public string ConfigHash { get; private set; }
		
		public async Task LoadConfigs(IDataStorageProvider storageProvider) {
			_items.Clear();

			var configs = (await storageProvider.LoadAll()).ToArray();

#if DEVELOPMENT_BUILD
			var index = 1;
			foreach (var gameItem in configs) {
				if (gameItem == null) throw new Exception($"Error loading configs: item #{index} is NULL - resave configs.asset!");
				if (gameItem.Id == ItemId.None) throw new Exception($"Error loading configs: item #{index} ({gameItem}) has Id={ItemId.None} - resave configs.asset!");
				++index;
			}
#endif

			foreach (var asset in configs.OrderBy(b => b.Id.AsInt())) {
				if (!_items.TryAdd(asset.Id, asset)) {
					Debug.Assert(asset.Equals(_items[asset.Id]));
				}
				var type = asset.GetType();
				if (asset is IGameItemSingleton) _singleton[type] = asset;
				if (asset is IConfigRemapper r) _version = Mathf.Max(_version, r.MigrationVersion);
				PrecomputeAsset(type, asset);

				if (asset is IGameItemContainer container && TypeOf<IGameItem>.IsAssignableFrom(container.ItemType)) {
					foreach (var e in container.RawElements) {
						if (e is not IGameItem item) continue;
						if (!_items.TryAdd(item.Id, item)) {
							Debug.Assert(item.Equals(_items[item.Id]));
						}
						PrecomputeAsset(item.GetType(), item);
					}
				}
			}

			await InitConfigs();

			ConfigHash = await storageProvider.GetConfigHash();
			Debug.Log($"Configs reloaded: {ConfigHash}");
#if UNITY3D
			_instance = this;
#endif
		}

		private void PrecomputeAsset(Type type, IGameItem asset) {
			do {
				if (!_precomputedMap.TryGetValue(type, out var list)) _precomputedMap[type] = list = new List<IGameItem>(16);

				list.Add(asset);
				type = type.BaseType;
			} while (type != TypeOf<ScriptableObject>.Raw);
		}

		private Task InitConfigs() {
			foreach ((_, var gameItem) in _items) {
				try {
					gameItem.Init(this);
				}
				catch (Exception e) {
					Debug.LogError($"Config initialization failed: {gameItem} (type={gameItem?.GetType().FullName})");
					Debug.LogException(e);
				}
			}

			return Task.CompletedTask;
		}

		T IGameDatabase.Get<T>(ItemId id, bool throwOnNotFound) {
			_items.TryGetValue(id, out var result);
			if (result == null && throwOnNotFound) throw new Exception($"{typeof(T).Name} id:{id.ToKeyString()} does not found in configs");

			var value = result as T;
			if (value == null && throwOnNotFound) throw new Exception($"{typeof(T).Name} id:{id.ToKeyString()} has type {result.GetType().Name} but expected {TypeOf<T>.Name}");
			return value;
		}

		public IEnumerable<T> All<T>() => _precomputedMap[TypeOf<T>.Raw].Cast<T>();

		public IEnumerable<T> AllAsInterface<T>() => _items.Values.OfType<T>();

		public T Once<T>(bool throwOnNotFound) {
			_singleton.TryGetValue(TypeOf<T>.Raw, out var result);

			if (result == null && throwOnNotFound) throw new Exception($"{typeof(T).Name} does not found in configs");

			if (result is not T value) {
				return throwOnNotFound
					? throw new Exception($"{typeof(T).Name} has type {result.GetType().Name} but expected {TypeOf<T>.Name}")
					: default;
			}

			return value;
		}

		public int ShortVersion => _version;

		public void Dispose() {
			if (_items == null) return;
			foreach (var item in _items.Values) item.Dispose();
			_items.Clear();
			_precomputedMap.Clear();
			_singleton.Clear();

#if UNITY3D
			if (_instance == this) _instance = null;
#endif
		}
	}

}