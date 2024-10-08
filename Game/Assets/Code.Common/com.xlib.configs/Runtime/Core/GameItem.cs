using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Configs.Core
{
    [DisableInPlayMode]
    public abstract class GameItem : GameItemBase
    {
        [HorizontalGroup("header"), VerticalGroup("header/right"),
         FoldoutGroup("header/right/Name & Description", true), PropertyOrder(-10)]
        [SerializeField, HideInInlineEditors]
        protected string _name;

        [VerticalGroup("header/right"), FoldoutGroup("header/right/Name & Description"), PropertyOrder(-5)]
        [SerializeField, HideInInlineEditors]
        protected string _desc;

        public string Name => _name;
        public string Description => _desc;
    }
}