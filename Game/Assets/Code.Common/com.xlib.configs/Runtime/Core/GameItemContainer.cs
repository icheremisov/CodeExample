using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Sheets.Contracts;

namespace XLib.Configs.Core {

	public abstract class GameItemContainer<T> : GameItemBaseContainer<T> where T : GameItemComponent {
		[HorizontalGroup("header"), VerticalGroup("header/right"), FoldoutGroup("header/right/Name & Description", true)]
		[SerializeField, HideInInlineEditors] protected string _name;
		[VerticalGroup("header/right"), FoldoutGroup("header/right/Name & Description")]
		[SerializeField, HideInInlineEditors] protected string _desc;
		public string Name => _name.ToString();
		public string Description => _desc.ToString();
	}

}