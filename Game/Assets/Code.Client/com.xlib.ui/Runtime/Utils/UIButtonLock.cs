using UnityEngine.UI;
using XLib.Core.Utils;

namespace XLib.UI.Utils {

	public class UIButtonLock : ILockable {
		private readonly Button _button;
		private int _locks;
		public bool IsLocked => _locks > 0;

		public UIButtonLock(Button button) => _button = button;

		public LockInstance Lock() {
			_locks++;
			if (_locks == 1) _button.interactable = false;

			return new LockInstance(this);
		}

		public void Unlock(LockInstance inst) {
			_locks--;
			if (!IsLocked) _button.interactable = true;
		}
	}

}