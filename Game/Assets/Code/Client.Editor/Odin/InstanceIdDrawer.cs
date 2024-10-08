using System;
using Entitas.VisualDebugging.Unity.Editor;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;

namespace Client.Odin {

	[CustomPropertyDrawer(typeof(InstanceId))]
	public class InstanceIdDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			property.intValue = EditorGUI.IntField(position, property.intValue);
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}

	public class InstanceIdTypeDrawer : ITypeDrawer {
		public bool HandlesType(Type type) => type == typeof(InstanceId);

		public object DrawAndGetNewValue(
			Type memberType,
			string memberName,
			object value,
			object target) {
			return (object)EditorGUILayout.IntField(memberName, (int)value);
		}
	}

}