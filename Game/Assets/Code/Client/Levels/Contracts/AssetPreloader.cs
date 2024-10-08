using System;
using System.Linq;
using Client.Definitions;
using Client.Levels.Internal;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using XLib.Assets.Contracts;
using XLib.Configs.Contracts;
using Object = UnityEngine.Object;

namespace Client.Levels.Contracts {
	public class AssetPreloader : IAssetPreloader {
		private readonly IAssetProvider _assetProvider;
		private readonly LevelContext _levelContext;

		public AssetPreloader(IAssetProvider assetProvider, LevelContext levelContext) {
			_assetProvider = assetProvider;
			_levelContext = levelContext;
		}

		public UniTask PreloadAsync() {
			var loads = GameData.All<BlockDefinition>()
				.SelectMany(b => b.GetAssetRefs(AssetTypeFilter.BlockPrefab))
				.Select(PreloadAssetAsync);
			return UniTask.WhenAll(loads);
		}

		private async UniTask PreloadAssetAsync(AssetReference reference) {
			await _assetProvider.LoadAsync<Object>(reference);
		}
	}

}