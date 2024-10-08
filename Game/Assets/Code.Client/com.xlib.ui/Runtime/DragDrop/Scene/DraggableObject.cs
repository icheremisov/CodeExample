using UnityEngine;
using UnityEngine.EventSystems;
using XLib.UI.Buttons;
using XLib.UI.DragDrop.Contracts;
using XLib.UI.DragDrop.Internal;

namespace XLib.UI.DragDrop.Scene {

	public class DraggableObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

		private IDraggableObject _draggableObject;
		private UITapHandler _tapHandler;

		private void Awake() {
			_draggableObject = this.GetExistingComponent<IDraggableObject>();
			_tapHandler = this.GetComponent<UITapHandler>();
		}

		public void OnBeginDrag(PointerEventData eventData) {
			if (DragDropRoot.S.BeginDrag(_draggableObject, eventData)) {
				_tapHandler.ResetLongTap();
			}
		}

		public void OnDrag(PointerEventData eventData) => DragDropRoot.S.Drag(eventData);

		public void OnEndDrag(PointerEventData eventData) => DragDropRoot.S.EndDrag(eventData);
	}

}