using UnityEngine;

namespace XLib.UI.Interactable.Contracts {

	public interface IInteractableView {
		int Order { get; }
		Rect ScreenRect { get; }
		
		bool IsTapAvailable { get; }
		bool IsLongTapAvailable { get; }
		bool IsDragAvailable { get; }
		bool IsDragBeginAvailable { get; }
		bool IsDragMoveAvailable { get; }
		bool IsDragEndAvailable { get; }

		bool CanDropToTarget(IInteractableTargetView targetView);
		
		void Tap();
		void LongTapBegin();
		void LongTapEnd();
		void DragBegin();
		void DragMove(Vector2 dragPosition, float dragSpeed);
		void DragEnd(IInteractableTargetView targetView);
		void Cancel();
	}

}