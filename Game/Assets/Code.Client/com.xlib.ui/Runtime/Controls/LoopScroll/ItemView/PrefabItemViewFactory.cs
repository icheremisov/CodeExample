using UnityEngine;
using XLib.Core.Utils;
using XLib.Unity.Utils;

namespace XLib.UI.Controls.LoopScroll.ItemView {

	/// <summary>
	///     default factory for views
	/// </summary>
	public class PrefabItemViewFactory : MonoBehaviour, ILoopScrollItemViewFactory {

		[SerializeField] private GameObject _viewPrefab;

		private GameObject _poolRoot;

		private void Awake() {
			_poolRoot = new GameObject($"{name}_poolRoot", TypeOf<RectTransform>.Raw);
			_poolRoot.transform.SetParent(transform, false);
		}

		public GameObject GetObject() => _viewPrefab.Spawn().gameObject;

		public void ReturnObject(Transform go) {
			go.SetParent(_poolRoot.transform, false);
			go.Recycle();
		}

	}

}