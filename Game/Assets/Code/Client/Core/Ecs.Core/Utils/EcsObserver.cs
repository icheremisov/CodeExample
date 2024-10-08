using Entitas;
using Entitas.VisualDebugging.Unity;
using UnityEngine;

namespace Client.Ecs.Core.Utils {

	public class EcsObserver {

		private ContextObserver _observer;

		public EcsObserver(IContext context) {
			
			_observer = new ContextObserver(context);

			Object.DontDestroyOnLoad(_observer.gameObject);
		}

		public void Shutdown() {
			if (_observer != null) {
				_observer.Deactivate();
				Object.Destroy(_observer.gameObject);
				_observer = null;
			}
		}

	}

}