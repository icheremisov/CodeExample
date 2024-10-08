namespace XLib.UI.Controls.LoopScroll.ItemView {

	/// <summary>
	///     update view with model
	/// </summary>
	public interface ILoopScrollItemView<in TModel> {

		void UpdateView(TModel model, int idx);

	}

}