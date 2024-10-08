// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using JetBrains.Annotations;
// using UnityEditor;
// using UnityEditor.IMGUI.Controls;
// using UnityEngine;
// using XLib.Unity.Utils;
//
// namespace ThirdParty.Editor.CleanEmptyDir.Editor {
//
// 	public class MainWindow : EditorWindow {
// 		private class DirTreeView : TreeView {
// 			[Serializable]
// 			internal class MyTreeElement : TreeViewItem {
// 				public bool Enabled = true;
// 				[CanBeNull] public DirectoryInfo DirInfo;
//
// 				public MyTreeElement(string displayName, DirectoryInfo dirInfo = null) {
// 					DirInfo = dirInfo;
// 					base.displayName = displayName;
// 				}
// 			}
//
// 			private readonly IReadOnlyList<DirectoryInfo> _dirs;
// 			public readonly IReadOnlyList<DirectoryInfo> Dirs;
//
// 			public DirTreeView(TreeViewState treeViewState, IReadOnlyList<DirectoryInfo> dirs) : base(treeViewState) {
// 				_dirs = dirs;
// 				rowHeight = 20;
// 				Reload();
// 				ExpandAll();
// 			}
//
// 			protected override TreeViewItem BuildRoot() {
// 				var root = new TreeViewItem { id = 0, depth = -1, displayName = "Assets" };
//
// 				CreateTreeElement(_dirs, root, "");
//
// 				SetupDepthsFromParentsAndChildren(root);
//
// 				return root;
// 			}
//
// 			public void SelectAll() {
// 				foreach (var treeViewItem in GetRows()) {
// 					((MyTreeElement)treeViewItem).Enabled = true;
// 				}
// 			}
//
// 			public void ClearAll() {
// 				foreach (var treeViewItem in GetRows()) {
// 					((MyTreeElement)treeViewItem).Enabled = false;
// 				}
// 			}
//
// 			public IEnumerable<(DirectoryInfo DirInfo, string DisplayName, int Depth)> GetSelectedDirInfos =>
// 				GetRows().Cast<MyTreeElement>().Where(x => x.Enabled).Select(y => (y.DirInfo, y.displayName, y.depth)).ToList();
//
// 			private static void CreateTreeElement(IEnumerable<DirectoryInfo> dirs, TreeViewItem rootElement, string relPath) {
// 				var processedDirs = dirs.Select(x => {
// 						var curRelPath = Core.GetRelativePath(x.FullName, $"{Application.dataPath}{relPath}");
// 						var idx = curRelPath.IndexOf("\\", StringComparison.Ordinal);
// 						var firstFolderInCurRelPath = idx < 0 ? curRelPath : curRelPath[..idx];
// 						return (dirInfo: x, newRelPath: idx < 0 ? relPath : $"{relPath}/{firstFolderInCurRelPath}", folder: firstFolderInCurRelPath);
// 					})
// 					.GroupBy(x => x.newRelPath)
// 					.ToArray();
//
// 				var hasLeaves = processedDirs.Length > 2;
//
// 				if (!hasLeaves) {
// 					var hasL = processedDirs.FirstOrDefault(x => x.Key == relPath)
// 						?.Any(info => processedDirs.Where(z => z.Key != relPath).All(w => !w.Key.EndsWith(info.folder)));
// 					hasLeaves = hasL.HasValue && hasL.Value;
// 				}
//
// 				TreeViewItem newParent;
// 				if (!relPath.IsNullOrEmpty() && hasLeaves) {
// 					var displayName = rootElement.displayName == "Assets"
// 						? relPath[1..]
// 						: relPath[(relPath.LastIndexOf(rootElement.displayName, StringComparison.Ordinal) + rootElement.displayName.Length + 1)..];
// 					newParent = new MyTreeElement(displayName, new DirectoryInfo($"{Application.dataPath}/{relPath}"));
// 					rootElement.AddChild(newParent);
// 				}
// 				else
// 					newParent = rootElement;
//
// 				foreach (var leavesDir in processedDirs.Where(x => x.Key == relPath)) {
// 					foreach (var dirInfo in leavesDir) {
// 						if (processedDirs.Any(x => x != leavesDir && x.Key.EndsWith(dirInfo.folder))) continue;
// 						newParent.AddChild(new MyTreeElement(dirInfo.folder, dirInfo.dirInfo));
// 					}
// 				}
//
// 				foreach (var directoryInfo in processedDirs.Where(x => x.Key != relPath)) {
// 					CreateTreeElement(directoryInfo.Select(x => x.dirInfo).ToList(), newParent, directoryInfo.Key);
// 				}
// 			}
//
// 			protected override void RowGUI(RowGUIArgs args) {
// 				var item = (MyTreeElement)args.item;
//
// 				var rect = args.rowRect;
// 				rect.xMin += (item.depth + 1) * 20;
// 				var toggleRect = rect;
// 				toggleRect.width = 20;
// 				toggleRect.height = 20;
// 				EditorGUI.BeginChangeCheck();
// 				item.Enabled = EditorGUI.Toggle(toggleRect, item.Enabled);
//
// 				if (EditorGUI.EndChangeCheck() && item.hasChildren) SetChildrenEnabled(item.Enabled, item.children);
//
// 				rect.xMin += 30;
// 				GUI.Label(rect, item.displayName);
//
// 				void SetChildrenEnabled(bool enabled, List<TreeViewItem> children) {
// 					foreach (var child in children.Cast<MyTreeElement>()) {
// 						child.Enabled = enabled;
// 						if (child.hasChildren) SetChildrenEnabled(enabled, child.children);
// 					}
// 				}
// 			}
// 		}
//
// 		[SerializeField] private TreeViewState _treeViewState;
// 		private List<DirectoryInfo> emptyDirs;
// 		private Vector2 scrollPosition;
// 		private bool lastCleanOnSave;
// 		private string delayedNotiMsg;
// 		private GUIStyle updateMsgStyle;
// 		private DirTreeView _dirTree;
// 		private SerializedObject _so;
//
// 		private bool hasNoEmptyDir { get { return emptyDirs == null || emptyDirs.Count == 0; } }
//
// 		private const float DIR_LABEL_HEIGHT = 21;
//
// 		[MenuItem("Window/Clean Empty Dir")]
// 		public static void ShowWindow() {
// 			var w = GetWindow<MainWindow>();
// 			w.titleContent = new GUIContent("Clean");
// 		}
//
// 		private void OnEnable() {
// 			lastCleanOnSave = Core.CleanOnSave;
// 			Core.OnAutoClean += Core_OnAutoClean;
// 			var settings = EditorUtils.LoadFirstAsset<CleanEmptyDirSettings>();
// 			if (settings == null) {
// 				settings = CreateInstance<CleanEmptyDirSettings>();
// 				AssetDatabase.CreateAsset(settings, $"Assets/Settings/CleanEmptyDirSettings.asset");
// 				AssetDatabase.SaveAssets();
// 				AssetDatabase.Refresh();
// 			}
//
// 			_so = new SerializedObject(settings);
// 			_so.Update();
// 			delayedNotiMsg = "Click 'Find Empty Dirs' Button.";
// 		}
//
// 		private void OnDisable() {
// 			Core.CleanOnSave = lastCleanOnSave;
// 			Core.OnAutoClean -= Core_OnAutoClean;
// 		}
//
// 		private void Core_OnAutoClean() {
// 			delayedNotiMsg = "Cleaned on Save";
// 		}
//
// 		private void OnGUI() {
// 			if (delayedNotiMsg != null) {
// 				ShowNotification(new GUIContent(delayedNotiMsg));
// 				delayedNotiMsg = null;
// 			}
//
// 			EditorGUILayout.BeginVertical();
// 			{
// 				var filter = _so.FindProperty("_ignoreWildcards");
// 				EditorGUILayout.PropertyField(filter, true); 
// 				_so.ApplyModifiedProperties();
// 				EditorGUILayout.BeginHorizontal();
// 				{
// 					if (GUILayout.Button("Find Empty Dirs")) {
// 						Core.FillEmptyDirList(out emptyDirs);
//
// 						if (hasNoEmptyDir) {
// 							ShowNotification(new GUIContent("No Empty Directory"));
// 						}
// 						else {
// 							RemoveNotification();
// 							_treeViewState = new TreeViewState();
// 							_dirTree = new DirTreeView(_treeViewState, emptyDirs);
// 						}
// 					}
//
// 					if (ColorButton("Delete All", !hasNoEmptyDir, Color.red)) {
// 						Core.DeleteAllEmptyDirAndMeta(ref emptyDirs);
// 						ShowNotification(new GUIContent("Deleted All"));
// 					}
//
// 					if (ColorButton("Delete Selected", !hasNoEmptyDir, Color.red)) {
// 						Core.DeleteSelectedEmptyDirAndMeta(_dirTree?.GetSelectedDirInfos);
// 						Core.FillEmptyDirList(out emptyDirs);
// 						if (emptyDirs.IsNullOrEmpty()) {
// 							_dirTree?.ClearAll();
// 						}
// 						else {
// 							_dirTree = new DirTreeView(_treeViewState, emptyDirs);
// 							ShowNotification(new GUIContent("Deleted All Selected"));
// 						}
// 					}
// 				}
// 				EditorGUILayout.EndHorizontal();
//
// 				var cleanOnSave = GUILayout.Toggle(lastCleanOnSave, " Clean Empty Dirs Automatically On Save");
// 				if (cleanOnSave != lastCleanOnSave) {
// 					lastCleanOnSave = cleanOnSave;
// 					Core.CleanOnSave = cleanOnSave;
// 				}
//
// 				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
//
// 				if (!hasNoEmptyDir) {
// 					scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));
// 					{
// 						EditorGUILayout.BeginVertical();
// 						{
// #if UNITY_4_6 // and higher
// 							GUIContent folderContent = EditorGUIUtility.IconContent("Folder Icon");
// #else
// 							GUIContent folderContent = new GUIContent();
// #endif
//
// 							_dirTree?.OnGUI(new Rect(0, 0, position.width, position.height));
// 						}
//
// 						EditorGUILayout.EndVertical();
// 					}
// 					EditorGUILayout.EndScrollView();
// 				}
// 			}
// 			GUILayout.BeginHorizontal();
// 			if (GUILayout.Button(new GUIContent("Select All"), GUILayout.ExpandWidth(true))) {
// 				_dirTree?.SelectAll();
// 			}
//
// 			if (GUILayout.Button(new GUIContent("Deselect All"), GUILayout.ExpandWidth(true))) {
// 				_dirTree?.ClearAll();
// 			}
//
// 			GUILayout.EndHorizontal();
// 			EditorGUILayout.EndVertical();
// 		}
//
// 		private void ColorLabel(string title, Color color) {
// 			Color oldColor = GUI.color;
// 			//GUI.color = color;
// 			GUI.enabled = false;
// 			GUILayout.Label(title);
// 			GUI.enabled = true;
// 			;
// 			GUI.color = oldColor;
// 		}
//
// 		private bool ColorButton(string title, bool enabled, Color color) {
// 			bool oldEnabled = GUI.enabled;
// 			Color oldColor = GUI.color;
//
// 			GUI.enabled = enabled;
// 			GUI.color = color;
//
// 			bool ret = GUILayout.Button(title);
//
// 			GUI.enabled = oldEnabled;
// 			GUI.color = oldColor;
//
// 			return ret;
// 		}
// 	}
//
// }