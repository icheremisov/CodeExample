using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using XLib.Assets.Cache;
using XLib.Assets.Types;

namespace XLib.Assets.Contracts {

	public interface IAssetProvider : IDisposable {
		string ConfigHash { get; }
		bool IsFullDownloaded { get; }
		long DownloadingSize { get; }
		UniTask InitializeAsync(string catalogPath, CancellationToken ct);
		UniTask InitializeCatalogAsync(string version, string configHash, CancellationToken ct);

		UniTask<T> LoadByKeyAsync<T>(string key, string category = AddressableCategory.Default) where T : class;
		UniTask<T> LoadAsync<T>(AssetReference reference, string category = AddressableCategory.Default) where T : class;
		UniTask<IEnumerable<T>> LoadByLabelAsync<T>(AssetLabel label, string category = AddressableCategory.Default) where T : class;
		UniTask<IEnumerable<T>> LoadAsync<T>(IEnumerable<string> keys, string category = AddressableCategory.Default) where T : class;
		UniTask PreloadAsync(string[] keys);
		UniTask PreloadAllAssets(IProgress<float> progress = null);
		void SetNeedClearCacheAsync();

		void Unload(string key);
		void UnloadCategory(string category);

		bool IsKeyValid<T>(string key) where T : class;

	}

}