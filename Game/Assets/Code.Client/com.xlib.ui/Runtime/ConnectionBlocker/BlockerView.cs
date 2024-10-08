using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.UI.Types;

namespace XLib.UI.ConnectionBlocker {

	internal class BlockerView : MonoBehaviour, IBlockerView {
		[SerializeField, Required] private GameObject _view;
		
		private readonly HashSet<ScreenLockTag> _lockers = new();
		private readonly HashSet<ScreenLockTag> _unlockers = new();

		// TODO remove forever after MVP2
		public bool IsBlocked => !_lockers.IsNullOrEmpty();

		private bool IsLocked => _lockers.Count > 0 && _unlockers.Count == 0;
		
		private void Awake() {
			UpdateVisible();
		}

		private void UpdateVisible() {
			_view.SetActive(IsLocked);
		}

		public void UnlockAll() {
			_lockers.Clear();
			_unlockers.Clear();
			UpdateVisible();
		}

		public bool HasTag(ScreenLockTag t) => _lockers.Contains(t);

		public void Open(ScreenLockTag t) {
			if (!this) return;
			_lockers.AddOnce(t);
			UpdateVisible();
		}

		public void Close(ScreenLockTag t) {
			if (!this) return;
			_lockers.Remove(t);
			UpdateVisible();
		}

		public void DisableOpening(ScreenLockTag t) {
			if (!this) return;
			_unlockers.AddOnce(t);
			UpdateVisible();
		}

		public void EnableOpening(ScreenLockTag t) {
			if (!this) return;
			_unlockers.Remove(t);
			UpdateVisible();
		}
	}

}