using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XLib.UI.Controls {

	public class UIRaycastRedirect : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler {

		public delegate void EventHandler(PointerEventData eventData);

		public EventHandler PointerDown { get; set; }
		public EventHandler PointerClick { get; set; }
		public EventHandler PointerUp { get; set; }
		public EventHandler PointerEnter { get; set; }
		public EventHandler PointerExit { get; set; }

		public void OnPointerClick(PointerEventData eventData) {
			UILogger.Log("PointerClick");
			PointerClick?.Invoke(eventData);
		}

		public void OnPointerDown(PointerEventData eventData) {
			PointerDown?.Invoke(eventData);
		}

		public void OnPointerEnter(PointerEventData eventData) {
			PointerEnter?.Invoke(eventData);
		}

		public void OnPointerExit(PointerEventData eventData) {
			PointerExit?.Invoke(eventData);
		}

		public void OnPointerUp(PointerEventData eventData) {
			PointerUp?.Invoke(eventData);
		}

	}

}