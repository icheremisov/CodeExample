using UnityEngine;

namespace XLib.UI.Views {

	public enum UIElementIdentifierIds {
		None,
		ButtonBack,
		ButtonHome,
		ButtonBurgerMenu,
		ButtonClose,
		ButtonGoto,
		ButtonClaim,
		ButtonAuto,
		ButtonHider,

		Special1 = 1000,
		Special2 = 1001,
		Special3 = 1002,
		Special4 = 1003,
	}

	public class UIElementIdentifier : MonoBehaviour {
		[SerializeField] private UIElementIdentifierIds _id;
		public UIElementIdentifierIds ID => _id;
	}

}