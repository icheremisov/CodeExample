using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using Object = UnityEngine.Object;

namespace XLib.Configs {

	public sealed class AssetListDropdownAttributeDrawer : OdinAttributeDrawer<AssetListDropdownAttribute> {
		private struct Info {
			public Object Asset { get; set; }
			public string Path { get; set; }
		}

		private Type _propType;
		private Info[] _assets;
		private int _index = 0;
		private bool _changed;

		private ValueResolver<bool> _disableIfResolver;

		protected override void Initialize() {
			_propType = Property.ValueEntry.BaseValueType ?? Property.ValueEntry.TypeOfValue;
			_assets = AssetDatabase.FindAssets($"t:{_propType}", null)
				.Select(AssetDatabase.GUIDToAssetPath)
				.SelectMany(x => {
					var assets = AssetDatabase.LoadAllAssetsAtPath(x);

					return assets.Length > 1 && assets.Any(AssetDatabase.IsMainAsset)
						? assets.Where(a => !AssetDatabase.IsMainAsset(a)).Select(y => new Info { Asset = y, Path = x })
						: assets.Select(y => new Info { Asset = y, Path = x });
				})
				.Distinct()
				.ToArray();
			base.Initialize();
		}

		protected override void DrawPropertyLayout(GUIContent label) {
			var condition = Property.GetAttribute<DisableIfAttribute>()?.Condition;
			if (!condition.IsNullOrEmpty()) {
				_disableIfResolver = ValueResolver.Get<bool>(Property, condition);
				GUI.enabled = !_disableIfResolver.GetValue();
			}

			EditorGUILayout.BeginHorizontal();

			var value = Property.TryGetTypedValueEntry<Object>();
			var name = value != null
				? value.SmartValue != null
					? value.SmartValue.name
					: ""
				: "";
			if (EditorGUILayout.DropdownButton(new GUIContent(name), FocusType.Passive)) {
				Show().Forget();
			}

			EditorGUILayout.EndHorizontal();

			if (_changed) {
				_changed = false;
				Property.TryGetTypedValueEntry<Object>().SmartValue = _assets[_index].Asset;
			}

			GUI.enabled = true;
		}

		private async UniTask Show() {
			_changed = false;
			_index = -1;
			await UniTask.Yield();
			_index = PopupWindow.Init(_assets);
			if (_index == -1) return;
			_changed = true;
		}

		private class PopupWindow : EditorWindow {
			private static Vector2 _scrollPos;
			private static int _selected;
			private static Info[] _assets;
			private static PopupWindow _wnd;
			private static GUIStyle _style;
			private string _text;

			public static int Init(Info[] assets) {
				_scrollPos = Vector2.zero;
				_selected = -1;
				_assets = assets;
				_wnd = GetWindow<PopupWindow>();
				_wnd.minSize = new Vector2(200, 0);
				_wnd.maxSize = new Vector2(300, 400);
				_wnd.ShowModal();
				return _selected;
			}

			private void OnGUI() {
				_style ??= new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };

				var baseRect = EditorGUILayout.GetControlRect(false, 20);
				_text = GUI.TextField(baseRect, _text, _style);

				_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);

				for (var i = 0; i < _assets.Length; i++) {
					if (!_text.IsNullOrEmpty() && !_assets[i].Asset.name.ToLower().StartsWith(_text.ToLower())) continue;
					var icon = (Texture)EditorGUIUtility.GetIconForObject(_assets[i].Asset);
					icon ??= AssetDatabase.GetCachedIcon(_assets[i].Path);

					if (GUILayout.Button(new GUIContent(_assets[i].Asset.name, icon), _style, GUILayout.Height(20), GUILayout.ExpandWidth(true))) {
						_selected = i;
						_wnd.Close();
					}
				}

				EditorGUILayout.EndScrollView();
			}
		}
	}

}