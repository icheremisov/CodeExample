using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;
using XLib.Configs.Contracts;

namespace XLib.Configs.Odin {

	[UsedImplicitly]
	public class EnumBurgerAttributeDrawer<T> : OdinAttributeDrawer<EnumBurgerAttribute, T> {
		private (GUIContent name, ulong value)[] _members;
		private bool _isFlags;
		private List<int> _columnCounts;
		private float _previousControlRectWidth;
		private GUIStyle _style;
		private EnumBurgerAttribute _attribute;

		public override bool CanDrawTypeFilter(Type type) => type.IsEnum;

		protected override void Initialize() {
			_style = new GUIStyle(SirenixGUIStyles.MiniButtonMid) { fontSize = 10, margin = new RectOffset(), border = new RectOffset() };

			_attribute = ValueEntry.Property.Attributes.GetAttribute<EnumBurgerAttribute>();
			var enumType = ValueEntry.TypeOfValue;
			_members = EnumTypeUtilities<T>.VisibleEnumMemberInfos
				.Select(x => (name: new GUIContent(x.NiceName),
					value: TypeExtensions.GetEnumBitmask(Enum.Parse(enumType, x.Name), enumType)))
				.ToArray();
			_isFlags = enumType.IsDefined<FlagsAttribute>();
		}

		protected override void DrawPropertyLayout(GUIContent label) {
			var valueEntry = ValueEntry;
			var type = valueEntry.WeakValues[0].GetType();
			for (var i = 1; i < valueEntry.WeakValues.Count; ++i) {
				if (type == valueEntry.WeakValues[i].GetType()) continue;
				SirenixEditorGUI.ErrorMessageBox("ToggleEnum does not support multiple different enum types.");
				return;
			}

			var value = TypeExtensions.GetEnumBitmask(valueEntry.SmartValue, typeof(T));

			if (_attribute.IsHorizontal)
				SirenixEditorGUI.BeginIndentedHorizontal();
			else
				SirenixEditorGUI.BeginIndentedVertical();

			for (var index = 0; index < _members.Length; ++index) {
				bool on;
				if (_isFlags) {
					var enumBitmask = TypeExtensions.GetEnumBitmask(_members[index].value, typeof(T));
					on = value != 0UL ? enumBitmask != 0UL && ((long)enumBitmask & (long)value) == (long)enumBitmask : enumBitmask == 0UL;
				}
				else
					on = (long)_members[index].value == (long)value;

				var result = GUILayout.Toggle(on, _members[index].name, _style);
				if (result != on) {
					GUIHelper.RemoveFocusControl();
					if (!_isFlags || Event.current.button == 1 || Event.current.modifiers == EventModifiers.Control) {
						valueEntry.WeakSmartValue = Enum.ToObject(typeof(T), _members[index].value);
					}
					else {
						if (_members[index].value == 0UL)
							value = 0UL;
						else if (on)
							value &= ~_members[index].value;
						else
							value |= _members[index].value;
						valueEntry.WeakSmartValue = Enum.ToObject(typeof(T), value);
					}
				}
			}

			if (_attribute.IsHorizontal)
				SirenixEditorGUI.EndIndentedHorizontal();
			else
				SirenixEditorGUI.EndIndentedVertical();
		}
	}

}