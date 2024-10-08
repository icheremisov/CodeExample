using XLib.UI.Interactable.Contracts;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.UI.Interactable {

	public class InteractableTargetView : MonoBehaviour, IInteractableTargetView {
		[SerializeField, BoxGroup("Interactable target view")] private CanvasGroup _highlightCanvasGroup;

		private RectTransform _rectTransform;
		
		public Rect ScreenRect => _rectTransform.RectTransformToScreenSpace(InteractionController.S.Camera);

		protected virtual void Awake() {
			_rectTransform = this.GetRectTransform();
			if (_highlightCanvasGroup != null) _highlightCanvasGroup.alpha = 0;
		}

		public void Highlight() {
			if (_highlightCanvasGroup != null) _highlightCanvasGroup.DOFade(1, 0.1f);
		}

		public void Unhighlight() {
			if (_highlightCanvasGroup != null) _highlightCanvasGroup.DOFade(0, 0.1f);
		}
		
		protected virtual void OnEnable() {
			if (InteractionController.S != null) InteractionController.S.AddTargetView(this);
		}
		protected virtual void OnDisable() {
			if (InteractionController.S != null) InteractionController.S.RemoveTargetView(this);
		}
	}

}