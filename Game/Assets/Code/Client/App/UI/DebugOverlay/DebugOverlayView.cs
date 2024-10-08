
using Client.Meta.UI.GlobalOverlay;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using XLib.BuildSystem;

#pragma warning disable 0162

namespace Client.Meta.UI.DebugOverlay {

	public class DebugOverlayView : MonoBehaviour, IDebugOverlayBehaviour {
		[SerializeField, Required] private TMP_Text _stepInfo;

		private void Awake() {
			Hide();
			_stepInfo.text = null;
			_stepInfo.SetActive(false);
		}

		public void Show() {
			if (!GameFeature.Cheats) return;
			this.SetActive(true);
		}

		public void Hide() {
			this.SetActive(false);
		}

		public void SetInfo(string text) {
			if (!GameFeature.Cheats) return;
			_stepInfo.text = text;
			_stepInfo.SetActive(!text.IsNullOrEmpty());
		}
	}

}