using UnityEngine;
using XLib.Core.Utils;
using XLib.UI.Controls.LoopScroll.ItemView;

namespace XLib.UI.Controls.LoopScroll.ItemModel {

	public abstract class LoopScrollModelSource<TModel, TView> : ILoopScrollModelSource
		where TView : ILoopScrollItemView<TModel> {

		public void FillView(Transform viewTransform, int index) {
			var view = viewTransform.GetComponent<TView>();
			if (view == null) {
				UILogger.LogError($"Cannot find component {TypeOf<TView>.Name} in {viewTransform.GetFullPath()}");
				return;
			}

			FillView(view, index);
		}

		protected abstract void FillView(TView view, int index);

	}

}