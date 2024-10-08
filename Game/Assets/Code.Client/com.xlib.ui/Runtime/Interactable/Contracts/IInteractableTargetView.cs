using UnityEngine;

namespace XLib.UI.Interactable.Contracts {

	public interface IInteractableTargetView {
		Rect ScreenRect { get; }

		void Highlight();
		void Unhighlight();
	}

}