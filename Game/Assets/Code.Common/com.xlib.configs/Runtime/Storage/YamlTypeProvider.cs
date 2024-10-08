#if !UNITY3D
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XLib.Configs.Core;
using YamlDotNet.RepresentationModel;

namespace XLib.Configs.Storage {

	public interface IConfigTypeProvider {

		Type GetTypeFromGuid(string guid);

		Type GetTypeByName(string className, string namespaceName, string assemblyName);
	}

	public class ManifestTypeProvider : IConfigTypeProvider {

		private readonly Dictionary<string, Type> _types = new(128);
		private readonly Assembly[] _assemblies;

		public ManifestTypeProvider(Assembly[] assemblies) => _assemblies = assemblies;
		
		public void PhaseExtractTypes(YamlStorage storage) {
			var manifest = storage.GetEntryByName("types_list.asset");
			var behaviour = manifest.YamlNode["MonoBehaviour"];

			var scripts = behaviour[TypesManifest.ScriptsName] as YamlSequenceNode;
			foreach (var script in scripts.Children) {
				var typename = ((YamlScalarNode)script[TypesManifest.ScriptInfo.TypeName]).Value ?? throw new ArgumentNullException(TypesManifest.ScriptInfo.TypeName);
				var guid = ((YamlScalarNode)script[TypesManifest.ScriptInfo.GuidName]).Value ?? throw new InvalidOperationException(TypesManifest.ScriptInfo.GuidName);
				var type = _assemblies.Select(asm => asm.GetType(typename)).FirstOrDefault(asmType => asmType != null);
				if (type == null) Debug.LogError($"Type {typename} not found (guid: {guid})");
				_types[guid] = type;
			}
		}

		public Type GetTypeFromGuid(string guid) => _types.TryGetValue(guid, out var result) ? result : throw new KeyNotFoundException($"Cannot find type for GUID '{guid}'");

		public Type GetTypeByName(string className, string namespaceName, string assemblyName) {
			var assembly = _assemblies.First(assembly => assembly.GetName().Name == assemblyName);
			return assembly.GetType($"{namespaceName}.{className}");
		}

	}

}
#endif