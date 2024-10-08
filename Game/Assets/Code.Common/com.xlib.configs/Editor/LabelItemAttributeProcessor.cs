using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;

namespace XLib.Configs {

	[AttributeUsage(AttributeTargets.All)]
	public class TagListAttribute : Attribute { }

	public class LabelItemAttributeProcessor : OdinAttributeProcessor<ICollection<LabelItem>> {
		private static InspectorProperty _property;

		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes) {
			_property = property;
			var attr = new ValueDropdownAttribute("") {
				ValuesGetter = $"@{GetType().FullName}.{nameof(GetAllTagItems)}()",
				IsUniqueList = true,
				DoubleClickToConfirm = false,
				DropdownWidth = 300,
				DropdownHeight = 500,
				SortDropdownItems = true
			};
			attributes.Add(attr);

			attributes.Add(new TagListAttribute());
		}

		public static IEnumerable GetAllTagItems() =>
			GameDatabase_Editor.GetDatabase()
				.Once<LabelContainer>()
				.RawElements
				.Select(x => new ValueDropdownItem(x.name, x));
	}

	public sealed class CustomRangeAttributeDrawer : OdinAttributeDrawer<TagListAttribute> {
		private const int Spacing = 5;
		private const int Offset = 3;
		private const float AddBtSize = 25;
		private float _width = 0;
		private float _lastHeight = 0;
		private float _screenWidthOnDrawRealContent = 0;
		private float _newScreenWidth = 0;
		private float _prevScreenWidth = 0;
		private bool _drawEmpty;
		private static GUIStyle _style;
		private AssetUserData _userData;
		private IGameDatabase _gameDatabase;

		private IGameDatabase GameDatabase =>
			_gameDatabase ??= GameDatabase_Editor.GetDatabase();

		protected override void Initialize() {
			_width = 0;
			_lastHeight = 0;
			_screenWidthOnDrawRealContent = 0;
			_newScreenWidth = 0;
			_prevScreenWidth = 0;
			_drawEmpty = false;
			_userData = AssetDatabaseUserData.LoadUserData<AssetUserData>(GameDatabase.Once<LabelContainer>());
		}

		private static void SetupStyle() =>
			_style ??= new GUIStyle(GUI.skin.label) {
				alignment = TextAnchor.MiddleCenter,
				fontSize = 12,
				normal = { textColor = Color.white },
				wordWrap = false,
				stretchWidth = false,
				padding = new RectOffset(10, 10, 2, 2),
				margin = new RectOffset(Spacing, Spacing, Spacing, Spacing)
			};

		protected override void DrawPropertyLayout(GUIContent label) {
			var values = (IList<LabelItem>)Property.TryGetTypedValueEntry<ICollection<LabelItem>>()?.SmartValue;

			var labelContent = new GUIContent(label.text);
			var labelWidth = GUI.skin.label.CalcSize(labelContent).x;
			if (values == null || values.Count == 0) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(labelContent, GUILayout.MaxWidth(labelWidth));
				GUILayout.FlexibleSpace();
				DrawAddButton();
				EditorGUILayout.EndHorizontal();
				return;
			}

			if (Event.current.type == EventType.Layout) _newScreenWidth = Screen.width;

			if (Math.Abs(_screenWidthOnDrawRealContent - _newScreenWidth) > float.Epsilon && Math.Abs(_prevScreenWidth - _newScreenWidth) > float.Epsilon) {
				if (!_drawEmpty && Event.current.type == EventType.Repaint) return;
				_drawEmpty = true;
				EditorGUILayout.BeginHorizontal(GUILayout.Height(_lastHeight));
				GUILayout.Label(new GUIContent(""), GUILayout.ExpandWidth(true));
				EditorGUILayout.EndHorizontal();

				if (Event.current.type != EventType.Repaint) return;
				_width = GUILayoutUtility.GetLastRect().width;
				_screenWidthOnDrawRealContent = _newScreenWidth;
				_drawEmpty = false;
				return;
			}

			if (Math.Abs(_width - 1.0f) < float.Epsilon) return;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelContent, GUILayout.MaxWidth(labelWidth));
			DrawAddButton();

			SetupStyle();
			var rect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			var index = 0;
			var cname = new GUIContent(values[index].name);
			var elementWidth = _style.CalcSize(cname).x + Spacing;
			var currWidth = AddBtSize + labelWidth;

			while (index < values.Count) {
				EditorGUILayout.BeginHorizontal();
				do {
					currWidth += elementWidth;
					var color = _userData.GetColor(values[index]) ?? Color.gray;
					var curRect = GUILayoutUtility.GetRect(cname, _style, GUILayout.ExpandWidth(false));
					GUI.DrawTexture(curRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, color, 0f, curRect.height * 0.5f);
					GUI.DrawTexture(curRect.Padding(Offset), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, color * 0.4f, 0f, (curRect.height - Offset * 2) * 0.5f);
					EditorGUI.LabelField(curRect, cname, _style);
					index++;
					if (index == values.Count) break;

					cname = new GUIContent(values[index].name);
					elementWidth = _style.CalcSize(cname).x + Spacing;
				} while (currWidth + elementWidth <= _width);

				currWidth = AddBtSize + labelWidth;
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Space();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			if (Event.current.type != EventType.Repaint) return;
			_lastHeight = rect.height + GUI.skin.window.padding.bottom;
			_prevScreenWidth = _newScreenWidth;
		}

		private void DrawAddButton() {
			if (!GUILayout.Button(EditorIcons.Plus.Raw, GUILayout.Height(AddBtSize), GUILayout.Width(AddBtSize))) return;
			ShowSelector(new Rect(Event.current.mousePosition, Vector2.zero)).SelectionChanged +=
				x => Property.TryGetTypedValueEntry<ICollection>().SmartValue = x.Select(y => (LabelItem)y).ToArray();
		}

		private OdinSelector<object> ShowSelector(Rect rect) {
			var drawer = Property.GetActiveDrawerChain().BakedDrawerArray.FirstOrDefaultType<ValueDropdownAttributeDrawer>();
			var selector = (GenericSelector<object>)drawer.GetType().GetMethod("CreateSelector", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(drawer, null);
			selector.DrawConfirmSelectionButton = false;
			rect.x = (int)rect.x;
			rect.y = (int)rect.y;
			rect.width = (int)rect.width;
			rect.height = (int)rect.height;
			selector.ShowInPopup(rect, new Vector2((float)300, (float)500));
			return selector;
		}
	}

}