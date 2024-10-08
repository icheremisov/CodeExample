using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
/*

*/

namespace XLib.UI.Widgets {

	public class TitleWidget : MonoBehaviour {

		[SerializeField, Required] private TextMeshProUGUI _lbTitle;
		// [SerializeField, Required] private LocalizeComponent _lbTitleLoc;

		[Space, SerializeField, /*KeysPopup,*/ OnValueChanged(nameof(ApplyTerm))] private string _term;

		public string TitleTerm {
			set {
				_term = value;

				// _lbTitleLoc.enabled = true;
				// _lbTitleLoc.Key = value;
			}
		}

		public string TitleText {
			set {
				// _lbTitleLoc.enabled = false;
				_lbTitle.text = value;
			}
		}

		private void Awake() {
			if (!_term.IsNullOrEmpty()) TitleTerm = _term;
		}

		private void ApplyTerm() {
			// _lbTitleLoc.enabled = true;
			// _lbTitleLoc.Key = _term;
			// _lbTitleLoc.OnLocalize(true);
		}

	}

}