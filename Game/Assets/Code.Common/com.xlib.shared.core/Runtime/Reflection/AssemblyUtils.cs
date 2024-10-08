using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace XLib.Core.Reflection {

	public static class AssemblyUtils {

		/// <summary>
		///     filter only own assemblies by name
		/// </summary>
		public static readonly Func<Assembly, bool> OwnAssemblies = x => x?.GetCustomAttribute<OwnAssemblyAttribute>() != null;

#if UNITY3D
		/// <summary>
		///     get all types from all dependend assemblies (only for Unity Client)
		/// </summary>
		public static IEnumerable<Type> GetAllTypes(this AppDomain domain, Func<Type, bool> typeFilter, Func<Assembly, bool> assemblyFilter = null) {
			RecursiveLoadAssemblies(domain, Assembly.GetEntryAssembly(), assemblyFilter);

			var q = domain.GetAssemblies().AsEnumerable();
			if (assemblyFilter != null) q = q.Where(assemblyFilter);

			if (typeFilter == null) return q.SelectMany(s => s.GetTypes());
			return q.SelectMany(s => s.GetTypes().Where(typeFilter));
		}

		/// <summary>
		///     get all public non-abstract classes from all dependend assemblies (only for Unity Client)
		/// </summary>
		public static IEnumerable<Type> GetDerivedClasses<T>(this AppDomain domain, bool publicOnly, Func<Assembly, bool> assemblyFilter = null)
			where T : class {
			RecursiveLoadAssemblies(domain, Assembly.GetEntryAssembly(), assemblyFilter);

			var q = domain.GetAssemblies().AsEnumerable();
			if (assemblyFilter != null) q = q.Where(assemblyFilter);

			var type = typeof(T);
			return q.SelectMany(s => s.GetTypes())
				.Where(p => type.IsAssignableFrom(p) && p.IsClass && (!publicOnly || p.IsPublic) && !p.IsAbstract);
		}

		public static void RecursiveLoadAssemblies(this AppDomain domain, Assembly assembly, Func<Assembly, bool> assemblyFilter) {
			if (assemblyFilter != null && !assemblyFilter(assembly)) return;

			foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies()) {
				var referencedAssembly = domain.Load(referencedAssemblyName);
				RecursiveLoadAssemblies(domain, referencedAssembly, assemblyFilter);
			}
		}
#endif		

		private static Dictionary<string, (Assembly assembly, string[] references)> GetRelevantAssemblies() {
			var result = new Dictionary<string, (Assembly assembly, string[] references)>();
			var assemblies = GetAssemblies();
			foreach (var asm in assemblies) {
				var name = asm.GetName().Name;
				var referenced = asm.GetReferencedAssemblies();
				result[name] = (asm, referenced.Select(x => x.Name).ToArray());
			}

			var directory = AppDomain.CurrentDomain.BaseDirectory;
			foreach (var asm in assemblies) {
				var name = asm.GetName().Name;
				var referenced = result[name].references;
				foreach (var assemblyName in referenced) {
					if (result.ContainsKey(assemblyName)) continue;
					if (File.Exists(Path.Combine(directory, $"{assemblyName}.dll"))) {
						Debug.Log($"Load Assembly: {assemblyName}");
						var assembly = Assembly.Load(assemblyName);
						result[assemblyName] = (assembly, assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray());
					}
				}
			}
			
			return result;
		}

		private static Assembly[] GetAssemblies() {
#if UNITY3D
			return AppDomain.CurrentDomain.GetAssemblies();
#else
			var currentContext = System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(typeof(AssemblyUtils).Assembly);
			var rootContext = System.Runtime.Loader.AssemblyLoadContext.Default;
			return new[] {currentContext, rootContext}.SelectMany(x => x.Assemblies).ToArray();
#endif
		}

		public static Assembly[] GetAssembliesReferencingAn(params Assembly[] assemblyList) {
			var assemblies = new HashSet<string>();
			var checkQueue = new Queue<string>();
			foreach (var assembly in assemblyList) {
				var rootName = assembly.GetName().Name;
				checkQueue.Enqueue(rootName);
				assemblies.Add(rootName);
			}

			var assemblyMeta = GetRelevantAssemblies();

			while (checkQueue.Count > 0) {
				var assemblyName = checkQueue.Dequeue();

				foreach (var (name, (_, references)) in assemblyMeta) {
					if (assemblies.Contains(name)) continue;
					if (!references.Contains(assemblyName)) continue;
					assemblies.Add(name);
					checkQueue.Enqueue(name);
				}
			}

			// Order assemblies in dependency order (Any assembly is later in the list than all of its dependencies)
			var list = new Assembly[assemblies.Count];
			var index = 0;
			var remaining = assemblies.Count;
			while (remaining > 0) {
				foreach (var assemblyName in assemblies) {
					var (asm, references) = assemblyMeta[assemblyName];
					if (!assemblies.Overlaps(references)) {
						list[index++] = asm;
						assemblies.Remove(assemblyName);
						break;
					}
				}

				if (remaining == assemblies.Count) {
					// Что то пошло не так - рекурсивные зависимости? Это вообще возможно?
					foreach (var asm in assemblies) list[index++] = assemblyMeta[asm].assembly;
					return list;
				}

				remaining = assemblies.Count;
			}

			return list;
		}

	}

}