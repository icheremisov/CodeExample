namespace XLib.Configs.Contracts {

	public interface ILocIndexedObjectGetter {
		int LocIndexCount { get; }
		object LocIndexGetter(int index);
	}

}