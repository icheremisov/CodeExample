using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLib.Core.Utils;
using XLib.Core.Utils.Attributes;
using XLib.Unity.Extensions;
using XLib.Unity.Utils;
using Object = UnityEngine.Object;

namespace Client.Odin {

	public class AssetReferenceEditor : LokiEditor<AssetReference> {
		protected class AssetSelectorBase : SimpleSelectorBase<AddressableAssetEntry> {
			private readonly Type _assetType;
			private readonly IReadOnlyCollection<string> _labels;
			private readonly GUIStyle _helper;

			public AssetSelectorBase(Type assetType, IReadOnlyCollection<string> labels) {
				_assetType = assetType;
				_labels = labels;
				_helper = new GUIStyle(GUI.skin.label) {
					alignment = TextAnchor.MiddleRight, fontStyle = FontStyle.Italic, normal = new GUIStyleState() { textColor = Color.gray }
				};

				EnableSingleClickToSelectInternal();
			}

			private void EnableSingleClickToSelectInternal() => SelectionTree.EnumerateTree(x =>
			{
				x.OnDrawItem -= EnableSingleClickToSelectInternal;
				x.OnDrawItem += EnableSingleClickToSelectInternal;
			});

			private void EnableSingleClickToSelectInternal(OdinMenuItem obj)
			{
				var type = Event.current.type;
				if (type == EventType.Layout || !obj.Rect.Contains(Event.current.mousePosition)) return;
				GUIHelper.RequestRepaint();
				if (type != EventType.MouseUp || obj.ChildMenuItems.Count != 0) return;
				obj.Select();
				obj.MenuTree.Selection.ConfirmSelection();
				Event.current.Use();
			}


			protected override void BuildSelectionTree(OdinMenuTree tree) {
				var allEntries = new List<AddressableAssetEntry>();
				AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(allEntries, false, group => true);
				var filtered = _assetType != null
					? allEntries
						.Where(entry => entry != null && entry.guid.IsNotNullOrEmpty() && (entry.MainAssetType == _assetType || entry.MainAssetType.IsSubclassOf(_assetType)))
					: allEntries.Where(entry => entry != null && entry.guid.IsNotNullOrEmpty());

				if (_labels.Count > 0) filtered = filtered.Where(entry => entry.labels.Overlaps(_labels));

				tree.Add("<None>", null);
				foreach (var item in tree.AddRange(filtered, entry => entry.ToString(), entry => AssetDatabase.GetCachedIcon(entry.AssetPath) as Texture2D)) {
					item.OnDrawItem += OnDrawItem;
				}

				var withSearch = tree.MenuItems.Count > 10;
				tree.Config.DrawSearchToolbar = withSearch;
				tree.Config.AutoFocusSearchBar = withSearch;
			}

			private void OnDrawItem(OdinMenuItem item) {
				if (item.Value is not AddressableAssetEntry entry) return;
				var group = entry.parentGroup.Name;
				var text = new GUIContent(group.Length > 20 ? $"{group[..20]}..." : group);
				var rc = item.LabelRect;
				GUI.Label(new Rect(rc.xMax - rc.width/2-20, rc.y, rc.width/2, rc.height), text, _helper);
			}

			protected override void DrawSelectionTree() {
				if (_labels.Count > 0) {
					GUILayout.Label($"Labels: {_labels.JoinToString(", ")}");
				}
				base.DrawSelectionTree();
			}

			public void SetSelection(string guid) {
				SetSelection(SelectionTree.EnumerateTree().FirstOrDefault(item => ((AddressableAssetEntry)item.Value)?.guid == guid)?.Value as AddressableAssetEntry);
			}
		}

		protected class Drawer : LokiValueDrawer {
			protected override void DrawPropertyLayout(GUIContent label) {
				var value = Property.GetValue<AssetReference>();
				var type = Property.ValueEntry.TypeOfValue;
				while (type != null) {
					if (type.IsGenericType) {
						type = type.GenericTypeArguments.FirstOrDefault();
						break;
					}

					type = type.BaseType;
				}

				if (type == null) type = TypeOf<Object>.Raw;
				var rect = EditorGUILayout.GetControlRect(label != null);
				if (SirenixEditorGUI.IconButton(rect.AlignRight(rect.height), EditorIcons.Transparent)) {
					ShowMenu(Property, type);
					return;
				}

				EditorGUI.BeginChangeCheck();
				var asset = SirenixEditorFields.UnityObjectField(rect, label, value.editorAsset, type, false);
				if (EditorGUI.EndChangeCheck()) SetAsset(asset);
			}

