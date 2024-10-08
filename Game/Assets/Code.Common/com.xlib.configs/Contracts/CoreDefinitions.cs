using XLib.Core.Utils;

namespace XLib.Configs.Contracts {

	public enum PairItemId : long {
		None = 0
	}

	public enum FullItemId : long {
		None = 0
	}

	public enum ItemId {
		None = 0
	}

	public enum FileId {
		None = 0
	}

	public static class PairItemExtension {
		public static ItemId ToItemId(this PairItemId itemId) => ((itemId.AsLong() >> 32) & 0xFFFFFFFF).ToEnum<ItemId>();
		public static ItemId ToTargetId(this PairItemId itemId) => (itemId.AsLong() & 0xFFFFFFFF).ToEnum<ItemId>();
		public static PairItemId ToPairId(this ItemId main, ItemId targetId) => (((long)main.AsInt() << 32) | (uint)targetId.AsInt()).ToEnum<PairItemId>();
	}

	public static class FullItemExtension {
		public static string ToDebugString(this FullItemId itemId) => $"{itemId.ToItemId()}({itemId.ToItemId().ToKeyString()}) / {itemId.ToFileId()}({itemId.ToFileId().ToKeyString()})";
		public static ItemId ToItemId(this FullItemId itemId) => ((itemId.AsLong() >> 32) & 0xFFFFFFFF).ToEnum<ItemId>();
		public static FileId ToFileId(this FullItemId itemId) => (itemId.AsLong() & 0xFFFFFFFF).ToEnum<FileId>();
		public static (ItemId,FileId) ToIds(this FullItemId itemId) => (ToItemId(itemId), ToFileId(itemId));
		public static FullItemId ToFullItemId(this ItemId main, FileId fileId) => (((long)main.AsInt() << 32) | (uint)fileId.AsInt()).ToEnum<FullItemId>();
	}

}