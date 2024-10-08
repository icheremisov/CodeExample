using XLib.Configs.Contracts;
using XLib.Configs.Core;

// ReSharper disable once CheckNamespace
public static class ItemIdExtensions {
	public static T As<T>(this ItemId id, bool throwOnNotFound = true) where T : GameItemBase => GameData.Get<T>(id, throwOnNotFound);
	public static T AsInterface<T>(this ItemId id, bool throwOnNotFound = true) where T : class => GameData.Get<T>(id, throwOnNotFound);
}
