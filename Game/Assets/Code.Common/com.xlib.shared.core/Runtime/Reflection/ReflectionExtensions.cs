using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using XLib.Core.Utils;

namespace XLib.Core.Reflection {

	[SuppressMessage("ReSharper", "CheckNamespace")]
	public static class ReflectionExtensions {

		private static readonly Type TypeObject = typeof(object);

		private static readonly ConcurrentDictionary<Type, TypeInfoEx> TypeInfo = new();

		static ReflectionExtensions() {
			LocalizationExtensions.ForceIncluded();
		}

		public static bool IsGenericList(this Type oType) => oType.GetTypeInfo().IsGenericType && oType.GetGenericTypeDefinition() == typeof(List<>);

		public static bool IsGenericDictionary(this Type oType) => oType.GetTypeInfo().IsGenericType && oType.GetGenericTypeDefinition() == typeof(Dictionary<,>);

		public static bool IsGenericHashSet(this Type oType) => oType.GetTypeInfo().IsGenericType && oType.GetGenericTypeDefinition() == typeof(HashSet<>);

		public static IEnumerable<PropertyInfo> EnumerateInstanceProperties(this Type t, bool includeBaseClasses) {
			while (t != null && t != TypeObject) {
				foreach (var item in t.GetTypeInfo().DeclaredProperties) yield return item;

				if (!includeBaseClasses) break;

				t = t.BaseType;
			}
		}

		public static IEnumerable<FieldInfo> EnumerateInstanceFields(this Type t, bool includeBaseClasses) {
			while (t != null && t != TypeObject) {
				foreach (var item in t.GetTypeInfo().DeclaredFields.Where(x => !x.IsStatic)) yield return item;

				if (!includeBaseClasses) break;

				t = t.BaseType;
			}
		}

		public static IEnumerable<MemberInfo> EnumerateInstanceFieldsAndProperties(this Type t, bool includeBaseClasses) {
			foreach (var m in t.EnumerateInstanceProperties(includeBaseClasses)) yield return m;
			foreach (var m in t.EnumerateInstanceFields(includeBaseClasses)) yield return m;
		}

		public static bool HasAttribute<T>(this Type t) where T : Attribute => t?.GetCustomAttribute<T>(true) != null;

		public static bool HasAttribute<T>(this MemberInfo t) where T : Attribute => Attribute.GetCustomAttribute(t, typeof(T), true) != null;

		/// <summary>
		///     check attribute by full class name
		/// </summary>
		public static bool HasAttribute(this MemberInfo t, string fullAttrClassName) {
			return Attribute.GetCustomAttributes(t, true).Any(x => x.GetType().FullName == fullAttrClassName);
		}

		public static bool IsEnum(this Type t) => t.GetTypeInfo().IsEnum;

		public static bool IsClass(this Type t) => t.GetTypeInfo().IsClass;

		public static bool IsInterface(this Type t) => t.GetTypeInfo().IsInterface;

		public static Type Get1stGenericArgument(this Type t) => GetGenericArgument(t, 0);

		public static Type GetGenericArgument(this Type t, int index) => t.GetTypeInfo().GenericTypeArguments[index];

		public static MethodInfo GetDeclaredMethod(this Type t, string methodName) => t.GetTypeInfo().GetDeclaredMethod(methodName);

		/// <summary>
		///     check if type derived from generic type
		/// </summary>
		public static bool IsSubclassOfRawGeneric(this Type t, Type generic) {
			while (t != null && t != typeof(object)) {
				var cur = t.IsGenericType ? t.GetGenericTypeDefinition() : t;
				if (generic == cur) return true;
				t = t.BaseType;
			}

			return false;
		}

		/// <summary>
		///     check type implemented IInterface`T or BaseClass`T
		///     usage: componentType.IsImplementGeneric(typeof(ValueComponent`T))
		/// </summary>
		public static bool IsImplementGeneric(this Type type, Type iType) {
			if (!iType.IsGenericTypeDefinition) return iType.IsAssignableFrom(type);

			if (iType.IsInterface) {
				return type.GetInterfaces()
					.Where(i => i.IsGenericType)
					.Any(i => i.GetGenericTypeDefinition() == iType);
			}

			var t = type;

			while (t != null && t != TypeOf<object>.Raw) {
				if (t.IsGenericType && t.GetGenericTypeDefinition() == iType) return true;

				t = t.BaseType;
			}

			return false;
		}
		public static bool IsImplementGeneric<TImpl>(this Type type) => IsImplementGeneric(type, TypeOf<TImpl>.Raw);

