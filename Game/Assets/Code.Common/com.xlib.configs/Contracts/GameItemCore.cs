using System;
using XLib.Core.Utils;

namespace XLib.Configs.Contracts {

	public abstract class GameItemCore : GameItemOrComponent, IGameItem, IOrderBy {
		public abstract ItemId Id { get; }
		public abstract string FileName { get; set; }
		public abstract void Init(IGameDatabase gameDatabase);
		public abstract void Dispose();

		public static implicit operator ItemId(GameItemCore item) => item == null ? ItemId.None : item.Id;
		int IComparable<IGameItem>.CompareTo(IGameItem other) => ((IComparable<ItemId>)this).CompareTo(other?.Id ?? Enums.ToEnum<ItemId, int>(int.MaxValue));
		int IComparable<ItemId>.CompareTo(ItemId other) => 0;
		bool IEquatable<IGameItem>.Equals(IGameItem other) => EnumComparer<ItemId>.Default.Equals(Id, other?.Id ?? Enums.ToEnum<ItemId, int>(int.MaxValue));
		bool IEquatable<ItemId>.Equals(ItemId other) => EnumComparer<ItemId>.Default.Equals(Id, other);
		public override int GetHashCode() => EnumComparer<ItemId>.Default.GetHashCode(Id);

		public override bool Equals(object other) {
			if (other is IGameItem item) return ((IEquatable<IGameItem>)this).Equals(item);
			if (other is ItemId id) return ((IEquatable<ItemId>)this).Equals(id);
			return false;
		}

		public override string ToString() => $"{FileName}#{Id.ToKeyString()}";
		public virtual int OrderByValue => (GetType().GetHashCode() % 1000) * 1000 + GetHashCode() % 1000;
	}

}