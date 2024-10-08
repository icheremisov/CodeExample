using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLib.Configs.Contracts;
using XLib.Configs.Core;
using XLib.Unity.Utils;

namespace Client.Definitions
{
    public class BlockDefinition : GameItem, IConfigAssetRefsCollector
    {
        [SerializeField, Required, ViewReference] private AssetReferenceGameObject _prefab; 
        
        public IEnumerable<AssetReference> GetAssetRefs(AssetTypeFilter filter = AssetTypeFilter.All)
        {
            if (filter.Has(AssetTypeFilter.BlockPrefab))
                yield return _prefab;   
        }
    }
}