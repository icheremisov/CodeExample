using System.Linq;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;

namespace Client.Definitions
{
    public class LevelDefinition : GameItem
    {
        [SerializeField, SceneFilter("*/Data/Scenes/*_Logic*", SceneFilterAttribute.SceneBuildModes.AddInBuildSettings)] 
        private string _mainScene;
        
        [SerializeField, SceneFilter("*/Environment/*", SceneFilterAttribute.SceneBuildModes.AddInBuildSettings)]
        private string[] _scenes;
        public string[] Scenes => _scenes.Prepend(_mainScene).ToArray();
        
        [SerializeField] private Vector2Int _size;
        public Vector2Int Size => _size;
        
    }
}