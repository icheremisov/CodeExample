namespace XLib.Configs.Contracts {

	public interface IGameItemComponent
	{
		FileId Id { get; }
		ItemId OwnerId { get; }
	}

}