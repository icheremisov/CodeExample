using XLib.UI.Interactable.Contracts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.UI.Interactable {

	public abstract class InteractableView : MonoBehaviour, IInteractableView {
		[SerializeField, BoxGroup("Interactable view")] private bool _tapable = true;
		[SerializeField, BoxGroup("Interactable view")] private bool _longTapable = true;
		[SerializeField, BoxGroup("Interactable view")] protected bool _draggable = true;

		[SerializeField, BoxGroup("Drag")] private float _dragScale = 1.0f;

		public bool IsDragged { get; private set; }
		protected RectTransform RectTransform { get; private set; }

		public int Order => -RectTransform.GetSiblingIndex();
		public Rect ScreenRect => RectTransform.RectTransformToScreenSpace(InteractionController.S.Camera);

		protected virtual void Awake() {
			RectTransform = this.GetRectTransform();
		}

		protected virtual void OnEnable() {
			if (InteractionController.S != null) InteractionController.S.AddInteractableView(this);
		}

		protected virtual void OnDisable() {
			if (InteractionController.S != null) InteractionController.S.RemoveInteractableView(this);
		}

		public virtual bool IsTapAvailable => _tapable;
		public virtual bool IsLongTapAvailable => _longTapable;
		public virtual bool IsDragAvailable => _draggable;
		public abstract bool IsDragBeginAvailable { get; }
		public abstract bool IsDragMoveAvailable { get; }
		public abstract bool IsDragEndAvailable { get; }

		public void Tap() => OnTap();
		public void LongTapBegin() => OnLongTapBegin();
		public void LongTapEnd() => OnLongTapEnd();

		public void DragBegin() {
			IsDragged = true;
			OnDragBegin(_dragScale);
		}

		public void DragMove(Vector2 dragPosition, float dragSpeed) => OnDragMove(dragPosition, dragSpeed);

		public void DragEnd(IInteractableTargetView targetView) {
			IsDragged = false;
			OnDragEnd(targetView);
		}

		public void Cancel() {
			IsDragged = false;
			OnCancel();
		}

		public virtual bool CanDropToTarget(IInteractableTargetView targetView) => false;

		protected virtual void OnTap() { }
		protected virtual void OnLongTapBegin() { }
		protected virtual void OnLongTapEnd() { }
		protected virtual void OnDragBegin(float dragScale) { }
		protected virtual void OnDragMove(Vector2 dragPosition, float dragSpeed) { }
		protected virtual void OnDragEnd(IInteractableTargetView targetView) { }
		protected virtual void OnCancel() { }
	}

}