using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;

namespace Client.Definitions
{
    public class GlobalDefinition : GameItemSingleton<GlobalDefinition>
    {
        [SerializeField, Required, SceneFilter("*/Environment/*", SceneFilterAttribute.SceneBuildModes.AddInBuildSettings)]
        private string _metaMainScene;

        public string MetaMainScene => _metaMainScene;
    }
}