			private void ShowMenu(InspectorProperty property, Type type) {
				var labels = property.Attributes.OfType<AssetReferenceUILabelRestriction>().SelectMany(restriction => restriction.m_AllowedLabels).ToHashSet();

				var selector = new AssetSelectorBase(type, labels);
				selector.SelectionConfirmed += OnSetAsset;
				selector.SetSelection(property.GetPropertyValue<string>("m_AssetGUID"));
				selector.ShowInPopup(400);
			}

			private void OnSetAsset(IEnumerable<AddressableAssetEntry> objects) {
				var guid = objects.FirstOrDefault()?.guid;
				Property.SetPropertyValue("m_AssetGUID", guid);
			}

			private async void SetAsset(Object asset) {
				await UniTask.Yield();
				var path = AssetDatabase.GetAssetPath(asset);
				if (AddressableAssetUtility.IsInResources(path))
					Addressables.LogWarning("Cannot use an AssetReference on an asset in Resources. Move asset out of Resources first. ");
				else if (!AddressableAssetUtility.IsPathValidForEntry(path)) Addressables.LogWarning("Dragged asset is not valid as an Asset Reference. " + path);

				AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long localId);

				var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
				var entry = settings.FindAssetEntry(guid);
				if (entry == null && !string.IsNullOrEmpty(guid)) {
					if (!AddressableAssetUtility.IsAssetPathInAddressableDirectory(settings, path, out var assetName)) {
						if (EditorUtility.DisplayDialog("Unused asset", "Asset not added to Addressable. Add his default group and set the required labels?", "Yes", "Cancel")) {
							var labels = Property.Attributes.OfType<AssetReferenceUILabelRestriction>().SelectMany(restriction => restriction.m_AllowedLabels).ToHashSet();
							var group = Property.Attributes.OfType<AssetGroupAttribute>().FirstOrDefault();
							var assetGroup = settings.DefaultGroup;
							if (group != null) {
								assetGroup = settings.FindGroup(group.GroupName);
								if (assetGroup == null) {
									EditorUtility.DisplayDialog("Unused asset", $"Can't find {group.GroupName} group in Addressable", "Ok");
									return;
								}
							}

							entry = settings.CreateOrMoveEntry(guid, assetGroup);
							foreach (var label in labels) entry.SetLabel(label, true);
							entry.SetAddress(asset.name);
						}
						else
							return;
					}
				}

				Property.SetPropertyValue("m_AssetGUID", guid);
			}
		}

		private static class AddressableAssetUtility {
			internal static bool IsAssetPathInAddressableDirectory(AddressableAssetSettings settings, string assetPath, out string assetName) {
				if (!string.IsNullOrEmpty(assetPath)) {
					var dir = Path.GetDirectoryName(assetPath);
					while (!string.IsNullOrEmpty(dir)) {
						var dirEntry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(dir));
						if (dirEntry != null) {
							assetName = dirEntry.address + assetPath.Remove(0, dir.Length);
							return true;
						}

						dir = Path.GetDirectoryName(dir);
					}
				}

				assetName = "";
				return false;
			}

			internal static bool IsPathValidForEntry(string path) {
				if (string.IsNullOrEmpty(path)) return false;
				path = path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
				if (!path.StartsWith("assets", StringComparison.OrdinalIgnoreCase) && !IsPathValidPackageAsset(path)) return false;
				if (path is UnityEditorResourcePath or UnityDefaultResourcePath or UnityBuiltInExtraPath) return false;
				if (path.EndsWith($"{Path.DirectorySeparatorChar}Editor") || path.Contains($"{Path.DirectorySeparatorChar}Editor{Path.DirectorySeparatorChar}")
					|| path.EndsWith("/Editor") || path.Contains("/Editor/"))
					return false;
				if (path == "Assets") return false;
				var settings = AddressableAssetSettingsDefaultObject.SettingsExists ? AddressableAssetSettingsDefaultObject.Settings : null;
				if ((settings != null && path.StartsWith(settings.ConfigFolder)) || path.StartsWith(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder)) return false;
				return !ExcludedExtensions.Contains(Path.GetExtension(path));
			}

			private static readonly HashSet<string> ExcludedExtensions = new(new[] {
				".cs",
				".js",
				".boo",
				".exe",
				".dll",
				".meta",
				".preset",
				".asmdef"
			});

			internal static bool IsPathValidPackageAsset(string path) {
				string[] splitPath = path.ToLower().Split(Path.DirectorySeparatorChar);

				if (splitPath.Length < 3) return false;
				if (splitPath[0] != "packages") return false;
				if (splitPath[2] == "package.json") return false;
				return true;
			}

			internal static bool IsInResources(string path) => path.Replace('\\', '/').ToLower().Contains("/resources/");

			private const string UnityEditorResourcePath = "library/unity editor resources";
			private const string UnityDefaultResourcePath = "library/unity default resources";
			private const string UnityBuiltInExtraPath = "resources/unity_builtin_extra";
		}
	}

}