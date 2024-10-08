using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XLib.Configs.Contracts;
using Object = UnityEngine.Object;

namespace XLib.Configs.Core {

	public interface IGameItemContainer {
		Type ItemType { get; }
		public GameItemComponent RawItem(FileId index);
		public IEnumerable<Object> RawElements { get; }
	}

	public interface ISkippAbleContainerItem {
		bool Skip { get; set; }
	}

	public abstract class GameItemBaseContainer : GameItemBase, IGameItemContainer {
		public abstract Type ItemType { get; }
		public abstract GameItemComponent RawItem(FileId index);
		public abstract GameItemComponent[] RawElements { get; }
		IEnumerable<Object> IGameItemContainer.RawElements => RawElements;
	}

	[Serializable]
	[WithTabName("Container")]
	public abstract partial class GameItemBaseContainer<T> : GameItemBaseContainer where T : GameItemComponent {
		[SerializeField, HideInInspector, ShtIgnore]
		protected T[] _elements;

		public T this[FileId fileId] { get { return _elements.FirstOrDefault(x => x.Id == fileId); } }
		public override GameItemComponent RawItem(FileId fileId) => this[fileId];
		public override Type ItemType => typeof(T);

		// ReSharper disable once CoVariantArrayConversion
		public override GameItemComponent[] RawElements => _elements ??= Array.Empty<T>();

		public IEnumerable<T> AsEnumerable() => RawElements.Cast<T>();
		public IEnumerable<TChild> AsEnumerable<TChild>() => RawElements?.Cast<TChild>() ?? Enumerable.Empty<TChild>();

		public override void Init(IGameDatabase gameDatabase) {
			base.Init(gameDatabase);
			_elements ??= Array.Empty<T>();
			foreach (var el in AsEnumerable()) el?.Init(gameDatabase);
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ContainerConfigAttribute : Attribute {
		public static readonly ContainerConfigAttribute Default = new();
		public Type[] IsAllowedElementType = Array.Empty<Type>();
		public int MaxElementCount = 0;
		public bool AllowDeletion = true;
		public bool AllowReorder = true;
		public bool AllowTypeChange = false;
		public bool AllowCopyPaste = true;
		public bool GroupComponents = false;
		public bool WithCustomName = false;
		public bool WithColor = false;
		public bool DrawElements = true;
		public bool DrawElementsAsLine = false;
	}

}