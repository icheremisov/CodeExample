using UnityEngine;

namespace XLib.UI.Controls.LoopScroll.ItemModel {

	/// <summary>
	///     provide models data for loop scroll
	/// </summary>
	public interface ILoopScrollModelSource {

		void FillView(Transform viewTransform, int index);

	}

}