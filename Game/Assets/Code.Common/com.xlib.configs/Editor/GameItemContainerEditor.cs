using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;
using XLib.Unity.Utils;
using Object = UnityEngine.Object;

namespace XLib.Configs {

	[CustomEditor(typeof(GameItemBaseContainer), true), CanEditMultipleObjects]
	public class GameItemContainerEditor : ContainerEditor {
		private int _selectionIndex;
		private int _typeIndex;
		private List<Type> _elementTypes;

		private ReorderableList _list;

		private int _index = -1;
		private float[] _heights;

		protected override void OnRootEnable() {
			base.OnRootEnable();

			var obj = target as GameItemBaseContainer;

			if (!obj) return;

			obj.Init(GameDatabase_Editor.Instance);

			_index = -1;

			var prop = serializedObject.FindProperty("_elements");
			_heights = new float[prop.arraySize];
			_list = new ReorderableList(serializedObject, prop, true, false, false, false) {
				elementHeight = 24,
				drawElementCallback = OnListDrawElement,
				elementHeightCallback = OnListElementHeight,
				onChangedCallback = _ => {
					var elements = obj.RawElements ?? Array.Empty<GameItemComponent>();
					((IGameItemContainerEditor)obj).SetItems(elements);
				},
				onSelectCallback = l => { _index = l.index; }
			};
		}

		private float OnListElementHeight(int index) {
			if (_heights.Length != _list.serializedProperty.arraySize) Array.Resize(ref _heights, _list.serializedProperty.arraySize);
			return Mathf.Max(_heights[index], 24);
		}

		protected override Type GetElementType(Type container) {
			var parent = typeof(GameItemBaseContainer<>);

			var elementType = container;
			while (elementType != null && (!elementType.IsGenericType || elementType.GetGenericTypeDefinition() != parent)) elementType = elementType.BaseType;

			Debug.Assert(elementType != null, $"Unknown element type for container {container.Name} {target}");
			return elementType.GetGenericArguments()[0];
		}

		protected override async UniTask<bool> AddChild(Object asset) {
			var result = await base.AddChild(asset);
			if (!result) return false;

			if (asset is IItemContainerEditor component) component.AddToContainer(target, GetLastFileId());

			return true;
		}

		protected override void PasteObject(Object obj, string data, Action<string, object> pasteMethod) {
			var comp = (GameItemComponent)obj;
			var fileId = comp.Id;
			base.PasteObject(obj, data, pasteMethod);
			comp.Id = fileId;
		}

		private FileId GetLastFileId() {
			var maxFileId = Elements.OfType<GameItemComponent>().MaxByOrDefault(comp => comp.Id)?.Id ?? FileId.None;
			return maxFileId + 1;
		}

		protected override void OnRootInspectorGUI() {
			base.OnRootInspectorGUI();
			if (!Attributes.DrawElementsAsLine) return;

			var obj = target as GameItemBaseContainer;
			if (obj == null) return;

			var elems = (obj.RawElements ?? Array.Empty<GameItemComponent>()).ToList();

			GUILayout.Space(20);

			DrawControlPanel(obj, elems);

			GUILayout.Space(10);

			serializedObject.Update();
			_list.DoLayoutList();
			serializedObject.ApplyModifiedProperties();

			GUILayout.Space(10);

			DrawControlPanel(obj, elems);
		}

		private void DrawControlPanel(GameItemBaseContainer obj, List<GameItemComponent> elems) {
			EditorGUIUtility.labelWidth = 50;

			GUILayout.BeginHorizontal();
			{
				_elementTypes ??= TypeCache.GetTypesDerivedFrom(ElementType).ToList();
				if (_elementTypes.IsNullOrEmpty()) _elementTypes.Add(ElementType);
				_typeIndex = EditorGUILayout.Popup("Type", _typeIndex, _elementTypes.Select(type => type.Name.RemoveAll("Step", "Definition", "Tutorial")).ToArray(),
					GUILayout.MaxWidth(250));
			}

			GUILayout.EndHorizontal();

			var gameItemBaseContEditor = (IGameItemContainerEditor)obj;
			GUILayout.BeginHorizontal();
			{
				var c = GUI.color;
				GUI.enabled = _typeIndex >= 0;
				GUI.color = Color.green;
				if (GUILayout.Button("Add", GUILayout.MaxWidth(60))) {
					gameItemBaseContEditor.AddNew(elems, _elementTypes[_typeIndex], _index);
				}

				GUI.color = c;
				GUI.enabled = true;

				if (GUILayout.Button("Refresh", GUILayout.MaxWidth(60))) gameItemBaseContEditor.UpdateChildrenNames();

				var hasSelection = _index >= 0;

				c = GUI.color;

				GUI.enabled = hasSelection;
				GUI.color = Color.cyan;
				if (GUILayout.Button("Copy", GUILayout.MaxWidth(60))) {
					DoCopy(elems[_index]);
				}

				GUI.color = Color.yellow;
				GUI.enabled = CanPaste;
				if (GUILayout.Button("Paste", GUILayout.MaxWidth(60))) {
					DoPaste(_index);
				}

				GUI.enabled = hasSelection;
				GUI.color = Color.red;
				if (GUILayout.Button("Remove", GUILayout.MaxWidth(60))) {
					DoRemove(_index);
				}

				GUI.enabled = true;
				GUI.color = c;
			}
			GUILayout.EndHorizontal();
		}

