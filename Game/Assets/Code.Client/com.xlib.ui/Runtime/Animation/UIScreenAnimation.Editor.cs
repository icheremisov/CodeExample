#if UNITY_EDITOR

using System.Linq;
using UnityEngine;

namespace XLib.UI.Animation {

	public sealed partial class UIScreenAnimation {
		private bool IsOtherGameObject(GameObject go) {
			if (go == null) return true;

			var tr = go.transform;
			
			do {
				if (tr == transform) return false;
				tr = tr.parent;
			} while (tr != null);

			return true;
		}
		
		private void OnRootObjectsChange() {
			_rootObjects = _rootObjects.Where(IsOtherGameObject).ToArray();
		}
	}

}

#endif