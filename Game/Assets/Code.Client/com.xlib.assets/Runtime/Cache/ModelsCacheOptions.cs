using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using XLib.Assets.Configs;
using XLib.Assets.Contracts;
using XLib.Assets.Internal;

namespace XLib.Assets.Cache {

	public class ModelsCacheOptions {

		internal List<string> PreloadRequests { get; } = new(4);

		internal List<Func<IAssetProvider, ModelsCacheService, UniTask>> CacheRequests { get; } = new(4);

		/// <summary>
		///     only preload addressables with this label and dont add it to cache
		/// </summary>
		public ModelsCacheOptions PreloadOnly<TModel>(AssetLabel label)
			where TModel : class {
			PreloadRequests.AddOnce(label.ToString());
			CacheRequests.Add((assetProvider, cache) => assetProvider.LoadByLabelAsync<TModel>(label));

			return this;
		}

		/// <summary>
		///     load configs with label
		/// </summary>
		public ModelsCacheOptions AsList<TModel>(AssetLabel label, Func<TModel, object> getKey)
			where TModel : class {
			PreloadRequests.AddOnce(label.ToString());
			CacheRequests.Add((assetProvider, cache) => assetProvider.LoadByLabelAsync<TModel>(label).ContinueWith(configs => cache.AddList(configs, getKey)));

			return this;
		}

		/// <summary>
		///     load configs with label and extract them from BaseHolder class
		/// </summary>
		public ModelsCacheOptions AsList<THolder, TModel>(AssetLabel label, Func<TModel, object> getKey)
			where THolder : class, IBaseHolder<TModel>
			where TModel : class {
			PreloadRequests.AddOnce(label.ToString());
			CacheRequests.Add((assetProvider, cache) =>
				assetProvider.LoadByLabelAsync<THolder>(label).ContinueWith(configs => cache.AddList(configs.Select(x => x.Item), getKey)));

			return this;
		}

		/// <summary>
		///     load configs with label and extract them from holder class
		/// </summary>
		public ModelsCacheOptions AsList<THolder, TModel>(AssetLabel label, Func<THolder, TModel> getConfig, Func<TModel, object> getKey)
			where THolder : class
			where TModel : class {
			PreloadRequests.AddOnce(label.ToString());
			CacheRequests.Add((assetProvider, cache) =>
				assetProvider.LoadByLabelAsync<THolder>(label).ContinueWith(configs => cache.AddList(configs.Select(getConfig), getKey)));

			return this;
		}

		/// <summary>
		///     load single config
		/// </summary>
		public ModelsCacheOptions AsSingle<TModel>(string address)
			where TModel : class {
			CacheRequests.Add((assetProvider, cache) => assetProvider.LoadByKeyAsync<TModel>(address).ContinueWith(cache.AddSingle));

			return this;
		}

		/// <summary>
		///     load single config and extract them from holder class
		/// </summary>
		public ModelsCacheOptions AsSingle<THolder, TModel>(string address, Func<THolder, TModel> getConfig)
			where TModel : class
			where THolder : class {
			CacheRequests.Add((assetProvider, cache) => assetProvider.LoadByKeyAsync<THolder>(address).ContinueWith(holder => cache.AddSingle(getConfig(holder))));

			return this;
		}

		/// <summary>
		///     load single config by label
		/// </summary>
		public ModelsCacheOptions AsSingle<TModel>(AssetLabel label)
			where TModel : class {
			CacheRequests.Add((assetProvider, cache) => assetProvider.LoadByKeyAsync<TModel>(label.ToString()).ContinueWith(cache.AddSingle));

			return this;
		}

		/// <summary>
		///     load single config by label and extract them from holder class
		/// </summary>
		public ModelsCacheOptions AsSingle<THolder, TModel>(AssetLabel label, Func<THolder, TModel> getConfig)
			where TModel : class
			where THolder : class {
			CacheRequests.Add((assetProvider, cache) =>
				assetProvider.LoadByKeyAsync<THolder>(label.ToString()).ContinueWith(holder => cache.AddSingle(getConfig(holder))));

			return this;
		}

	}

}