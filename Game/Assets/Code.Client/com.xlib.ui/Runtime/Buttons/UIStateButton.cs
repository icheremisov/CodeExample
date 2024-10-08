using System;
using UnityEngine.UI;

namespace XLib.UI.Buttons {

	public class UIStateButton : Button {

		public event Action<UIStateButton> StateChanged;

		private SelectionState? _prevState;

		protected override void OnEnable() {
			base.OnEnable();

			DoStateChanged();
		}

		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);
			if (state == _prevState) return;
			
			_prevState = state;
			DoStateChanged();
		}		
		
		private void DoStateChanged() {
			StateChanged?.Invoke(this);
		}

	}

}