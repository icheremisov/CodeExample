using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLib.UI.Controls;

namespace XLib.UI.Buttons {

	[RequireComponent(typeof(Button))]
	public partial class UIButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler {
		[SerializeField] private TextMeshProUGUI _lbTitle;
		[SerializeField] private GameObject _badge;
		// [SerializeField] private LocalizeComponent _lbTitleLoc;
		[SerializeField] private UIMaterialReplace _matReplace;

		[Space, SerializeField, /*KeysPopup, */OnValueChanged(nameof(ApplyTerm))]
		private string _term;

		[SerializeField] private float _timeOut = 0.1f;

		[SerializeField, Required, Title("Content")] private RectTransform _content;

		private Button _button;
		private bool _interactable = true;
		private float _lastClickTime;

		private bool _wasPressed;
		public RectTransform Content { get => _content; set => _content = value; }

		public string TitleTerm {
			set {
				_term = value;

				ApplyTerm();
			}
			get => _term;
		}

		public string TitleText {
			set {
				// if (_lbTitleLoc) _lbTitleLoc.enabled = false;

				if (_lbTitle) _lbTitle.text = value;
			}
			get => _lbTitle != null ? _lbTitle.text : string.Empty;
		}

		public bool Interactable {
			get => _interactable;
			set {
				_interactable = value;

				UpdateButtonState();

				// if (!value && _uiButtonAnimation) {
				// 	_uiButtonAnimation.Stop();
				// 	_wasPressed = false;
				// }

				InteractableChanged?.Invoke(value);

				if (_matReplace) _matReplace.SetReplaced(!value);
			}
		}

		private void Awake() {
			_button = GetComponent<Button>();

			if (_button) _button.onClick.AddListener(TryClick);

			ApplyTerm();
			UpdateButtonState();
			SetBadgeActive(false);
		}

		private void OnDisable() {
			_wasPressed = false;
		}

		public void OnPointerDown(PointerEventData eventData) {
			if (_button == null) return;

			if (!_button.interactable) return;

			_wasPressed = true;

			// if (_uiButtonAnimation) _uiButtonAnimation.PlayDown();
		}

		public void OnPointerEnter(PointerEventData eventData) {
			if (_button == null) return;

			if (!_button.interactable) return;

			// if (_wasPressed && _uiButtonAnimation) _uiButtonAnimation.PlayDown();
		}

		public void OnPointerExit(PointerEventData eventData) {
			if (_button == null) return;

			if (!_button.interactable) return;

			// if (_wasPressed && _uiButtonAnimation) _uiButtonAnimation.PlayUp();
		}

		public void OnPointerUp(PointerEventData eventData) {
			if (_button == null) return;

			if (!_button.interactable) return;

			// if (_wasPressed && _uiButtonAnimation) _uiButtonAnimation.PlayUp();

			_wasPressed = false;
		}

		public event Action Click;
		public event Action<bool> InteractableChanged;

		private void TryClick() {
			if (Time.realtimeSinceStartup - _lastClickTime < _timeOut) return;

			_lastClickTime = Time.realtimeSinceStartup;
			Click?.Invoke();
		}

		private void UpdateButtonState() {
			if (_button == null) return;

			_button.interactable = _interactable;
		}

		private void ApplyTerm() {
			// if (!_lbTitleLoc) return;
			//
			// if (!_term.IsNullOrEmpty()) {
			// 	_lbTitleLoc.enabled = true;
			// 	_lbTitleLoc.Key = _term.ToLocKey();
			// 	_lbTitleLoc.OnLocalize(true);
			// }
			// else
			// 	_lbTitleLoc.enabled = false;
		}

		public void SetBadgeActive(bool active) {
			if (_badge != null) _badge.SetActive(active);
		}
	}

}