using System;
using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Widgets {

	public class CloseableWidget : MonoBehaviour {

		[SerializeField] private Button[] _closeButtons;
		[SerializeField] private GameObject[] _closeButtonsToHide;
		[SerializeField] private bool _closeEnabled = true;

		public bool CloseEnabled {
			get => _closeEnabled;
			set {
				if (_closeEnabled != value) {
					_closeEnabled = value;
					UpdateState();
				}
			}
		}

		private void Awake() {
			foreach (var button in _closeButtons) {
				if (!button) {
					UILogger.LogError($"null button in CloseableView/_closeButtons {this.GetFullPath()}");
					continue;
				}

				button.onClick.AddListener(DoCloseClick);
			}

			UpdateState();
		}

		public event Action CloseClick;

		private void DoCloseClick() {
			if (_closeEnabled) CloseClick?.Invoke();
		}

		private void UpdateState() {
			void SetCloseEnabled(GameObject obj) {
				if (obj)
					obj.SetActive(_closeEnabled);
				else
					UILogger.LogError($"null button in CloseableView/_closeButtonsToHide {this.GetFullPath()}");
			}

			_closeButtonsToHide?.ForEach(SetCloseEnabled);
		}

	}

}