namespace XLib.Configs.Contracts {

	public interface IConfigRemapper {
		public int MigrationVersion { get; }
		bool TryGetReMappedId(int version, ref ItemId id);
		bool TryGetReMappedId(int version, ref ItemId id, ref FileId fileId);
	}

}