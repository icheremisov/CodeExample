using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Views {

	public enum UIBuiltInAction {
		Close,
		Menu,
		HomeScreen,
	}
		
	
	[RequireComponent(typeof(Button))]
	public class UIActionButton : MonoBehaviour {
		[SerializeField] private UIBuiltInAction _action;
		[SerializeField] private bool _transition;
		
		public UIBuiltInAction Action => _action;
		public bool Transition => _transition;
	}

}