using XLib.Configs.Contracts;

namespace XLib.Configs.Core {

	[ItemCategory("Global")]
	public abstract class GameItemSingleton<T> : GameItemBase, IGameItemSingleton where T : GameItemSingleton<T> {
		
#if UNITY3D		
		public static T Instance => GameData.Once<T>();
#endif
	}

}