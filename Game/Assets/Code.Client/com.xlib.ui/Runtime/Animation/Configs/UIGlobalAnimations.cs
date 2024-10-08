using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Unity.Utils;

namespace XLib.UI.Animation.Configs {
	
	[Serializable]
	public class UIGlobalAnimation {
		[SerializeField, PropertyOrder(-1)] private string _name;
		[SerializeField] private UIAnimationPart[] _parts;
		
		public string Name => _name;
		public UIAnimationPart[] Parts => _parts;
	}
	
	[CreateAssetMenu(menuName = "Configs/UIGlobalAnimations", fileName = "UIGlobalAnimations")]
	public class UIGlobalAnimations : SingletonScriptableObject<UIGlobalAnimations> {
		public const string AssetName = "UIGlobalAnimations";

		[SerializeField] private UIGlobalAnimation[] _animations;
		
		public UIGlobalAnimation[] Animations => _animations;

		public UIGlobalAnimation GetAnimation(string animationName) =>
			_animations.FirstOrDefault(a => a.Name == animationName);
	}

}