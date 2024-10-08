using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using XLib.UI.DragDrop.Internal;

namespace XLib.UI.Buttons {

	public class UITapHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler {

		[Header("Long Tap")]
		[SerializeField] private bool _hasLongTap = true;
		[SerializeField, ShowIf(nameof(_hasLongTap))] private float _tapTime = 0.5f;
        
		[Header("Double Tap")]
		[SerializeField] private bool _hasDoubleTap = true;
		[SerializeField, ShowIf(nameof(_hasDoubleTap))] private float _doubleTapTime = 0.5f;


		public bool HasLongTap { get => _hasLongTap; set => _hasLongTap = value; }
		public bool HasDoubleTap { get => _hasDoubleTap; set => _hasDoubleTap = value; }
		
		public float LongTapTime { get => _tapTime; set => _tapTime = value; }
		public float DoubleTapTime { get => _doubleTapTime; set => _doubleTapTime = value; }

		private bool _tapWaiting;
		private bool _tapHappened;
		
		private float _lastTapTime;

		public Action ShortTap { get; set; }
		public Action LongTap { get; set; }
		public Action DoubleTap { get; set; }
		private void OnDisable() {
			ResetLongTap();
		}
		private IEnumerator LongTapCoroutine() {
			_tapHappened = false;
			_tapWaiting = true;
			yield return new WaitForSeconds(_tapTime);
			
			DoLongTap();
		}

		public void OnPointerDown(PointerEventData eventData) {
			if (!_hasLongTap) return;
			this.StartThrowingCoroutine(LongTapCoroutine());
		}

		public void OnPointerExit(PointerEventData eventData) {
			if (!_hasLongTap) return;
			ResetLongTap();
		}
		
		public void OnPointerUp(PointerEventData eventData) {
			if (!_hasLongTap) return;
			if (_tapHappened) DoLongTapEnd();
		}

		public void OnPointerClick(PointerEventData eventData) {
			if (!_tapHappened) DoShortTap();
			ResetLongTap();
		}
		
		private void DoShortTap() {
			if (_hasDoubleTap && Time.unscaledTime - _lastTapTime < _doubleTapTime) {
				DoubleTap?.Invoke();
				_lastTapTime = 0;
			}
			else {
				ShortTap?.Invoke();
				_lastTapTime = Time.unscaledTime;
			}
		}

		private void DoLongTap() {
			if (!_hasLongTap) return;
			
			_tapWaiting = false;
			_tapHappened = true;
			
			DragDropRoot.S.CancelDrag();
			LongTap?.Invoke();
		}

		private void DoLongTapEnd() {
			if (!_hasLongTap) return;
			
		}

		public void ResetLongTap() {
			StopAllCoroutines();
			_tapWaiting = false;
			_tapHappened = false;
		}

	}

}