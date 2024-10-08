#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;
using XLib.Configs.Sheets.Contracts;
using XLib.Configs.Sheets.Core;
using XLib.Core.CommonTypes;
using XLib.Core.Utils;

namespace XLib.Configs.Sheets.Converters {

	public abstract class SheetsConverter<TObj, TTo> : ISheetsConverter {
		public Type ToType => typeof(TTo);

		public abstract TTo To(TObj obj, Type type);
		public abstract TObj From(string value, Type type);
		public virtual IEnumerable<object> GetValues(SheetRowProperty property) => null;

		object ISheetsConverter.To(object obj, Type type) => To((TObj)obj, type);
		object ISheetsConverter.From(string value, Type type) => From(value, type);
	}

	[UsedImplicitly]
	[SheetsConverter(typeof(ItemId), false)]
	public class ItemIdSheetsConverter : SheetsConverter<ItemId, string> {
		public override string To(ItemId obj, Type type) => $"'{obj.ToKeyString()}";
		public override ItemId From(string value, Type type) => value.ToItemId();
	}

	[UsedImplicitly]
	[SheetsConverter(typeof((ItemId, FileId)), false)]
	public class ItemFileIdSheetsConverter : SheetsConverter<(ItemId, FileId), string> {
		public override string To((ItemId, FileId) obj, Type type) => $"'{obj.Item1.ToKeyString()}:{obj.Item2.ToKeyString()}";

		public override (ItemId, FileId) From(string value, Type type) {
			if (string.IsNullOrEmpty(value)) return (ItemId.None, FileId.None);
			var parts = value.Split(":");
			return (parts[0].ToItemId(), parts[1].ToFileId());
		}
	}

	[UsedImplicitly]
	[SheetsConverter(typeof(FileId))]
	public class FileIdSheetsConverter : SheetsConverter<FileId, int> {
		public override int To(FileId obj, Type type) => (int)obj;
		public override FileId From(string value, Type type) => (FileId)int.Parse(value);
	}

	[UsedImplicitly]
	[SheetsConverter(typeof(FullItemId))]
	public class FullItemIdSheetsConverter : SheetsConverter<FullItemId, string> {
		public override string To(FullItemId obj, Type type) => obj.ToString("X");
		public override FullItemId From(string value, Type type) => (FullItemId)long.Parse(value, System.Globalization.NumberStyles.HexNumber);
	}

	[UsedImplicitly]
	[SheetsConverter(typeof(Duration))]
	public class DurationSheetsConverter : SheetsConverter<Duration, int> {
		public override int To(Duration obj, Type type) => (int)obj;
		public override Duration From(string value, Type type) => new Duration(int.Parse(value));
	}

	[SheetsConverter(typeof(GameItemBase), false)]
	public class GameItemBaseSheetsConverter : SheetsConverter<GameItemBase, string> {
		public override IEnumerable<object> GetValues(SheetRowProperty property) {
			var type = property.ElementType ?? property.Type;
			return GameData.All<GameItemBase>()
				.Where(v => v.GetType() == type || v.GetType().IsSubclassOf(type))
				.Select(obj => To(obj, type));
		}

		public override string To(GameItemBase obj, Type type) => obj?.FileName ?? string.Empty;

		public override GameItemBase From(string value, Type type) {
			if (value.IsNullOrEmpty()) return null;
			return GameData.All<GameItemBase>()
				.FirstOrDefault(v =>
					v.FileName == value && IsValidType(v, type));
		}

		private bool IsValidType(GameItemBase item, Type type) {
			var itemType = item.GetType();
			while (itemType != null) {
				if (itemType == type) return true;
				itemType = itemType.BaseType;
				if (itemType == TypeOf<object>.Raw) return false;
			}

			return false;
		}
	}

	[SheetsConverter(typeof(GameItemComponent), false)]
	public class GameItemComponentConverter : SheetsConverter<GameItemComponent, string> {
		public override string To(GameItemComponent obj, Type type) => $"{obj.GetOwner<GameItemBaseContainer>().FileName}:{obj.Id.AsInt()}";

		public override GameItemComponent From(string value, Type type) {
			var split = value.Split(":");
			var container = GameData.All<GameItemBaseContainer>()
				.FirstOrDefault(v =>
					v.FileName == split[0]);
			var fileId = int.Parse(split[1]).ToEnum<FileId>();
			return container.RawElements.FirstOrDefault(component => component.Id == fileId);
		}
	}

	[UsedImplicitly]
	[SheetsConverter(typeof(GameItemOrComponent), false)]
	public class GameItemOrComponentConverter : SheetsConverter<GameItemOrComponent, string> {
		private static readonly GameItemComponentConverter GameItemComponentConverter = new();
		private static readonly GameItemBaseSheetsConverter GameItemBaseConverter = new();

		public override string To(GameItemOrComponent obj, Type type) {
			return obj switch {
				GameItemBase gameItemBase           => GameItemBaseConverter.To(gameItemBase, type),
				GameItemComponent gameItemComponent => GameItemComponentConverter.To(gameItemComponent, type),
				_                                   => $"{obj.name}"
			};
		}

		public override GameItemOrComponent From(string value, Type type) {
			var result = GameItemBaseConverter.From(value, type);
			if (result != null) return result;

			return GameItemComponentConverter.From(value, type);
		}
	}

	[UsedImplicitly]
	[SheetsConverter(typeof(LabelItem), false)]
	public class TagItemComponentConverter : SheetsConverter<LabelItem, string> {
		public override string To(LabelItem obj, Type type) => obj.name;

		public override LabelItem From(string value, Type type) {
			return GameData.All<LabelContainer>()
				.SelectMany(container => container.RawElements.OfType<LabelItem>())
				.FirstOrDefault(item => item.name == value);
		}

		public override IEnumerable<object> GetValues(SheetRowProperty property) {
			var type = property.ElementType ?? property.Type;
			return GameData.All<LabelContainer>().SelectMany(container => container.RawElements.OfType<LabelItem>()).Select(item => To(item, type));
		}
	}

	[UsedImplicitly]
	[SheetsConverter(typeof(Color), false)]
	public class ColorConverter : SheetsConverter<Color, string> {
		public override string To(Color obj, Type type) => ColorUtility.ToHtmlStringRGBA(obj);
		public override Color From(string value, Type type) => ColorUtility.TryParseHtmlString(value, out var color) ? color : default;
	}

}

#endif