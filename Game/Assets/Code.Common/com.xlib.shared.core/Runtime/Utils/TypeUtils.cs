using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using XLib.Core.Reflection;

namespace XLib.Core.Utils {

	/// <summary>
	///     parsed type info from single type string "namespace.name.typeName, AssemblyName, version"
	/// </summary>
	public struct TypeNameInfo {

		// form: "namespace.name.typeName, AssemblyName, version"
		public string AssemblyQualifiedName;

		// form: "namespace.name.typeName"
		public string FullTypeName;

		// form: "typeName"
		public string TypeName;

		public static TypeNameInfo From(string typeName) {
			var result = new TypeNameInfo { AssemblyQualifiedName = typeName };

			var commaIndex = result.AssemblyQualifiedName.IndexOf(',');
			result.FullTypeName = commaIndex > 0 ? result.AssemblyQualifiedName.Substring(0, commaIndex) : result.AssemblyQualifiedName;

			var lastDot = result.FullTypeName.LastIndexOf('.');
			result.TypeName = lastDot > 0 ? result.FullTypeName.Substring(lastDot + 1) : result.FullTypeName;

			return result;
		}

	}

	[SuppressMessage("ReSharper", "CheckNamespace")]
	public static class TypeUtils {

		private static IEnumerable<Assembly> Assemblies => AssemblyUtils.GetAssembliesReferencingAn(typeof(AssemblyUtils).Assembly); 

		/// <summary>
		///     return type with advanced Assembly lookup
		///     expected namespace format for auto type lookup: [AssemblyName].[Namespace1]. ... .[NamespaceN].[ClassName]
		/// </summary>
		public static Type GetType(string typeName) {
			if (typeName.IsNullOrEmpty()) return null;

			var result = Type.GetType(typeName, false, true);
			if (result != null) return result;

			foreach (var assembly in Assemblies) {
				if (assembly.IsDynamic) continue;

				result = assembly.GetType(typeName, false, true);
				if (result != null) return result;
			}

			return null;
		}

		/// <summary>
		///     return type with advanced Assembly lookup or throw exception
		/// </summary>
		public static Type GetExistingType(string typeName) {
			var result = GetType(typeName);
			if (result == null) throw new TypeLoadException($"Cannot find type '{typeName}'");

			return result;
		}

		public static IEnumerable<Type> EnumerateAll(Func<Type, bool> filter) {
			if (filter == null) throw new ArgumentNullException(nameof(filter));
			
			return Assemblies
				.Where(x => !x.IsDynamic)
				.SelectMany(x => x.GetTypes().Where(filter));
		}

	}

}