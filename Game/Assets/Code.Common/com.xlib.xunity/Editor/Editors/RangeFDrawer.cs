using UnityEditor;
using UnityEngine;
using XLib.Core.CommonTypes;

namespace XLib.Unity.Editors {

	[CustomPropertyDrawer(typeof(RangeF))]
	public class RangeFDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var rect1 = new Rect(position.x, position.y, 50, position.height);
			var rect2 = new Rect(position.x + 55, position.y, 50, position.height);

			EditorGUI.PropertyField(rect1, property.FindPropertyRelative("min"), GUIContent.none);
			EditorGUI.PropertyField(rect2, property.FindPropertyRelative("max"), GUIContent.none);

			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}

	}

}