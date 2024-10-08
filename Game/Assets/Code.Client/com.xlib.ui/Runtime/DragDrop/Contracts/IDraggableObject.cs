using UnityEngine;

namespace XLib.UI.DragDrop.Contracts {

	public interface IDraggableObject {

		bool CanDrag { get; }

		IDraggableObjectCopy MakeDraggableCopy(RectTransform dragRoot);
		
		void DragStarted();
		void DragCompleted();

	}

}