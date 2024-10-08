using Client.Definitions;
using UnityEngine;
using XLib.Configs.Contracts;

namespace Client.Core.GameFlow.Contracts
{
    public class LevelArgumentData
    {
        public LevelArgumentData(LevelDefinition levelDefinition)
        {
            LevelDefinition = levelDefinition;
        }

        public LevelDefinition LevelDefinition { get; }
    }
}