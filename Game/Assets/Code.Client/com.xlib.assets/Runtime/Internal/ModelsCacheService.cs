using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using XLib.Assets.Cache;
using XLib.Assets.Contracts;
using XLib.Core.Utils;

namespace XLib.Assets.Internal {

	/// <summary>
	///     preload configs into memory for faster instant access
	/// </summary>
	internal class ModelsCacheService : IModelsCacheService {

		private readonly IAssetProvider _assetProvider;

		private readonly Dictionary<Type, CacheEntry> _cache = new(8);

		public ModelsCacheService(IAssetProvider assetProvider) {
			_assetProvider = assetProvider;
		}

		/// <summary>
		///     initialize cache once
		/// </summary>
		public async UniTask ApplyAsync(ModelsCacheOptions options) {
			await _assetProvider.PreloadAsync(options.PreloadRequests.ToArray());

			foreach (var request in options.CacheRequests) await request(_assetProvider, this);
		}

		public IEnumerable<TModel> GetList<TModel>(Func<TModel, bool> selector = null) where TModel : class {
			var modelType = TypeOf<TModel>.Raw;
			if (!_cache.TryGetValue(modelType, out var entry)) throw new KeyNotFoundException($"Models {modelType.FullName} not loaded into cache!");

			if (!entry.isList) throw new InvalidOperationException($"Cannot get single model {modelType.FullName} as list!");

			var result = (TModel[])entry.items;
			return selector == null ? result : result.Where(selector);
		}

		public TModel GetSingle<TModel>() where TModel : class {
			var modelType = TypeOf<TModel>.Raw;
			if (!_cache.TryGetValue(modelType, out var entry)) throw new KeyNotFoundException($"Models {modelType.FullName} not loaded into cache!");

			if (entry.isList) throw new InvalidOperationException($"Cannot get single model {modelType.FullName} from list!");

			return (TModel)entry.items;
		}

		public TModel GetByKey<TModel>(object key) where TModel : class {
			if (key == null) return null;

			var modelType = TypeOf<TModel>.Raw;
			if (!_cache.TryGetValue(modelType, out var entry)) throw new KeyNotFoundException($"Models {modelType.FullName} not loaded into cache!");

			if (!entry.isList) throw new InvalidOperationException($"Cannot get single model {modelType.FullName} by key!");

			return entry.lookup.ContainsKey(key) ? (TModel)entry.lookup[key] : default;
		}

		public void AddList<TModel>(IEnumerable<TModel> configs, Func<TModel, object> getKey) where TModel : class {
			var modelType = TypeOf<TModel>.Raw;

			if (_cache.ContainsKey(modelType)) throw new InvalidOperationException($"Models {modelType.FullName} already loaded into cache!");

			var items = configs.OrderBy(getKey).ToArray();
			var map = new Hashtable(items.Length);
			foreach (var item in items) map.Add(getKey(item), item);

			_cache.Add(modelType, new CacheEntry { isList = true, items = items, lookup = map });
		}

		public void AddSingle<TModel>(TModel config) where TModel : class {
			var modelType = TypeOf<TModel>.Raw;

			if (_cache.ContainsKey(modelType)) throw new InvalidOperationException($"Models {modelType.FullName} already loaded into cache!");

			_cache.Add(modelType, new CacheEntry { isList = false, items = config });
		}

		private struct CacheEntry {

			public bool isList;
			public object items;
			public Hashtable lookup;

		}

	}

}