using UnityEngine;
using XLib.UI.Controls.LoopScroll.ItemView;

namespace XLib.UI.Controls.LoopScroll.ItemModel {

	/// <summary>
	///     fill list from array
	/// </summary>
	public class ArrayModelSource<TModel, TView> : LoopScrollModelSource<TModel, TView>
		where TView : ILoopScrollItemView<TModel> {

		private readonly TModel[] _objectsToFill;

		public ArrayModelSource(TModel[] objectsToFill) {
			_objectsToFill = objectsToFill;
		}

		protected override void FillView(TView view, int index) {
			Debug.Assert(_objectsToFill.IsValidIndex(index));

			view.UpdateView(_objectsToFill[index], index);
		}

	}

}