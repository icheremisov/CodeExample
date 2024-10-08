using System;

namespace XLib.Configs.Contracts {

	public interface IGameItem : IDisposable,
								 IComparable<IGameItem>,
								 IComparable<ItemId>,
								 IEquatable<IGameItem>,
								 IEquatable<ItemId> {
		ItemId Id { get; }
		string FileName { get; }
		void Init(IGameDatabase gameDatabase);
	}

	public interface IGameItemEditor {
		public void SetId(ItemId itemIdm, string fileName = null);
	}

}