using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace XLib.Core.Utils {

	[SuppressMessage("ReSharper", "CheckNamespace"), SuppressMessage("ReSharper", "StaticMemberInGenericType")]
	public static class TypeOf<T> {

		public static readonly Type Raw = typeof(T);

		public static readonly string Name = Raw.Name;
		public static readonly string FullName = Raw.FullName;
		public static readonly TypeCode TypeCode = Type.GetTypeCode(Raw);
		public static readonly Assembly Assembly = Raw.Assembly;
		public static readonly bool IsValueType = Raw.IsValueType;

		public static bool IsAssignableFrom(Type other) => Raw.IsAssignableFrom(other);
		public static bool IsAssignableFrom<TOther>() => Raw.IsAssignableFrom(TypeOf<TOther>.Raw);

		public static IEnumerable<string> InheritorsNames => TypeCache<T>.Names;
		public static IEnumerable<Type> Inheritors => TypeCache<T>.CachedTypes;
		public static Type GetInheritorById(Guid guid) => TypeCache<T>.FirstOrDefault(guid);

	}

}