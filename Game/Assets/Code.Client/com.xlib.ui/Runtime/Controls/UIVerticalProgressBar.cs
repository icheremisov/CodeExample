using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace XLib.UI.Controls {

	public class UIVerticalProgressBar : MonoBehaviour {

		[SerializeField] private RectTransform _imgTm;
		[SerializeField] private RectTransform _pbRoot;
		[SerializeField] private TextMeshProUGUI _txtValue;

		[SerializeField] private float _minHeight;
		[SerializeField] private float _maxHeight;

		public float value = 0.5f;

		[Button]
		private void SeT() {
			SetBarValue(value);
		}

		public void SetBarValue(float value, string text = null) {
			var sizeDelta = _imgTm.sizeDelta;
			_imgTm.sizeDelta = new Vector2(sizeDelta.x, Mathf.Min(_maxHeight, Mathf.Max(_minHeight, _pbRoot.rect.height * value)));

			_txtValue.text = text;
		}

	}

}