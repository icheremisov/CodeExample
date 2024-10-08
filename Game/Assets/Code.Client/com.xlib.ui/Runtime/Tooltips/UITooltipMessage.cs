using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using XLib.UI.Utils;

namespace XLib.UI.Tooltips {

	public class UITooltipMessage : UITooltipBase, IPointerClickHandler {
		[SerializeField, ChildGameObjectsOnly] private TMP_Text _header;
		[SerializeField, Required, ChildGameObjectsOnly] private TMP_Text _description;
		[SerializeField] private float _maxWidth = 800;

		private Action _onClick = null;

		public void Show(RectTransform rt, string header, string message, Action onClickAction = null) {
			base.Show(new TooltipClickData(rt));
			if (_header != null) {
				_header.SetActive(true);
				_header.SetTextWithResize(header);
			}

			_description.SetTextWithResize(message, _maxWidth);
			_onClick = onClickAction;
			base.UpdateTooltipPosition();
		}

		public void Show(RectTransform rt, string message, Action onClickAction = null) {
			base.Show(new TooltipClickData(rt));
			_description.SetTextWithResize(message, _maxWidth);
			_header.SetActive(false);
			_onClick = onClickAction;
		}

		public void OnPointerClick(PointerEventData eventData) => _onClick?.Invoke();
	}

}