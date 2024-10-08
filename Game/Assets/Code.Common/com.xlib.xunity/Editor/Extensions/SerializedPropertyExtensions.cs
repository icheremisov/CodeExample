// --------------------------------------------------------------------------------------------------------------------
// <author>
//   HiddenMonk
//   http://answers.unity3d.com/users/496850/hiddenmonk.html
//   
//   Johannes Deml
//   send@johannesdeml.com
// </author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XLib.Core.Reflection;

// ReSharper disable MemberCanBePrivate.Global

/// <summary>
///     Extension class for SerializedProperties
///     See also: http://answers.unity3d.com/questions/627090/convert-serializedproperty-to-custom-class.html
/// </summary>
// ReSharper disable once CheckNamespace
public static class SerializedPropertyExtensions {
	/// <summary>
	///     Get the object the serialized property holds by using reflection
	/// </summary>
	/// <typeparam name="T">The object type that the property contains</typeparam>
	/// <param name="property"></param>
	/// <returns>Returns the object type T if it is the type the property actually contains</returns>
	public static T GetValue<T>(this SerializedProperty property) => GetNestedObject<T>(property.propertyPath, GetSerializedPropertyRootObject(property));

	/// <summary>
	///     Set the value of a field of the property with the type T
	/// </summary>
	/// <typeparam name="T">The type of the field that is set</typeparam>
	/// <param name="property">The serialized property that should be set</param>
	/// <param name="value">The new value for the specified property</param>
	/// <returns>Returns if the operation was successful or failed</returns>
	public static bool SetValue<T>(this SerializedProperty property, T value) {
		var obj = GetSerializedPropertyRootObject(property);
		//Iterate to parent object of the value, necessary if it is a nested object
		var fieldStructure = property.propertyPath.Split('.');
		for (var i = 0; i < fieldStructure.Length - 1; i++) obj = GetFieldOrPropertyValue<object>(fieldStructure[i], obj);

		var fieldName = fieldStructure.Last();

		return SetFieldOrPropertyValue(fieldName, obj, value);
	}

	/// <summary>
	///     Get the component of a serialized property
	/// </summary>
	/// <param name="property">The property that is part of the component</param>
	/// <returns>The root component of the property</returns>
	public static object GetSerializedPropertyRootObject(this SerializedProperty property) => property.serializedObject.targetObject;

	/// <summary>
	///     Iterates through objects to handle objects that are nested in the root object
	/// </summary>
	/// <typeparam name="T">The type of the nested object</typeparam>
	/// <param name="path">Path to the object through other properties e.g. PlayerInformation.Health</param>
	/// <param name="obj">The root object from which this path leads to the property</param>
	/// <param name="includeAllBases">Include base classes and interfaces as well</param>
	/// <returns>Returns the nested object cast to the type T</returns>
	public static T GetNestedObject<T>(string path, object obj, bool includeAllBases = false) {
		// _rewardSteps.Array.data[0]._rewards.Array.data[0]._item
		var split = path.Split('.');
		var parseArray = false;

		foreach (var part in split) {
			if (part == "Array") {
				parseArray = true;
				continue;
			}

			if (parseArray) {
				var fn = part.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
				Debug.Assert(fn.Length == 2);

				var arrayIndex = int.Parse(fn[1]);
				var array = (IList)obj;
				if (arrayIndex < 0 || arrayIndex >= array.Count) return default;
				obj = array[arrayIndex];

				parseArray = false;
			}
			else
				obj = GetFieldOrPropertyValue<object>(part, obj, includeAllBases);
		}

		return (T)obj;
	}

	public static T GetNestedObjectParent<T>(string path, object obj, bool includeAllBases = false) {
		// _rewardSteps.Array.data[0]._rewards.Array.data[0]._item
		var split = path.Split('.');
		var parseArray = false;

		var parentObj = obj;

		foreach (var part in split) {
			if (part == "Array") {
				parseArray = true;
				continue;
			}

			if (parseArray) {
				var fn = part.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
				Debug.Assert(fn.Length == 2);

				var arrayIndex = int.Parse(fn[1]);
				var array = (IList)obj;

				parentObj = obj;
				obj = array[arrayIndex];

				parseArray = false;
			}
			else {
				parentObj = obj;
				obj = GetFieldOrPropertyValue<object>(part, obj, includeAllBases);
			}
		}

		return (T)parentObj;
	}

	public static T GetFieldOrPropertyValue<T>(string fieldName, object obj, bool includeAllBases = false,
		BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
		if (obj == null) return default;

		var field = obj.GetType().GetField(fieldName, bindings);
		if (field != null) return (T)(field.GetValue(obj));

		var property = obj.GetType().GetProperty(fieldName, bindings);
		if (property != null) return (T)(property.GetValue(obj, null));

		if (!includeAllBases) return default;
		
		foreach (var type in obj.GetType().GetBaseClassesAndInterfaces()) {
			field = type.GetField(fieldName, bindings);
			if (field != null) return (T)(field.GetValue(obj));

			property = type.GetProperty(fieldName, bindings);
			if (property != null) return (T)(property.GetValue(obj, null));
		}

		return default;
	}

	public static T GetFieldOrPropertyValue<T>(string fieldName, int arrayIndex, object obj, bool includeAllBases = false,
		BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
		if (obj == null) return default;

		var field = obj.GetType().GetField(fieldName, bindings);
		if (field != null) return ConvertValue(field.GetValue(obj));

		var property = obj.GetType().GetProperty(fieldName, bindings);
		if (property != null) return ConvertValue(property.GetValue(obj, null));

		if (includeAllBases) {
			foreach (var type in obj.GetType().GetBaseClassesAndInterfaces()) {
				field = type.GetField(fieldName, bindings);
				if (field != null) return ConvertValue(field.GetValue(obj));

				property = type.GetProperty(fieldName, bindings);
				if (property != null) return ConvertValue(property.GetValue(obj, null));
			}
		}

		T ConvertValue(object value) {
			var array = (IList)value;
			return (T)array[arrayIndex];
		}

		return default;
	}

	public static bool SetFieldOrPropertyValue(string fieldName, object obj, object value, bool includeAllBases = false,
		BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
		var field = obj.GetType().GetField(fieldName, bindings);
		if (field != null) {
			field.SetValue(obj, value);
			return true;
		}

		var property = obj.GetType().GetProperty(fieldName, bindings);
		if (property != null) {
			property.SetValue(obj, value, null);
			return true;
		}

		if (!includeAllBases) return false;

		foreach (var type in obj.GetType().GetBaseClassesAndInterfaces()) {
			field = type.GetField(fieldName, bindings);
			if (field != null) {
				field.SetValue(obj, value);
				return true;
			}

			property = type.GetProperty(fieldName, bindings);
			if (property == null) continue;
			property.SetValue(obj, value, null);
			return true;
		}

		return false;
	}

	public static Type GetPropertyType(this SerializedProperty property) {
		var parent = GetNestedObjectParent<object>(property.propertyPath, property.serializedObject.targetObject);

		var field = parent.GetType().GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		return field?.FieldType;
	}
}