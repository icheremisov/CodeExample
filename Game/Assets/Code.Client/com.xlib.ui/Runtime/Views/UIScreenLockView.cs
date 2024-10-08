using Sirenix.OdinInspector;
using UnityEngine;
using XLib.UI.Controls;

namespace XLib.UI.Views {

	public class UIScreenLockView : MonoBehaviour {

		[SerializeField, Required] private GameObject _locker;
		[SerializeField, Required] private UIRaycastRedirect _redirect;

		public UIRaycastRedirect Redirect => _redirect;

		public void SetLockerVisible(bool v) {
			if (_locker) _locker.SetActive(v);
		}

	}

}