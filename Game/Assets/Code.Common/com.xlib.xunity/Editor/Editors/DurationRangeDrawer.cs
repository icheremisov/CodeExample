using UnityEditor;
using UnityEngine;
using XLib.Core.CommonTypes;

namespace XLib.Unity.Editors {

	[CustomPropertyDrawer(typeof(DurationRange))]
	public class DurationRangeDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			var indent = EditorGUI.indentLevel;
			var labelWidth = EditorGUIUtility.labelWidth;
			EditorGUI.indentLevel = 0;
			EditorGUIUtility.labelWidth = 30;

			var minRect = new Rect(position.x, position.y, position.width / 2f - 2f, position.height);
			var maxRect = new Rect(position.x + position.width / 2f + 4f, position.y, position.width / 2f - 2f, position.height);

			EditorGUI.PropertyField(minRect, property.FindPropertyRelative("min.Value"), new GUIContent("Min"));
			EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("max.Value"), new GUIContent("Max"));

			EditorGUI.indentLevel = indent;
			EditorGUIUtility.labelWidth = labelWidth;

			EditorGUI.EndProperty();
		}
	}

}