using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.UI.Controls {

	[ExecuteInEditMode]
	public class UISyncPosition : MonoBehaviour {

		[SerializeField, Required] private RectTransform _masterObject;
		[SerializeField, Required] private Vector2 _offset;
		private RectTransform _thisTm;

		private void LateUpdate() {
			UpdateView();
		}

		private void OnEnable() {
			UpdateView();
		}

		private void UpdateView() {
			if (_masterObject == null) return;

			if (_thisTm == null) _thisTm = (RectTransform)transform;

			_thisTm.anchoredPosition = _masterObject.anchoredPosition + _offset;
		}

	}

}