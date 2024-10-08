using System;
using System.Collections.Generic;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;
using Object = UnityEngine.Object;

namespace XLib.Configs.Utils {

	public abstract class ScriptableObjectContainer : ScriptableObject {
		public abstract Type ItemType { get; }
		public GameItemComponent RawItem(FileId index) => throw new NotImplementedException();
	}

	public abstract partial class ScriptableObjectContainer<T> : ScriptableObjectContainer, IGameItemContainer where T : Object {
		[SerializeField, HideInInspector]
		protected T[] _elements;
		public Object RawItem(int index) => _elements[index];
		public Object[] RawElements => _elements;
		IEnumerable<Object> IGameItemContainer.RawElements => _elements;
		public override Type ItemType => typeof(T);
	}

}