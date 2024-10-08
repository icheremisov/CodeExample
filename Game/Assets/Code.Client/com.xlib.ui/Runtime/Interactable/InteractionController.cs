using System.Collections;
using System.Collections.Generic;
using System.Linq;
using XLib.UI.Interactable.Contracts;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using XLib.Unity.Scene;

namespace XLib.UI.Interactable {

	public class InteractionController : DisableAbleSingleton<InteractionController>,
										 IInteractionController,
										 IPointerDownHandler,
										 IPointerUpHandler,
										 IPointerExitHandler,
										 IPointerEnterHandler,
										 IPointerMoveHandler,
										 IDragHandler,
										 ICancelHandler,
										 IInitializePotentialDragHandler {
		[SerializeField, BoxGroup("General"), Required] private Camera _camera;
		[SerializeField, BoxGroup("General")] private GameObject _backObject;
		[SerializeField, BoxGroup("Long tap")] private float _loggTapTime = 0.5f;
		[SerializeField, BoxGroup("Drag")] private float _dragDeadzone = 32f;
		[SerializeField, BoxGroup("Drag")] private float _dragSpeed = 10f;
		[SerializeField, BoxGroup("Drag")] private float _dragDropPercent = 0.5f;

		public Camera Camera => _camera;

		private readonly List<IInteractableView> _interactableViews = new(16);
		private readonly List<IInteractableTargetView> _targetViews = new(16);

		private Coroutine _longTapCoroutine;

		private int _pointerId = -1;
		private Vector2 _startTapPosition;
		private Vector2 _dragPosition;
		private IInteractableView _currentInteractableView;
		private IInteractableTargetView _currentTargetView;

		protected bool IsTapBegin { get; private set; }
		protected bool IsLongTapBegin { get; private set; }
		protected bool IsDragBegin { get; private set; }

		protected override void Awake() {
			if (S != null) S.OnDisable();
			base.Awake();
		}

		protected override void OnEnable() {
			if (S != null) S.OnDisable();
			base.OnEnable();
		}

		private IEnumerator LongTapCoroutine() {
			yield return new WaitForSeconds(_loggTapTime);
			_longTapCoroutine = null;
			LongTapBegin();
		}

		private void LongTapReset() {
			IsLongTapBegin = false;
			IsTapBegin = false;

			if (_longTapCoroutine == null) return;
			StopCoroutine(_longTapCoroutine);
			_longTapCoroutine = null;
		}

		private void DragReset() {
			IsDragBegin = false;
			IsTapBegin = false;
		}

		private IInteractableView GetInteractableView(Vector2 position) {
			return _interactableViews.OrderBy(interactableView => interactableView.Order).FirstOrDefault(interactableView => interactableView.ScreenRect.Contains(position));
		}

		private IInteractableTargetView GetTargetView(IInteractableView interactableView) {
			var target = _targetViews.Where(interactableView.CanDropToTarget).MaxByOrDefault(targetView => InteractionExtensions.IntersectsPercent(interactableView, targetView));
			return target != null && InteractionExtensions.IntersectsPercent(interactableView, target) > _dragDropPercent ? target : null;
		}

		public void OnPointerDown(PointerEventData eventData) {
			if (Input.touchCount == 1 || eventData.button == PointerEventData.InputButton.Left) IsLongTapBegin = false;

			if (_currentInteractableView != null) return;

			_currentInteractableView = GetInteractableView(eventData.position);

			if (_currentInteractableView == null) {
				ExecuteEvents.Execute(_backObject, eventData, ExecuteEvents.pointerDownHandler);
				return;
			}

			_pointerId = eventData.pointerId;
			IsTapBegin = true;
			_startTapPosition = eventData.position;

			if (_currentInteractableView.IsLongTapAvailable) {
				_longTapCoroutine = this.StartThrowingCoroutine(LongTapCoroutine());
			}
		}

		public void OnPointerUp(PointerEventData eventData) {
			if (_currentInteractableView == null || eventData.pointerId != _pointerId) {
				ExecuteEvents.Execute(_backObject, eventData, ExecuteEvents.pointerUpHandler);
				return;
			}

			if (IsDragBegin && _currentInteractableView.IsDragEndAvailable)
				DragEnd();
			else if (IsLongTapBegin)
				LongTapEnd();
			else if (IsTapBegin) {
				// при быстром прокликивании тап может произойти для другой карты
				var currentObject = GetInteractableView(eventData.position);
				if (currentObject != _currentInteractableView)
					Reset();
				else
					Tap();
			}

			Reset();
		}

		public void OnPointerExit(PointerEventData eventData) {
			if (_currentInteractableView == null || eventData.pointerId != _pointerId) {
				ExecuteEvents.Execute(_backObject, eventData, ExecuteEvents.pointerExitHandler);
				return;
			}

			if (!_currentInteractableView.IsDragEndAvailable) {
				Reset();
				return;
			}

			Cancel();
			Reset();
		}

		public void OnPointerEnter(PointerEventData eventData) {
			if (_currentInteractableView == null || eventData.pointerId != _pointerId) ExecuteEvents.Execute(_backObject, eventData, ExecuteEvents.pointerEnterHandler);
		}

		void IDragHandler.OnDrag(PointerEventData eventData) {
			if (_currentInteractableView == null || eventData.pointerId != _pointerId) {
				ExecuteEvents.Execute(_backObject, eventData, ExecuteEvents.dragHandler);
				return;
			}

			if (!_currentInteractableView.IsDragMoveAvailable) return;
			if (IsDragBegin) _dragPosition = Camera.ScreenToWorldPoint(eventData.position);
		}

		public void OnPointerMove(PointerEventData eventData) {
			if (_currentInteractableView == null || eventData.pointerId != _pointerId) {
				ExecuteEvents.Execute(_backObject, eventData, ExecuteEvents.pointerMoveHandler);
				return;
			}

			if (!_currentInteractableView.IsDragMoveAvailable) return;

			if (IsDragBegin)
				_dragPosition = Camera.ScreenToWorldPoint(eventData.position);
			else if (_currentInteractableView.IsDragBeginAvailable && IsTapBegin && !IsLongTapBegin && Vector2.Distance(_startTapPosition, eventData.position) > _dragDeadzone) {
				_dragPosition = Camera.ScreenToWorldPoint(eventData.position);
				DragBegin();
			}
		}

		public void OnCancel(BaseEventData eventData) {
			if (_currentInteractableView == null) {
				ExecuteEvents.Execute(_backObject, eventData, ExecuteEvents.cancelHandler);
				return;
			}

			Cancel();
			Reset();
		}

		public void OnInitializePotentialDrag(PointerEventData eventData) {
			eventData.useDragThreshold = false;
		}

		private void Reset() {
			ResetTarget();
			DragReset();
			LongTapReset();
			_currentInteractableView = null;
			_pointerId = -1;
		}

		private void Update() {
			if (_currentInteractableView == null || !IsDragBegin) return;

			DragMove();
		}

		private void UpdateTarget() {
			var targetView = GetTargetView(_currentInteractableView);

			if (targetView == _currentTargetView) return;

			_currentTargetView?.Unhighlight();
			_currentTargetView = null;

			if (targetView == null) return;

			_currentTargetView = targetView;
			_currentTargetView.Highlight();
		}

		private void ResetTarget() {
			_currentTargetView?.Unhighlight();
			_currentTargetView = null;
		}

		public void AddInteractableView(IInteractableView interactableView) {
			_interactableViews.Add(interactableView);
		}

		public void RemoveInteractableView(IInteractableView interactableView) {
			_interactableViews.Remove(interactableView);
		}

		public void AddTargetView(IInteractableTargetView interactableTargetView) {
			_targetViews.Add(interactableTargetView);
		}

		public void RemoveTargetView(IInteractableTargetView interactableTargetView) {
			_targetViews.Remove(interactableTargetView);
		}

		protected virtual void Tap() {
			if (_currentInteractableView == null) return;
			if (!_currentInteractableView.IsTapAvailable) return;

			LongTapReset();
			_currentInteractableView.Tap();
			OnTap(_currentInteractableView);
		}

		protected virtual void LongTapBegin() {
			IsLongTapBegin = true;
			_currentInteractableView.LongTapBegin();
			OnLongTapBegin(_currentInteractableView);
		}

		private void LongTapEnd() {
			if (_currentInteractableView == null) return;
			_currentInteractableView.LongTapEnd();
			OnLongTapEnd(_currentInteractableView);
		}

		private void DragBegin() {
			IsDragBegin = true;
			LongTapReset();
			_currentInteractableView.DragBegin();
			OnDragBegin(_currentInteractableView);
		}

		private void DragMove() {
			UpdateTarget();
			_currentInteractableView.DragMove(_dragPosition, _dragSpeed);
			OnDragMove(_currentInteractableView, _dragPosition, _dragSpeed);
		}

		private void DragEnd() {
			_currentInteractableView.DragEnd(_currentTargetView);
			OnDragEnd(_currentInteractableView, _currentTargetView);
		}

		private void Cancel() {
			_currentInteractableView.Cancel();
			OnCancel(_currentInteractableView);
		}

		protected virtual void OnTap(IInteractableView interactableView) { }
		protected virtual void OnLongTapBegin(IInteractableView interactableView) { }
		protected virtual void OnLongTapEnd(IInteractableView interactableView) { }
		protected virtual void OnDragBegin(IInteractableView interactableView) { }
		protected virtual void OnDragMove(IInteractableView interactableView, Vector2 dragPosition, float dragSpeed) { }
		protected virtual void OnDragEnd(IInteractableView interactableView, IInteractableTargetView targetView) { }
		protected virtual void OnCancel(IInteractableView interactableView) { }
	}

}