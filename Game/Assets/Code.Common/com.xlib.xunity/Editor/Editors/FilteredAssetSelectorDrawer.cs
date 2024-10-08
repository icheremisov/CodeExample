using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XLib.Core.Utils;
using XLib.Unity.Attributes;
using Object = UnityEngine.Object;

namespace XLib.Unity.Editors {

	[CustomPropertyDrawer(typeof(FilteredAssetSelectorAttribute))]
	public class FilteredAssetSelectorDrawer : PropertyDrawer {
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			var filter = ((FilteredAssetSelectorAttribute)attribute).Filter;
			Type t;
			if (filter is string methodName) {
				
				var parent = SerializedPropertyExtensions.GetNestedObjectParent<object>(property.propertyPath, property.serializedObject.targetObject);

				var method = parent.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				if (method == null) {
					GUI.Label(position, $"No method: '{methodName}'");
					return;
				}

				if (method.ReturnType != TypeOf<Type>.Raw || method.GetParameters().Length > 0) {
					GUI.Label(position, $"Method '{methodName}' must return Type and has no parameters");
					return;
				}

				t = (Type)method.Invoke(method.IsStatic ? null : parent, Array.Empty<object>());
			}
			else {
				t = filter as Type;
			}

			t ??= property.GetPropertyType();

			if (t == null) {
				GUI.Label(position, $"Error detecting type for property {property.name}");
				return;
			}
			// if (!TypeOf<Object>.IsAssignableFrom(t)) {
			// 	GUI.Label(position, $"Invalid type: {t.FullName}");
			// 	return;
			// }
			
			property.objectReferenceValue = EditorGUI.ObjectField(position, property.objectReferenceValue, t, false);
		}
		
	}

}