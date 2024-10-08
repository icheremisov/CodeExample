using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Controls {

	[RequireComponent(typeof(ScrollRect))]
	public class UIScrollRectMaskHider : MonoBehaviour {
		[SerializeField] private Vector4 _maskPaddingHidden;
		[SerializeField] private Vector4 _maskPaddingShown;
		[SerializeField] private bool _replaceMaskPadding;
		[SerializeField] private RectMask2D _mask;

		private ScrollRect _scrollRect;
		private ScrollRect ScrollRect => _scrollRect != null ? _scrollRect : _scrollRect = this.GetExistingComponent<ScrollRect>();

		private Behaviour _maskComponent;

		private bool _needUpdate;
		private bool _wasScrollEnabled;

		private void Start() {
			var scrollRect = ScrollRect;
			if (scrollRect.viewport) {
				_maskComponent = scrollRect.viewport.GetComponent<RectMask2D>();
				if (!_maskComponent) _maskComponent = scrollRect.viewport.GetComponent<Mask>();
				_needUpdate = true;
			}
		}

		private void OnEnable() {
			ScrollRect.onValueChanged.AddListener(OnScrollValueChanged);
			_wasScrollEnabled = _scrollRect.enabled;
			_needUpdate = true;
			UpdateView();
		}

		private void OnDisable() {
			ScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
		}

		private void Update() {
			if (!_needUpdate) _needUpdate = _wasScrollEnabled != _scrollRect.enabled;
			UpdateView();
		}

		private void OnScrollValueChanged(Vector2 scrollDelta) {
			_needUpdate = true;
			UpdateView();
		}

		private void UpdateView() {
			if (!_needUpdate || _maskComponent == null) return;
			_needUpdate = false;
			_wasScrollEnabled = _scrollRect.enabled;

			if (!_scrollRect.enabled) {
				_maskComponent.enabled = false;
				return;
			}

			var maskVisible = _scrollRect.horizontal && _scrollRect.normalizedPosition.x < 1;
			if (!maskVisible && _scrollRect.vertical && _scrollRect.normalizedPosition.y < 1) maskVisible = true;

			if (!_replaceMaskPadding)
				_maskComponent.enabled = maskVisible;
			else {
				if (_mask != null) {
					_mask.padding = maskVisible ? _maskPaddingShown : _maskPaddingHidden;
				}
			}
		}
	}

}