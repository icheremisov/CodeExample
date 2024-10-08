using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using XLib.UI.DragDrop.Contracts;
using XLib.Unity.Scene;

namespace XLib.UI.DragDrop.Internal {

	[RequireComponent(typeof(RectTransform))]
	internal class DragDropRoot : Singleton<DragDropRoot>, IDragDropRoot {

		private Vector2 _dragOffset;
		private IDraggableObjectCopy _view;
		private IDraggableObject _draggableObject;
		
		private Camera _camera;

		public event DropHandler ObjectDropped;
		
		public bool DragEnabled { get; set; } = true;

		private void Start() {
			_camera = this.GetCanvasCamera();
		}

		public void CancelDrag() {
			_view?.Destroy();
			_view = null;

			_draggableObject?.DragCompleted();
			_draggableObject = null;

			_view = null;
			_dragOffset = Vector2.zero;
		}

		public bool BeginDrag(IDraggableObject view, PointerEventData eventData) {
			if (!DragEnabled || !view.CanDrag) return false;

			_draggableObject = view;
			
			_view = _draggableObject.MakeDraggableCopy((RectTransform)transform);

			_draggableObject.DragStarted();
			
			RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, eventData.position, _camera, out var point);
			
			_dragOffset = (_view.Transform.position.ToXY() - point.ToXY()) * _view.Transform.GetLocalScale();

			return true;
		}

		public void EndDrag(PointerEventData eventData) {
			if (!DragEnabled || _view == null) return;

			var id = _view.ObjectId;
			var draggableObject = _draggableObject;
			CancelDrag();

			if (id == null) return;

			List<RaycastResult> raycastResults = new(8);
			EventSystem.current.RaycastAll(eventData, raycastResults);

			var slot = raycastResults.SelectNotNull(x => x.gameObject.GetComponent<IDragDropTarget>()).FirstOrDefault();


			var targetId = slot?.CanDrop(id, draggableObject) == true ? slot.TargetId : null; 
			ObjectDropped?.Invoke(id, targetId);
		}

		public void Drag(PointerEventData eventData) {
			if (_view == null) return;
			if (!RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, eventData.position, _camera, out var point)) return;

			_view.Transform.position = point + _dragOffset.ToXY0();
		}

	}

}