using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using XLib.Assets.Cache;
using XLib.Assets.Contracts;
using XLib.BuildSystem;
using XLib.Core.Utils;
using XLib.Unity.LocalStorage;

namespace XLib.Assets.Internal {

	internal class AddressablesAssetProvider : IAssetProvider {
		private readonly List<AssetHandle> _assets = new(256);
		private readonly List<string> _removableIds = new() { "AddressablesMainContentCatalog" };
		private readonly StoredValue<bool> _isNeedClearCache = new("addressables.is.need.clear.cache", false);

		public string ConfigHash { get; private set; }

		public bool IsFullDownloaded => _downloadingSize == 0;
		public long DownloadingSize => _downloadingSize;

		private long _downloadingSize = -1;
		private string _catalogPath;
		private IResourceLocator _remoteLocator;

		private CancellationTokenSource _cts;

		public async UniTask InitializeAsync(string catalogPath, CancellationToken ct) {
			ConfigHash = VersionService.ConfigHash;
			_catalogPath = catalogPath;

			using (TraceLog.Usage<AddressablesAssetProvider>(nameof(Addressables.InitializeAsync))) {
				await Addressables.InitializeAsync(true);
				ct.ThrowIfCancellationRequested();
			}

			if (_isNeedClearCache.Value) {
				await ClearCacheAsync();
				_isNeedClearCache.Value = false;
			}

			_downloadingSize = await GetFullDownloadingSizeAsync();
		}

		public async UniTask InitializeCatalogAsync(string version, string configHash, CancellationToken ct) {
			var catalogPath = GetUrl(version);
			Debug.Log("Load Content Catalog: " + catalogPath);

			var resourceLocator = await Addressables.LoadContentCatalogAsync(catalogPath).ToUniTask(cancellationToken: ct);
			ConfigHash = configHash;
			_remoteLocator = resourceLocator ?? throw new Exception();

			var oldLocator = Addressables.ResourceLocators.FirstOrDefault(locator => locator.LocatorId == _remoteLocator.LocatorId);

			if (oldLocator != null) _removableIds.Add(oldLocator.LocatorId);

			var locators = Addressables.ResourceLocators;
			var removableLocators = locators.Where(loc => _removableIds.Contains(loc.LocatorId)).ToList();

			foreach (var locator in removableLocators) {
				Debug.Log($"Removed Resource Locator: {locator.LocatorId}");
				Addressables.RemoveResourceLocator(locator);
			}

			Addressables.AddResourceLocator(_remoteLocator);
			Debug.Log($"Add Resource Locator: {_remoteLocator.LocatorId}");

			_downloadingSize = await GetFullDownloadingSizeAsync();
		}

		private string GetUrl(string version) =>
			_catalogPath
				.Replace("{version}", version)
				.Replace("{target}", VersionService.BundleTarget);

		public void Dispose() {
			foreach (var asset in _assets) Addressables.Release(asset.Handle);

			DisposeToken();
			_assets.Clear();
		}

		public async UniTask<T> LoadByKeyAsync<T>(string key, string category) where T : class {
			if (key.IsNullOrEmpty()) throw new Exception("key is null or empty");

			if (!await IsAssetInCacheAsync(key)) throw new Exception($"Tried to load asset not downloaded on device. Abort loading.\nKey: {key}");

			try {
				var asyncOperationHandle = Addressables.LoadAssetAsync<T>(key);
				_assets.Add(new AssetHandle { Handle = asyncOperationHandle, Key = key, Category = category });

				return await asyncOperationHandle;
			}
			catch (Exception ex) {
				Debug.LogError($"Error loading asset '{key}': {ex.Message}");
				throw;
			}
		}

		public async UniTask<T> LoadAsync<T>(AssetReference reference, string category) where T : class {
			if (reference.IsValid()) return reference.Asset as T;

			if (!await IsAssetInCacheAsync(reference.RuntimeKey)) throw new Exception($"Tried to load asset not downloaded on device. Abort loading.\nKey: {reference.RuntimeKey}");

			try {
				if (reference.IsValid())
					return (reference.Asset as T)
						?? throw new InvalidOperationException($"Failed to load {reference.AssetGUID} object of type {typeof(T).Name}.");

				var asyncOperationHandle = reference.LoadAssetAsync<T>();
				_assets.Add(new AssetHandle { Handle = asyncOperationHandle, Key = reference.AssetGUID, Category = category });

				return await asyncOperationHandle;
			}
			catch (Exception ex) {
				Debug.LogError($"Error loading asset '{reference.AssetGUID}': {ex.Message}");
				throw;
			}
		}

