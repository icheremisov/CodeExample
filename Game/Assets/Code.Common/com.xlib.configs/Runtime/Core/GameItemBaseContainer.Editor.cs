#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Core.Utils;
using XLib.Unity.Utils;
using Object = UnityEngine.Object;

namespace XLib.Configs.Core {

	public interface IGameItemContainerEditor {
		Object[] GetElementsInternal();
		void SetElementsInternal(Object[] elements);
		void UpdateChildrenNames();
		void SyncRawElements(GameItemComponent[] elements);

		void AddNew(List<GameItemComponent> steps, Type type, int index = -1);
		void Insert(List<GameItemComponent> steps, GameItemComponent asset, int index = -1);
		void RemoveAt(int index);
		void SetItems(IEnumerable<GameItemComponent> steps);
	}

	public interface IItemContainerEditor {
		void AddToContainer(Object target, FileId fileId);
	}

	public partial class GameItemBaseContainer<T> : IGameItemContainerEditor {
		// ReSharper disable once CoVariantArrayConversion
		Object[] IGameItemContainerEditor.GetElementsInternal() => _elements;

		void IGameItemContainerEditor.SetElementsInternal(Object[] elements) {
			if (_elements != null) {
				var changed = false;
				foreach (var element in _elements) {
					if (!elements.Contains(element)) {
						changed = true;
						DestroyImmediate(element, true);
					}

					if (changed) this.SetObjectDirty();
				}
			}

			_elements = (T[])elements;

			UpdateChildrenNamesInternal();

			if (!EditorUtility.IsPersistent(this)) AssetDatabase.SaveAssets();
		}

		protected override void OnValidate() {
			base.OnValidate();
			if (!UnityAppConstants.isPlaying && UnityAppConstants.isEditor) UpdateChildrenNamesInternal();
		}

		private bool UpdateChildrenNamesInternal() {
			var changed = false;
			if (_elements.IsNullOrEmpty()) return false;
			for (var index = 0; index < _elements.Length; index++) {
				var element = _elements[index];
				if (element) changed |= element.EditorUpdateName(index + 1);
			}

			if (changed) this.SetObjectDirty();

			return changed;
		}

		public void UpdateChildrenNames() {
			if (!UpdateChildrenNamesInternal()) return;
			if (EditorApplication.isUpdating) return;
			AssetDatabase.SaveAssetIfDirty(this);
			AssetDatabase.Refresh();
		}

		public void SyncRawElements(GameItemComponent[] elements) {
			var containedElements = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this)).Where(x => x != this);
			foreach (var containedElement in containedElements) DestroyImmediate(containedElement, true);

			var newElements = new T[elements.Length];
			for (var i = 0; i < elements.Length; i++) {
				var copy = CreateInstance(elements[i].GetType());
				EditorUtility.CopySerializedManagedFieldsOnly(elements[i], copy);
				AssetDatabase.AddObjectToAsset(copy, AssetDatabase.GetAssetPath(this));
				(copy as IItemContainerEditor)?.AddToContainer(this, elements[i].Id);
				(copy as GameItemComponent)?.Rename(elements[i].AssetName);
				newElements[i] = copy as T;
				AssetDatabase.Refresh();
			}

			_elements = newElements;
			Init(GameDatabase);
			UpdateChildrenNames();
			this.SetObjectDirty();
		}

		public void AddNew(List<GameItemComponent> steps, Type type, int index = -1) {
			var asset = (T)CreateInstance(type);
			Insert(steps, asset, index);
		}

		public void Insert(List<GameItemComponent> steps, GameItemComponent asset, int index = -1) {
			index = index < 0 ? steps.Count : Mathf.Min(index + 1, steps.Count);
			asset.name = asset.EditorGenerateName(index);

			var lastComp = steps.Where(comp => comp != null).MaxByOrDefault(comp => comp.Id);
			var newId = lastComp != null ? lastComp.Id : FileId.None;
			newId++;

			asset.AddToContainer(this, newId);

			AssetDatabase.AddObjectToAsset(asset, AssetDatabase.GetAssetPath(this));
			Undo.RegisterCreatedObjectUndo(asset, "Create child");
			Undo.RecordObject(this, "Create child");

			steps.Insert(index, asset);
			SetItems(steps);

			if (asset.name.IsNullOrEmpty()) {
				RequestName(asset).Forget();
			}
			else {
				UpdateChildrenNames();
			}
			
		}

		private async UniTaskVoid RequestName(GameItemComponent asset) {
			await UniTask.Yield();
			var assetName = asset.name;
			assetName = EditorUtils.ShowInputDialog(asset.GetType().Name, "Enter name", assetName);
			if (assetName.IsNullOrEmpty() || (RawElements?.Any(x => x.name == assetName) ?? false)) return;
			asset.name = assetName;
			UpdateChildrenNames();
		}

		public void RemoveAt(int index) {
			var steps = (RawElements ?? Array.Empty<GameItemComponent>()).ToList();
			if (!steps.IsValidIndex(index)) return;

			steps.RemoveAt(index);
			SetItems(steps);
		}

		public void SetItems(IEnumerable<GameItemComponent> steps) { // ReSharper disable once CoVariantArrayConversion
			((IGameItemContainerEditor)this).SetElementsInternal(steps.Cast<T>().ToArray());

			EditorApplication.delayCall += RefreshThis;
			return;

			void RefreshThis() {
				EditorApplication.delayCall -= RefreshThis;
				AssetDatabase.SaveAssetIfDirty(this);
				AssetDatabase.Refresh();
			}
		}
	}

}
#endif