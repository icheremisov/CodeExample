using XLib.Configs.Contracts;
using XLib.Configs.Core;

namespace XLib.Configs.Utils {

	public interface IItemController { }

	public interface IItemController<TItem> : IItemController where TItem : GameItemBase { }

}

public static class ItemIdExtensions {

	public static T As<T>(this ItemId id, IGameDatabase gameDatabase, bool throwOnNotFound = true) where T : GameItemBase => gameDatabase.Get<T>(id, throwOnNotFound);
	public static T AsInterface<T>(this ItemId id, IGameDatabase gameDatabase, bool throwOnNotFound = true) where T : class => gameDatabase.Get<T>(id, throwOnNotFound);

}