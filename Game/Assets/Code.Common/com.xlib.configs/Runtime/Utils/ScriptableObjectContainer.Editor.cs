#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using XLib.Configs.Core;
using Object = UnityEngine.Object;

namespace XLib.Configs.Utils {

	public partial class ScriptableObjectContainer<T> : IGameItemContainerEditor {
		Object[] IGameItemContainerEditor.GetElementsInternal() => _elements;

		void IGameItemContainerEditor.SetElementsInternal(Object[] elements) {
			if (_elements != null) {
				foreach (var element in _elements) {
					if (!_elements.Contains(element)) DestroyImmediate(element, true);
				}
			}

			_elements = (T[])elements;

			if (!EditorUtility.IsPersistent(this)) AssetDatabase.SaveAssets();
		}

		public void UpdateChildrenNames() { }

		public void SyncRawElements(GameItemComponent[] elements) { }
		public void AddNew(List<GameItemComponent> steps, Type type, int index = -1) { }
		public void Insert(List<GameItemComponent> steps, GameItemComponent asset, int index = -1) { }
		public void RemoveAt(int index) { }
		public void SetItems(IEnumerable<GameItemComponent> steps) { }
	}

}
#endif