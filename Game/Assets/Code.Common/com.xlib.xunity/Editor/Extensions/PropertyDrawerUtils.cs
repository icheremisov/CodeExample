using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XLib.Unity.Extensions {

	public static class PropertyDrawerUtils {
		private static int _lastIndent = 0;

		public static void ClearIndent() {
			_lastIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
		}

		public static void EndProperty() {
			EditorGUI.EndProperty();
			EditorGUI.indentLevel = _lastIndent;
		}

		public static void SetTooltip(this PropertyDrawer drawer, GUIContent label) {
			var attribute = drawer.fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true).FirstOrDefault();
			if (attribute != null) label.tooltip = ((TooltipAttribute)attribute).tooltip;
		}
	}

}