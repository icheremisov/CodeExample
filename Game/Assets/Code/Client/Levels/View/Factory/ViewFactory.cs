using System;
using System.Collections.Generic;
using System.Linq;
using Client.Definitions;
using Cysharp.Threading.Tasks;
using HellTap.PoolKit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLib.Assets.Contracts;
using XLib.Configs.Contracts;
using Object = UnityEngine.Object;

namespace Client.Levels.View.Factory {

	public class ViewFactory : ILevelViewFactory, IDisposable {
		private const string Category = "Level";
		private const string PoolName = "LevelViewsPool";

		private Pool _viewsPool;

		private readonly IAssetProvider _assetProvider;
		private readonly LevelSceneTransforms _sceneTransforms;
		private readonly LevelContext _levelContext;

		private readonly Dictionary<string, GameObject> _levelObjects = new(16);

		public ViewFactory(IAssetProvider assetProvider, LevelSceneTransforms sceneTransforms, LevelContext levelContext) {
			_assetProvider = assetProvider;
			_sceneTransforms = sceneTransforms;
			_levelContext = levelContext;
		}

		private async UniTask<PoolItem> InitAssetAsync(AssetReference reference) {
			var prefab = await _assetProvider.LoadAsync<GameObject>(reference, Category);

			_levelObjects[reference.AssetGUID] = prefab;

			return new PoolItem { prefabToPool = prefab, poolSize = 100, poolSizeOptions = PoolItem.PoolResizeOptions.AlwaysExpandPoolWhenNeeded};
		}

		public async UniTask InitializeAsync() {
			var loads = GameData.All<BlockDefinition>()
				.SelectMany(b => b.GetAssetRefs(AssetTypeFilter.BlockPrefab))
				.Where(reference => reference != null && reference.RuntimeKeyIsValid())
				.Distinct()
				.Select(InitAssetAsync);
			
			var pools = await UniTask.WhenAll(UniTask.WhenAll(loads));
			var poolItems = pools.SelectMany(pool => pool).ToArray();

			_viewsPool = PoolKit.CreatePool(PoolName, Pool.PoolType.Automatic, true, false, false, poolItems);
		}

		public IView CreateView(AssetReference reference) {
			if (reference == null) return null;

			if (!_levelObjects.TryGetValue(reference.AssetGUID, out var prefab)) return null;

			var viewObject = prefab == null ? null : _viewsPool.SpawnGO(prefab, Vector3.zero, Vector3.zero, Vector3.one, _sceneTransforms.RootTransform);
			return viewObject != null ? viewObject.GetComponent<IView>() : null;
		}

		public void DestroyView(IView view) {
			if (view == null) return;

			_viewsPool.Despawn((view as MonoBehaviour)?.gameObject);
		}

		public void Dispose() {
			if (_viewsPool != null) Object.Destroy(_viewsPool.gameObject);
			_assetProvider.UnloadCategory(Category);
		}
	}
}