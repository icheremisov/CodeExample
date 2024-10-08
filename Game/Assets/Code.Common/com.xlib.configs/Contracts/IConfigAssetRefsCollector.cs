using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace XLib.Configs.Contracts {

	[Flags]
	public enum AssetTypeFilter {
		BlockPrefab = 1 << 0,
		VFX = 1 << 1,
		Sprite = 1 << 2,
		
		
		All = -1
	}
	
	public interface IConfigAssetRefsCollector {
		IEnumerable<AssetReference> GetAssetRefs(AssetTypeFilter filter = AssetTypeFilter.All);
	}

}