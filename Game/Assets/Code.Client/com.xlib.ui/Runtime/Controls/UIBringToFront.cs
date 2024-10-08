using UnityEngine;

namespace XLib.UI.Controls {

	/// <summary>
	///     move control to foreground when enabled
	/// </summary>
	public class UIBringToFront : MonoBehaviour {

		private void OnEnable() {
			transform.SetAsLastSibling();
		}

	}

}