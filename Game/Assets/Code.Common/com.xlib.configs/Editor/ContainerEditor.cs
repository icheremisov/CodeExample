using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Core;
using XLib.Core.Utils;
using XLib.Unity.Editors;
using XLib.Unity.Utils;
using Object = UnityEngine.Object;

namespace XLib.Configs {

	public abstract class ContainerEditor : OdinEditor {
		private static UnityEditor.Editor[] _editors = new UnityEditor.Editor[1];

		protected ContainerConfigAttribute Attributes;
		protected Type ElementType;
		private Type _containerType;
		private readonly List<Type> _allowedChildren = new(8);
		private string[] _allowedChildrenNames;
		private int _selectedChildTypeIdx = 0;
		private string _search = "";
		private string _path;
		private Object _lastAdded;
		private string _copiedData;

		protected List<Object> Elements;
		protected AssetUserData _userData;
		protected bool _userDataChanged = false;

		protected virtual bool DrawElements => Attributes?.DrawElements ?? true;
		protected virtual int MaxElementCount => Attributes?.MaxElementCount ?? 0;

		protected abstract Type GetElementType(Type container);

		protected virtual bool IsAllowedElementType(Type type) => Attributes.IsAllowedElementType.IsNullOrEmpty() || Attributes.IsAllowedElementType.Contains(type);

		protected override void OnEnable() {
			base.OnEnable();
			OnRootEnable();

			if (target == null) return;
			_containerType = target.GetType();
			ElementType = GetElementType(_containerType);
			_allowedChildren.Clear();
			_path = AssetDatabase.GetAssetPath(target);
			_userData = AssetDatabaseUserData.LoadUserData<AssetUserData>(target);

			Elements = new List<Object>(((IGameItemContainerEditor)target).GetElementsInternal() ?? Array.Empty<ScriptableObject>());
			Attributes = _containerType.GetCustomAttribute<ContainerConfigAttribute>() ?? ContainerConfigAttribute.Default;

			if (string.IsNullOrEmpty(_path)) return;

			if (!AssetDatabase.IsMainAsset(target)) {
				AssetDatabase.SetMainObject(target, _path);
				AssetDatabase.ImportAsset(_path);
			}

			var containedElements = AssetDatabase.LoadAllAssetsAtPath(_path);
			var changed = false;
			foreach (var element in containedElements) {
				if (element == target) continue;
				if (Elements.IndexOf(element as ScriptableObject) == -1) {
					Elements.Add(element as ScriptableObject);
					changed = true;
				}
			}

			if (changed) UpdateContainer();

			foreach (var type in TypeUtils.EnumerateAll(type =>
						 (type == ElementType || type.IsSubclassOf(ElementType)) && IsAllowedElementType(type))) {
				_allowedChildren.Add(type);
			}

			_allowedChildrenNames = new string[_allowedChildren.Count];
			for (var i = 0; i < _allowedChildren.Count; i++) _allowedChildrenNames[i] = ObjectNames.NicifyVariableName(_allowedChildren[i].Name);

			Undo.undoRedoPerformed += OnUndo;
		}

		private void OnUndo() {
			Elements = new List<Object>(((IGameItemContainerEditor)target).GetElementsInternal() ?? Array.Empty<ScriptableObject>());
			UpdateContainer();
		}

		protected override void OnDisable() {
			base.OnDisable();

			if (target) target.SetObjectDirty();
			Undo.undoRedoPerformed -= OnUndo;
		}

		public override void OnInspectorGUI() {
			OnRootInspectorGUI();

			if (!DrawElements) return;

			if (serializedObject.isEditingMultipleObjects) {
				EditorGUILayout.HelpBox("Cannot edit components while multi-editing", MessageType.Info);
				return;
			}

			Elements = new List<Object>(((IGameItemContainerEditor)target).GetElementsInternal() ?? Array.Empty<ScriptableObject>());

			var groupComponents = Attributes?.GroupComponents == true;
			var allowedChildrenNames = _allowedChildrenNames ?? Array.Empty<string>();

			if (groupComponents && allowedChildrenNames.Length > 1) {
				_selectedChildTypeIdx = GUILayout.Toolbar(_selectedChildTypeIdx, allowedChildrenNames);
			}

			if (_editors.Length < Elements.Count) Array.Resize(ref _editors, Elements.Count);

			var prev = -1;
			for (var i = 0; i < Elements.Count; i++) {
				var asset = Elements[i];
				if (asset == target || (groupComponents && (asset.GetType() != _allowedChildren[_selectedChildTypeIdx]))) continue;
				DrawObjectHeader(asset, i, prev);
				prev = i;
				CreateCachedEditor(asset, null, ref _editors[i]);
				_editors[i].OnInspectorGUI();
			}

			if (MaxElementCount > 0 && Elements.Count >= MaxElementCount) return;

			EditorGUILayout.Separator();
			EditorGUILayout.BeginHorizontal();
			if (_allowedChildren.Count > 10) _search = EditorGUILayout.TextField(_search);
			var rect = EditorGUILayout.GetControlRect();
			if (groupComponents) {
				if (GUI.Button(rect, "Add " + allowedChildrenNames[_selectedChildTypeIdx])) CreateChild(_allowedChildren[_selectedChildTypeIdx]).Forget();
			}
			else if (GUI.Button(rect, "Add " + ObjectNames.NicifyVariableName(ElementType.Name) + "...")) ObjectTypeMenu(t => CreateChild(t).Forget(), rect);

			EditorGUILayout.EndHorizontal();

			if (_userDataChanged) {
				_userDataChanged = false;
				AssetDatabaseUserData.SaveUserData(_userData, target);
			}
		}

