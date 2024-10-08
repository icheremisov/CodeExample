using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XLib.UI.Controls {

	[Flags]
	public enum UIInputEvent : uint {
		UIPointerUp = 1 << 0,
		UIPointerDown = 1 << 1,
		UIPointerClick = 1 << 2,
		UIPointerEnter = 1 << 3,
		UIPointerExit = 1 << 4,
		UIPointerMove = 1 << 5,
		UIPointerScroll = 1 << 6,
		UIAllPointer = UIPointerUp | UIPointerDown | UIPointerClick | UIPointerEnter | UIPointerExit | UIPointerMove,

		UIBeginDrag = 1 << 7,
		UIEndDrag = 1 << 8,
		UIDrag = 1 << 9,
		UIAllDrags = UIBeginDrag | UIEndDrag | UIDrag,

		TapStarted = 1 << 10,
		Tap = 1 << 11,
		LongTap = 1 << 12,

		Select = 1 << 13,
		Deselect = 1 << 14,

		Submit = 1 << 15,
		Cancel = 1 << 16,
		MoveAxis = 1 << 17,

		All = 0xffffffff,
	}

	public class UIRaycastInterceptor : UIEmptyGraphic,
										IPointerUpHandler,
										IPointerDownHandler,
										IPointerClickHandler,
										IPointerEnterHandler,
										IPointerExitHandler,
										IPointerMoveHandler,
										IBeginDragHandler,
										IEndDragHandler,
										IDragHandler,
										ISubmitHandler,
										ICancelHandler,
										IMoveHandler,
										IScrollHandler,
										ISelectHandler,
										IDeselectHandler {
		[ShowInInspector, ReadOnly] private GameObject _uiTarget;
		private Func<UIInputEvent, BaseEventData, bool> _predicate;
		public GameObject Target => _uiTarget;
		private readonly List<RaycastResult> _results = new(16);
		[ShowInInspector, ReadOnly] private BaseRaycaster[] _raycasters;

		public override bool Raycast(Vector2 sp, Camera eventCamera) {
			var raycaster = Raycaster;
			if (raycaster == null) return false;
			foreach (var child in _uiTarget.GetComponentsInChildren<Graphic>()) {
				if (!child.raycastTarget) continue;
				var contains = RectTransformUtility.RectangleContainsScreenPoint(child.rectTransform, sp, raycaster.eventCamera);
				if (contains && child.Raycast(sp, raycaster.eventCamera)) return true;
			}

			return false;
		}

		public BaseRaycaster Raycaster => _raycasters.FirstOrDefault(raycaster => raycaster.isActiveAndEnabled);

		public void StartUIFilter(GameObject target, Func<UIInputEvent, BaseEventData, bool> predicate) {
			_uiTarget = target;
			_predicate = predicate;
			_raycasters = target.GetComponentsInParent<BaseRaycaster>(true);
		}

		private void HandleEvent<TInterface>(PointerEventData evt, UIInputEvent filter, ExecuteEvents.EventFunction<TInterface> func)
			where TInterface : class, IEventSystemHandler {
			var raycaster = Raycaster;
			if (raycaster == null) return;

			var eventData = GetEventData(evt, raycaster);
			_results.Clear();
			raycaster.Raycast(eventData, _results);

			if (_results.Count <= 0) {
				Debug.LogWarning($"Not found raycast from raycaster {raycaster}\n{eventData}");
				return;
			}

			_results.RemoveAll(result => !IsTargetChildren(result.gameObject));
			_results.Sort(RaycastComparer);
			
			var target = _results.Count > 0 ? _results[0].gameObject : _uiTarget;

			var handler = ExecuteEvents.GetEventHandler<TInterface>(target);
			if (handler == null) return;
			if (_predicate != null && !_predicate(filter, eventData)) return;

			ExecuteEvents.Execute(handler, eventData, func);
		}

		private void HandleEvent<TInterface>(BaseEventData evt, UIInputEvent filter, ExecuteEvents.EventFunction<TInterface> func)
			where TInterface : class, IEventSystemHandler {
			var eventData = new BaseEventData(EventSystem.current);
			var handler = ExecuteEvents.GetEventHandler<TInterface>(_uiTarget);
			if (handler == null) return;
			if (_predicate != null && !_predicate(filter, eventData)) return;
			ExecuteEvents.Execute(handler, eventData, func);
		}

		private bool IsTargetChildren(GameObject obj) {
			var t = obj.transform;
			var parent = _uiTarget.transform;
			while (t != null) {
				if (t == parent) return true;
				t = t.parent;
			}

			return false;
		}

		private static int RaycastComparer(RaycastResult lhs, RaycastResult rhs) {
			if (lhs.sortingLayer != rhs.sortingLayer) {
				return SortingLayer.GetLayerValueFromID(rhs.sortingLayer)
					.CompareTo(SortingLayer.GetLayerValueFromID(lhs.sortingLayer));
			}

			if (lhs.sortingOrder != rhs.sortingOrder) return rhs.sortingOrder.CompareTo(lhs.sortingOrder);

			if (lhs.depth != rhs.depth && lhs.module.rootRaycaster == rhs.module.rootRaycaster) return rhs.depth.CompareTo(lhs.depth);

			return lhs.distance != rhs.distance ? lhs.distance.CompareTo(rhs.distance) : lhs.index.CompareTo(rhs.index);
		}

		private PointerEventData GetEventData(PointerEventData eventData, BaseRaycaster raycaster) {
			var pressRaycast = eventData.pointerPressRaycast;
			var currentRaycast = eventData.pointerCurrentRaycast;

			pressRaycast.module = raycaster;
			currentRaycast.module = raycaster;
			
			pressRaycast.gameObject = pressRaycast.gameObject == gameObject ? _uiTarget : pressRaycast.gameObject;
			currentRaycast.gameObject = currentRaycast.gameObject == gameObject ? _uiTarget : currentRaycast.gameObject;

			return new PointerEventData(EventSystem.current) {
				pointerEnter = eventData.pointerEnter == gameObject ? _uiTarget : eventData.pointerEnter,
				rawPointerPress = eventData.rawPointerPress == gameObject ? _uiTarget : eventData.rawPointerPress,
				pointerDrag = eventData.pointerDrag == gameObject ? _uiTarget : eventData.pointerDrag,
				pointerCurrentRaycast = currentRaycast,
				pointerPressRaycast = pressRaycast,
				eligibleForClick = eventData.eligibleForClick,
				pointerId = eventData.pointerId,
				position = eventData.position,
				delta = eventData.delta,
				pressPosition = eventData.pressPosition,
				clickTime = eventData.clickTime,
				clickCount = eventData.clickCount,
				scrollDelta = eventData.scrollDelta,
				useDragThreshold = eventData.useDragThreshold,
				dragging = eventData.dragging,
				button = eventData.button,
				pointerPress = eventData.pointerPress == gameObject ? _uiTarget : eventData.pointerPress
			};
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData e) => HandleEvent(e, UIInputEvent.UIPointerUp, ExecuteEvents.pointerUpHandler);
		void IPointerDownHandler.OnPointerDown(PointerEventData e) => HandleEvent(e, UIInputEvent.UIPointerDown, ExecuteEvents.pointerDownHandler);
		void IPointerClickHandler.OnPointerClick(PointerEventData e) => HandleEvent(e, UIInputEvent.UIPointerClick, ExecuteEvents.pointerClickHandler);
		void IPointerEnterHandler.OnPointerEnter(PointerEventData e) => HandleEvent(e, UIInputEvent.UIPointerEnter, ExecuteEvents.pointerEnterHandler);
		void IPointerExitHandler.OnPointerExit(PointerEventData e) => HandleEvent(e, UIInputEvent.UIPointerExit, ExecuteEvents.pointerExitHandler);
		void IPointerMoveHandler.OnPointerMove(PointerEventData e) => HandleEvent(e, UIInputEvent.UIPointerMove, ExecuteEvents.pointerMoveHandler);
		void IBeginDragHandler.OnBeginDrag(PointerEventData e) => HandleEvent(e, UIInputEvent.UIBeginDrag, ExecuteEvents.beginDragHandler);
		void IEndDragHandler.OnEndDrag(PointerEventData e) => HandleEvent(e, UIInputEvent.UIEndDrag, ExecuteEvents.endDragHandler);
		void IDragHandler.OnDrag(PointerEventData e) => HandleEvent(e, UIInputEvent.UIDrag, ExecuteEvents.dragHandler);
		void ISubmitHandler.OnSubmit(BaseEventData e) => HandleEvent(e, UIInputEvent.Submit, ExecuteEvents.submitHandler);
		void ICancelHandler.OnCancel(BaseEventData e) => HandleEvent(e, UIInputEvent.Cancel, ExecuteEvents.cancelHandler);
		void IMoveHandler.OnMove(AxisEventData e) => HandleEvent(e, UIInputEvent.MoveAxis, ExecuteEvents.moveHandler);
		void IScrollHandler.OnScroll(PointerEventData e) => HandleEvent(e, UIInputEvent.UIPointerScroll, ExecuteEvents.scrollHandler);
		void ISelectHandler.OnSelect(BaseEventData e) => HandleEvent(e, UIInputEvent.Select, ExecuteEvents.selectHandler);
		void IDeselectHandler.OnDeselect(BaseEventData e) => HandleEvent(e, UIInputEvent.Deselect, ExecuteEvents.deselectHandler);
	}

}