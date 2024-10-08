namespace XLib.UI.Interactable.Contracts {

	public interface IInteractionController {
		void AddInteractableView(IInteractableView interactableView);
		void RemoveInteractableView(IInteractableView interactableView);
		void AddTargetView(IInteractableTargetView interactableTargetView);
		void RemoveTargetView(IInteractableTargetView interactableTargetView);
	}

}