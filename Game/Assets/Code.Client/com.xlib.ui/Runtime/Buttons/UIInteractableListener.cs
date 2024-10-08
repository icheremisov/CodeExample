using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.UI.Buttons {

	[RequireComponent(typeof(UIButton))]
	public class UIInteractableListener : MonoBehaviour {

		[SerializeField, Required, ListDrawerSettings(DefaultExpandedState = true), InlineProperty] private GameObject[] _objectsToActivate;
		private UIButton _uiButton;
		private UIButton UiButton => _uiButton ??= GetComponent<UIButton>();

		private void Awake() {
			UiButton.InteractableChanged += SetObjectsEnabledState;
			SetObjectsEnabledState(UiButton.Interactable);
		}

		private void OnEnable() {
			SetObjectsEnabledState(UiButton.Interactable);
		}

		private void SetObjectsEnabledState(bool state) {
			_objectsToActivate.SetActive(state);
			this.GetRectTransform().RecursiveUpdateLayout();
		}

	}

}