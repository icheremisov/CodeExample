#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using XLib.Core.Reflection;
using XLib.Core.Utils;

#if !DEVELOPMENT_BUILD
using System.Linq;
#else
using Debug = System.Diagnostics.Debug;
#endif

namespace XLib.Core.Json.Net {

	// ReSharper disable once UnusedType.Global
	public class JsonNetSerializationBinder : DefaultSerializationBinder {
		private Dictionary<string, Type> TypeList { get; } = LoadTypes();

		public override Type BindToType(string? assemblyName, string typeName) {
			if (!typeName.StartsWith("#")) return base.BindToType(assemblyName, typeName);

			return TypeList.FirstOrDefault(typeName) ?? throw new Exception($"Type is not found with id='{typeName}'");
		}

		public override void BindToName(Type serializedType, out string? assemblyName, out string? typeName) {
			var attr = serializedType.GetAttribute<JsonSerializeAttribute>();
			if (attr == null) {
				base.BindToName(serializedType, out assemblyName, out typeName);
				return;
			}

#if DEVELOPMENT_BUILD
			var realType = TypeList.FirstOrDefault(attr.Guid) ??
				throw new Exception($"Type map entry is not found with id='{attr.Guid}' name={serializedType.FullName}");

			Debug.Assert(realType == serializedType, $"Types mismatch: in table found {realType} but actual is {serializedType}");
#endif

			assemblyName = null;
			typeName = attr.Guid;
		}

		private static Dictionary<string, Type> LoadTypes() {
#if DEVELOPMENT_BUILD
			var result = new Dictionary<string, Type>(128);

			foreach (var x in TypeUtils.EnumerateAll(x => x.HasAttribute<JsonSerializeAttribute>())) {
				var key = x.GetAttribute<JsonSerializeAttribute>().Guid;
				if (result.ContainsKey(key)) throw new Exception($"Duplicated GUID found on class '{x.FullName}'");

				result.Add(key, x);
			}

			return result;
#else
			return TypeUtils.EnumerateAll(x => x.HasAttribute<JsonSerializeAttribute>())
				.ToDictionary(x => x.GetAttribute<JsonSerializeAttribute>().Guid, x => x);
#endif
		}

	}

}