		private void OnListDrawElement(Rect rect, int index, bool isActive, bool isFocused) {
			var obj = target as IGameItemContainerEditor;
			var element = _list.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2;

			var child = element.GetValue<GameItemComponent>();

			var pos = rect;

			GUI.Label(pos.SubMax(new Vector2(20, 0)), (index + 1).ToString());
			pos = pos.AddMin(new Vector2(20, 0));

			var skipAble = child as ISkippAbleContainerItem;
			if (skipAble != null) {
				var skip = GUI.Toggle(pos.SetWidth(20), skipAble.Skip, string.Empty);
				if (skip != skipAble.Skip) {
					skipAble.Skip = skip;
					child.SetObjectDirty();
				}
			}

			var label = child != null ? skipAble?.Skip ?? false ? $"SKIP {child.GetInspectorName}" : child.GetInspectorName : "<NULL>";
			if (label.IsNullOrEmpty()) label = "<NULL>";
			var lineCount = label.Count(c => c.Equals('\n')) + 1;
			var newHeight = Mathf.Max(24.0f, lineCount * 20.0f);
			if (Math.Abs(newHeight - _heights[index]) > float.Epsilon) {
				_heights[index] = newHeight;
				_list.elementHeightCallback.Invoke(index);
			}

			pos = pos.AddMin(new Vector2(20, 0));

			if (GUI.Button(pos.SetWidth(36), EditorGUIUtility.IconContent("d_SearchOverlay", "|Go To"))) {
				Selection.activeObject = child;
				return;
			}

			pos = pos.AddMin(new Vector2(40, 0));

			GUI.Label(pos.SubMax(new Vector2(90, 0)), label);

			var c = GUI.color;
			GUI.color = Color.cyan;
			if (GUI.Button(pos.AlignRight(24).SubX(60), EditorGUIUtility.IconContent("d_winbtn_win_restore_h@2x", "|Copy"))) {
				var elems = ((GameItemBaseContainer)obj)?.RawElements ?? Array.Empty<GameItemComponent>();
				DoCopy(elems[index]);
				return;
			}

			GUI.color = Color.yellow;
			GUI.enabled = CanPaste;
			if (GUI.Button(pos.AlignRight(24).SubX(30), EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow", "|Paste"))) {
				DoPaste(index);
				return;
			}

			GUI.enabled = true;

			GUI.color = Color.red;
			if (GUI.Button(pos.AlignRight(24), EditorGUIUtility.IconContent("d_CacheServerDisconnected", "|Remove"))) {
				DoRemove(index);
				return;
			}

			GUI.color = c;
		}

		private void DoRemove(int index) {
			var obj = (GameItemBaseContainer)target;
			var elems = obj.RawElements ?? Array.Empty<GameItemComponent>();
			if (!elems.IsValidIndex(index)) return;

			if (EditorUtility.DisplayDialog("Remove", $"Remove item '{elems[index]}'? It can't be undone!", "Yes", "Cancel")) ((IGameItemContainerEditor)obj).RemoveAt(index);
		}

		private bool CanPaste => Clipboard.CanPaste<GameItemComponent>();

		private static void DoCopy(GameItemComponent elem) {
			Clipboard.Copy(elem, CopyModes.DeepCopy);
		}

		private void DoPaste(int index) {
			var obj = (GameItemBaseContainer)target;

			var elem = Instantiate(Clipboard.Paste<GameItemComponent>());
			var elems = (obj.RawElements ?? Array.Empty<GameItemComponent>()).ToList();
			((IGameItemContainerEditor)obj).Insert(elems, elem, index);
			_list.Select(index);
		}

		protected override string GetHeaderLabel(Object obj) => $"{base.GetHeaderLabel(obj)} [{((GameItemComponent)obj)?.Id}]";
	}

}