		public async UniTask<IEnumerable<T>> LoadByLabelAsync<T>(AssetLabel label, string category) where T : class {
			if (label.Label.IsNullOrEmpty()) throw new Exception("key is null or empty");

			if (!await IsAssetInCacheAsync(label.Label)) throw new Exception($"Tried to load asset not downloaded on device. Abort loading.\nLabel: {label.Label}");

			try {
				var asyncOperationHandle = Addressables.LoadAssetsAsync<T>(label.Label, x => { });
				_assets.Add(new AssetHandle { Handle = asyncOperationHandle, Category = category });

				return await asyncOperationHandle;
			}
			catch (Exception ex) {
				Debug.LogError($"Error loading assets by label '{label}': {ex.Message}");
				throw;
			}
		}

		public async UniTask<IEnumerable<T>> LoadAsync<T>(IEnumerable<string> keys, string category) where T : class {
			return await UniTask.WhenAll(keys.Select(x => LoadByKeyAsync<T>(x, category)));
		}

		public async UniTask PreloadAllAssets(IProgress<float> progress = null) {
			using (TraceLog.Usage<AddressablesAssetProvider>(nameof(Addressables.DownloadDependenciesAsync))) {
				var keys = Addressables.ResourceLocators.SelectMany(k => k.Keys);
				var handle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union);
				try {
					Debug.Log($"[AddressableLoading] PreloadAllAssets. Start loading assets.");

					if (progress != null) {
						RefreshToken();
						var timing = 0.1f;
						ReportProgressAsync(handle, progress, timing, _cts.Token).Forget();
					}

					await handle.ToUniTask();
				}
				catch (Exception e) {
					Debug.LogError($"[{nameof(PreloadAllAssets)}] {e}");

					DisposeToken();
					throw;
				}
				finally {
					Addressables.Release(handle);
					_downloadingSize = await GetFullDownloadingSizeAsync();
				}
			}
		}

		public async UniTask PreloadAsync(string[] keys) {
			if (keys.IsNullOrEmpty()) return;
			Debug.Log($"[AddressableLoading] PreloadAsync.");

			await Addressables.DownloadDependenciesAsync(keys.AsEnumerable(), Addressables.MergeMode.Union).ToUniTask();
			_downloadingSize = await GetFullDownloadingSizeAsync();
		}

		public async UniTask<bool> IsFullLoadedAsync() {
			return await GetFullDownloadingSizeAsync() == 0;
		}

#if UNITY_EDITOR
		public UniTask<long> GetFullDownloadingSizeAsync() => UniTask.FromResult(0L);
#else
		public async UniTask<long> GetFullDownloadingSizeAsync() {
			using var _ = TraceLog.Usage("[AddressableLoading] Calculating downloading size.");
			var locator = _remoteLocator ?? Addressables.ResourceLocators.First();

			var size = await Addressables.GetDownloadSizeAsync(locator.Keys.AsEnumerable());
			return size;
		}
#endif

		public void SetNeedClearCacheAsync() {
			_isNeedClearCache.Value = true;
		}

		public void Unload(string key) {
			for (var i = 0; i < _assets.Count; i++) {
				var asset = _assets[i];

				if (asset.Key != key) continue;

				Addressables.Release(asset.Handle);
				_assets.RemoveAt(i);
				break;
			}
		}

		public void UnloadCategory(string category) {
			for (var i = 0; i < _assets.Count; i++) {
				var asset = _assets[i];

				if (asset.Category != category) continue;

				Addressables.Release(asset.Handle);
				_assets.RemoveAt(i);
				--i;
			}
		}

		public bool IsKeyValid<T>(string key) where T : class => Addressables.ResourceLocators.Any(l => l.Locate(key, TypeOf<T>.Raw, out _));

		private async UniTask<bool> IsAssetInCacheAsync(object key) {
			return await Addressables.GetDownloadSizeAsync(key) == 0;
		}

		private async UniTask ReportProgressAsync(AsyncOperationHandle handle, IProgress<float> progress, float timing, CancellationToken token) {
			while (!handle.IsDone) {
				progress.Report(handle.GetDownloadStatus().Percent);
				await UniTask.Delay(TimeSpan.FromSeconds(timing), cancellationToken: token);
			}
		}

		private UniTask ClearCacheAsync() {
			using var _ = TraceLog.Usage($"Clear addressable assets cache.");
			var locator = _remoteLocator ?? Addressables.ResourceLocators.First();
			Debug.Log($"[AddressableLoading] Clear cache for locator: {locator.LocatorId}");
			return Addressables.ClearDependencyCacheAsync(locator.Keys.AsEnumerable(), true).ToUniTask();
		}

		private void RefreshToken() {
			DisposeToken();
			_cts = new CancellationTokenSource();
		}

		private void DisposeToken() {
			if (_cts == null) return;
			_cts.Cancel();
			_cts.Dispose();
			_cts = null;
		}

		private struct AssetHandle {
			public AsyncOperationHandle Handle { get; set; }
			public string Key { get; set; }
			public string Category { get; set; }
		}
	}

}