using UnityEngine;

namespace XLib.UI.Controls.LoopScroll.ItemView {

	/// <summary>
	///     factory for views
	/// </summary>
	public interface ILoopScrollItemViewFactory {

		public GameObject GetObject();
		public void ReturnObject(Transform go);

	}

}