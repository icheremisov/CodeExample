using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XLib.UI.Utils {

	public static class UiInputExtensions {

		public enum PointerOverUIResult {

			OnUI = 0,
			PassUI = 1,
			OnFilteredUI = 2

		}

		/// <summary>
		///     return true if user click (press and release) anythere in this frame
		/// </summary>
		public static bool IsAnyClickPresent() {
#if UNITY_EDITOR
			if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) return true;
#endif

			if (Input.touchCount > 0) {
				for (var i = 0; i < Input.touches.Length; i++) {
					var touch = Input.touches[i];
					if (touch.phase == TouchPhase.Ended) return true;
				}
			}

			return false;
		}

		/// <summary>
		///     return true if user pressed (press) anythere in this frame or before
		/// </summary>
		public static bool IsUserAnytherePressed() {
#if UNITY_EDITOR
			if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) return true;
#endif

			if (Input.touchCount > 0) {
				for (var i = 0; i < Input.touches.Length; i++) {
					var touch = Input.touches[i];
					if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved) return true;
				}
			}

			return false;
		}

		/// <summary>
		///     return true if user pressed anywhere in this frame
		/// </summary>
		public static bool IsMouseDownOrTouchBegan() {
#if UNITY_EDITOR

			if (Input.GetMouseButtonDown(0)) return true;

			return false;

#else
			if (Input.touchCount > 0)
			{
				for (var i = 0; i < Input.touches.Length; i++)
				{
					var touch = Input.touches[i];
					if (touch.phase == TouchPhase.Began)
					{
						return true;
					}
				}
			}

			return false;

#endif
		}

		/// <summary>
		///     return true if any of pointers located over UI object
		/// </summary>
		public static PointerOverUIResult IsPointerOverUIObject(bool hitOnlyFilter = false, int? pointerId = null) {
			var res = IsPointerOverUIGameObject(hitOnlyFilter);

			if (res != PointerOverUIResult.PassUI) return res;

#if UNITY_EDITOR
			if (pointerId.HasValue) {
				res = IsPointerOverUIGameObject(hitOnlyFilter, pointerId.Value);

				if (Input.GetMouseButton(0) && res != PointerOverUIResult.PassUI) return res;
			}
			else {
				res = IsPointerOverUIGameObject(hitOnlyFilter, 0);

				if (Input.GetMouseButton(0) && res != PointerOverUIResult.PassUI) return res;
			}
#endif

			if (Input.touchCount == 0) return PointerOverUIResult.PassUI;

			for (var i = 0; i < Input.touches.Length; i++) {
				var touch = Input.touches[i];
				if (pointerId.HasValue && touch.fingerId != pointerId.Value) continue;

				res = IsPointerOverUIGameObject(hitOnlyFilter, touch.fingerId);

				if (touch.phase == TouchPhase.Began && res != PointerOverUIResult.PassUI) return res;
			}

			return PointerOverUIResult.PassUI;
		}

		private static PointerOverUIResult IsPointerOverUIGameObject(bool hitOnlyFilter, int? pointerId = null) {
			if (!hitOnlyFilter) {
				if (pointerId.HasValue) return EventSystem.current.IsPointerOverGameObject(pointerId.Value) ? PointerOverUIResult.OnUI : PointerOverUIResult.PassUI;
				return EventSystem.current.IsPointerOverGameObject() ? PointerOverUIResult.OnUI : PointerOverUIResult.PassUI;
			}

			if (pointerId.HasValue ? EventSystem.current.IsPointerOverGameObject(pointerId.Value) : EventSystem.current.IsPointerOverGameObject()) {
				PointerEventData pointerData;

				if (pointerId.HasValue && Input.touches.IsValidIndex(pointerId.Value)) {
					var touch = Input.touches[pointerId.Value];

					pointerData = new PointerEventData(EventSystem.current) { pointerId = pointerId.Value, position = touch.position };
				}
				else
					pointerData = new PointerEventData(EventSystem.current) { pointerId = -1, position = Input.mousePosition };

				var results = new List<RaycastResult>(2);

				EventSystem.current.RaycastAll(pointerData, results);

				foreach (var r in results) {
					if (r.gameObject.GetComponent<UIClickFilter>()) return PointerOverUIResult.OnUI;
				}

				return PointerOverUIResult.OnFilteredUI;
			}

			return PointerOverUIResult.PassUI;
		}

		public enum DraggedDirection {
			Up,
			Down,
			Right,
			Left
		}

		public static DraggedDirection GetDragDirection(PointerEventData eventData) {
			Vector3 dragVector = (eventData.position - eventData.pressPosition).normalized;
			return GetDragDirection(dragVector);
		}

		public static DraggedDirection GetDragDirection(Vector3 dragVector) {
			var positiveX = Mathf.Abs(dragVector.x);
			var positiveY = Mathf.Abs(dragVector.y);
			DraggedDirection draggedDir;
			if (positiveX > positiveY) {
				draggedDir = (dragVector.x > 0) ? DraggedDirection.Right : DraggedDirection.Left;
			}
			else {
				draggedDir = (dragVector.y > 0) ? DraggedDirection.Up : DraggedDirection.Down;
			}

			Debug.Log(draggedDir);
			return draggedDir;
		}
		
	}

}