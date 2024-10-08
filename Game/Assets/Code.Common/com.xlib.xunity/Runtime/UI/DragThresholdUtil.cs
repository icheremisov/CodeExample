using UnityEngine;
using UnityEngine.EventSystems;

namespace XLib.Unity.UI {

	public class DragThresholdUtil : MonoBehaviour {

		[SerializeField] private float _mediumSizedScreenDevicesDpi = 160f; // DPI value for medium sized screen devices

		private void Start() {
			var defaultValue = EventSystem.current.pixelDragThreshold;
			var pixelDragThreshold = Mathf.Max(defaultValue, (int)(defaultValue * Screen.dpi / _mediumSizedScreenDevicesDpi));

			Debug.Log($">>> Pixel drag threshold: {pixelDragThreshold}");

			EventSystem.current.pixelDragThreshold = pixelDragThreshold;
		}

	}

}