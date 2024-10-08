using UnityEngine;
using XLib.Core.Utils;

namespace XLib.UI.Utils {

	public class UICanvasLock : ILockable {
		private readonly CanvasGroup _canvasGroup;
		private int _locks;
		public bool IsLocked => _locks > 0;

		public UICanvasLock(CanvasGroup canvasGroup) => _canvasGroup = canvasGroup;

		public LockInstance Lock() {
			_locks++;
			if (_locks == 1) _canvasGroup.blocksRaycasts = false;
			return new LockInstance(this);
		}

		public void Unlock(LockInstance inst) {
			_locks--;
			if (!IsLocked) _canvasGroup.blocksRaycasts = true;
		}
	}

}