		/// <summary>
		///     return type implemented IInterface`T or BaseClass`T
		///     usage: componentType.GetGenericImplementation(typeof(ValueComponent`T))
		/// </summary>
		public static Type GetGenericImplementation(this Type type, Type iType) {
			if (!iType.IsGenericTypeDefinition) return iType;

			if (iType.IsInterface) {
				return type.GetInterfaces()
					.Where(i => i.IsGenericType)
					.FirstOrDefault(i => i.GetGenericTypeDefinition() == iType);
			}

			var t = type;

			while (t != null && t != TypeOf<object>.Raw) {
				if (t.IsGenericType && t.GetGenericTypeDefinition() == iType) return t;

				t = t.BaseType;
			}

			return null;
		}

		public static IEnumerable<Type> GetBaseClassesAndInterfaces(this Type type, bool includeSelf = false) {
			if (type == null) return Enumerable.Empty<Type>();

			var allTypes = new List<Type>(16);

			if (includeSelf) allTypes.Add(type);

			if (type.BaseType == TypeOf<object>.Raw)
				allTypes.AddRange(type.GetInterfaces());
			else
				allTypes.AddRange(Enumerable.Repeat(type.BaseType, 1).Concat(type.GetInterfaces()).Concat(GetBaseClassesAndInterfaces(type.BaseType)).Distinct());

			return allTypes;
		}

		public static IEnumerable<PropertyInfo> GetInstanceProperties(this Type type) {
			return TypeInfo.GetOrAddValue(type, () => new TypeInfoEx(type)).Properties.Values;
		}

		public static IEnumerable<FieldInfo> GetInstanceFields(this Type type) {
			return TypeInfo.GetOrAddValue(type, () => new TypeInfoEx(type)).Fields.Values;
		}

		public static IEnumerable<MemberInfo> GetInstanceFieldsAndProperties(this Type type) {
			foreach (var m in type.GetInstanceProperties()) yield return m;
			foreach (var m in type.GetInstanceFields()) yield return m;
		}

		public static PropertyInfo FindInstancedProperty(this Type type, string name) {
			return TypeInfo.GetOrAddValue(type, () => new TypeInfoEx(type)).Properties.FirstOrDefault(name);
		}

		public static FieldInfo FindInstancedField(this Type type, string name) {
			return TypeInfo.GetOrAddValue(type, () => new TypeInfoEx(type)).Fields.FirstOrDefault(name);
		}

		public static T GetAttribute<T>(this Type type) where T : Attribute {
			return (T)TypeInfo.GetOrAddValue(type, () => new TypeInfoEx(type)).Attributes.FirstOrDefault(TypeOf<T>.Raw);
		}

		public static IEnumerable<T> GetAttributes<T>(this Type type) where T : Attribute {
			return TypeInfo.GetOrAddValue(type, () => new TypeInfoEx(type)).Attributes.Values.OfType<T>();
		}

		public static T GetAttribute<T>(this ICustomAttributeProvider member, bool inherit = false) where T : Attribute =>
			member.GetCustomAttributes(TypeOf<T>.Raw, inherit).Cast<T>().FirstOrDefault();

		public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider member, bool inherit = false) where T : Attribute =>
			member.GetCustomAttributes(TypeOf<T>.Raw, inherit).Cast<T>();

		private class TypeInfoEx {

			private readonly Type _type;
			private Dictionary<Type, Attribute> _attributes;
			private Dictionary<string, FieldInfo> _fields;

			private Dictionary<string, PropertyInfo> _properties;

			public TypeInfoEx(Type t) {
				_type = t;
			}

			public Dictionary<string, PropertyInfo> Properties => _properties ??= _type.EnumerateInstanceProperties(true).ToDictionary(x => x.Name);
			public Dictionary<string, FieldInfo> Fields => _fields ??= _type.EnumerateInstanceFields(true).ToDictionary(x => x.Name);
			public Dictionary<Type, Attribute> Attributes => _attributes ??= _type.GetCustomAttributes(false).ToDictionary(x => x.GetType(), x => (Attribute)x);

		}

		public static Type ToNullableType(this Type type) {
			// Use Nullable.GetUnderlyingType() to remove the Nullable<T> wrapper if type is already nullable.
			type = Nullable.GetUnderlyingType(type) ?? type; // avoid type becoming null
			return type.IsValueType ? typeof(Nullable<>).MakeGenericType(type) : type;
		}
		
		public static Type FromNullableType(this Type nullableType) {
			return Nullable.GetUnderlyingType(nullableType) ?? nullableType; // avoid type becoming null
		}		
		
		public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;
		
		public static bool IsNullOrDefault<T>(this T argument)
		{
			// deal with normal scenarios
			if (argument == null) return true;

			if (EqualityComparer<T>.Default.Equals(argument, default)) return true;

			// deal with non-null nullables
			var methodType = TypeOf<T>.Raw;
			if (Nullable.GetUnderlyingType(methodType) != null) return false;

			// deal with boxed value types
			var argumentType = argument.GetType();
			if (argumentType.IsValueType && argumentType != methodType)
			{
				var obj = Activator.CreateInstance(argument.GetType());
				return obj.Equals(argument);
			}

			return false;
		}		
	}

}