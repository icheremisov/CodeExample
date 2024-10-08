using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.UI.Controls {

	public class UITicksList : MonoBehaviour {

		[SerializeField, Required, ChildGameObjectsOnly] private UIColorTransition _tickTemplate;

		private readonly List<UIColorTransition> _ticks = new (16);

		private void Awake() {
			_tickTemplate.SetActive(false);
			Populate(4);
		}

		public void Setup(int count) {
			_tickTemplate.SetActive(false);

			Populate(count);

			for (var i = 0; i < _ticks.Count; i++) {
				var tick = _ticks[i];
				tick.SetActive(i < count);
			}
			
			transform.RecursiveUpdateLayout();
		}

		private void Populate(int count) {
			while (_ticks.Count < count) {
				var tick = Instantiate(_tickTemplate, transform, false);
				_ticks.Add(tick);
			}
		}

		public void SetActiveIndex(int activeIndex, bool instant = false) {
			activeIndex = _ticks.ClampIndex(activeIndex);
			for (var i = 0; i < _ticks.Count; i++) {
				var tick = _ticks[i];
				if (tick.isActiveAndEnabled) tick.SetActiveState(i == activeIndex, instant);
			}
		}
		
		

	}

}