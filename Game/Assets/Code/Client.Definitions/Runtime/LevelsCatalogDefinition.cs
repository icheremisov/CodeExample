using UnityEngine;
using XLib.Configs.Core;

namespace Client.Definitions
{
    public class LevelsCatalogDefinition : GameItemSingleton<LevelsCatalogDefinition>
    {
        [SerializeField] private LevelDefinition[] _levels;

        public LevelDefinition[] Levels => _levels;
    }
}