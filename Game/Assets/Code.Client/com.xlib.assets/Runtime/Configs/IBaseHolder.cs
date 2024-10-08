namespace XLib.Assets.Configs {

	public interface IBaseHolder<out T> {

		T Item { get; }

	}

}