		private void ObjectTypeMenu(Action<Type> callback, Rect? rect = null) {
			Type last = null;
			var menu = new GenericMenu();
			foreach (var child in _allowedChildren) {
				if (child.Name.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0) {
					last = child;
					menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(child.Name)), false, obj => callback(obj as Type), child);
				}
			}

			if (menu.GetItemCount() > 1) {
				if (rect != null)
					menu.DropDown(rect.Value);
				else
					menu.ShowAsContext();
			}
			else if (last != null) callback(last);
		}

		private async UniTask CreateChild(Type type) {
			var assetName = type.Name;
			if (Attributes.WithCustomName) {
				await UniTask.Yield();
				assetName = EditorUtils.ShowInputDialog(type.Name, "Enter name", assetName);
				if (assetName.IsNullOrEmpty() || (Elements?.Any(x => x.name == assetName) ?? false)) return;
			}

			var asset = CreateInstance(type);
			asset.name = assetName;

			if (await AddChild(asset)) {
				Undo.RegisterCreatedObjectUndo(asset, "Create child");
				Undo.RecordObject(target, "Create child");
				_lastAdded = asset;
				Elements.Add(_lastAdded);
				UpdateContainer();
			}
		}

		protected virtual string GetHeaderLabel(Object obj) => obj != null ? (Attributes?.WithCustomName == true ? obj.name : ObjectNames.NicifyVariableName(obj.GetType().Name)) : "<NULL>";

		protected virtual UniTask<bool> AddChild(Object asset) {
			AssetDatabase.AddObjectToAsset(asset, AssetDatabase.GetAssetPath(target));
			AssetDatabase.Refresh();
			return UniTask.FromResult(true);
		}

		private void UpdateContainer() {
			var arr = Array.CreateInstance(ElementType, Elements.Count) as Object[];
			Elements.CopyTo(arr, 0);
			((IGameItemContainerEditor)target).SetElementsInternal(arr);
			AssetDatabase.SaveAssets();
			_userData = AssetDatabaseUserData.LoadUserData<AssetUserData>(target);
		}

		private void DrawObjectHeader(Object obj, int index, int prev) {
			var objColor = Attributes.WithColor ? _userData?.GetColor(obj) : null;

			EditorGuiEx.Separator();
			EditorGUILayout.BeginHorizontal();
			{
				using var _ = objColor != null ? GuiEx.Color(objColor.Value) : GuiEx.UsingCallback.None;
				EditorGUILayout.LabelField($"{GetHeaderLabel(obj)}", EditorStyles.boldLabel);
			}
			GUILayout.FlexibleSpace();

			if (prev >= 0 && Attributes.AllowReorder && GUILayout.Button("UP", EditorStyles.miniButton)) {
				Undo.RecordObject(target, "");
				SwapElements(index, prev);
			}

			if (Attributes.AllowTypeChange && GUILayout.Button("TYPE", "MiniPopup")) ObjectTypeMenu(type => EditorUtils.ChangeAssetType(obj, type));

			if (Attributes.AllowCopyPaste) {
				if (GUILayout.Button("COPY", EditorStyles.miniButton)) {
					_copiedData = CopyObject(obj, EditorJsonUtility.ToJson);
				}

				if (!string.IsNullOrEmpty(_copiedData) && GUILayout.Button("PASTE", EditorStyles.miniButton)) PasteObject(obj, _copiedData, EditorJsonUtility.FromJsonOverwrite);
			}

			if (Attributes.WithColor) {
				var color = objColor ?? Color.HSVToRGB(index * 0.1f, 1f, 1f);
				var ncolor = EditorGUILayout.ColorField(color, GUILayout.Width(50));
				if (ncolor != color) {
					if (_userData == null) _userData = new AssetUserData();
					_userData.SetColor(obj, ncolor);
					_userDataChanged = true;
				}
			}

			if (Attributes.AllowDeletion) {
				using var _ = GuiEx.Color(Color.red);
				if (GUILayout.Button("X", EditorStyles.miniButton)) {
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Confirm delete"), false, () => {
						Undo.RegisterCompleteObjectUndo(target, "");
						Undo.DestroyObjectImmediate(obj);
						Elements.Remove(obj);

						UpdateContainer();
					});
					menu.ShowAsContext();
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		protected virtual string CopyObject(Object obj, Func<object, string> copyMethod) => copyMethod(obj);

		protected virtual void PasteObject(Object obj, string data, Action<string, object> pasteMethod) {
			Undo.RecordObject(obj, "");
			pasteMethod(data, obj);
		}

		private void SwapElements(int a, int b) {
			if (a < 0 || b < 0 || a >= Elements.Count || b >= Elements.Count) return;
			(Elements[a], Elements[b]) = (Elements[b], Elements[a]);
			UpdateContainer();
		}

		protected virtual void OnRootInspectorGUI() => base.OnInspectorGUI();

		protected virtual void OnRootEnable() { }
	}

}