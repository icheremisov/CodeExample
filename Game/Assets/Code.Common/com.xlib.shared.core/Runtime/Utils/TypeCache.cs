using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XLib.Core.Utils.Attributes;

namespace XLib.Core.Utils {

	public static class TypeCache<TBaseType> {

		// ReSharper disable once StaticMemberInGenericType
		private static Dictionary<string, Type> _map;
		private static Dictionary<Guid, Type> _ids;

		public static IEnumerable<string> Names {
			get {
				CacheTypes();
				return _map.Keys.AsEnumerable();
			}
		}

		public static IEnumerable<Type> CachedTypes {
			get {
				CacheTypes();
				return _map.Values.AsEnumerable();
			}
		}

		public static Type FirstOrDefault(string shortTypeName) {
			CacheTypes();

			return _map.FirstOrDefault(shortTypeName);
		}

		public static Type FirstOrDefault(Guid guid) {
			CacheIdsTypes();
			return _ids.FirstOrDefault(guid);
		}

		private static void CacheTypes() {
			if (_map != null) return;
			_map = new Dictionary<string, Type>();
			foreach (var type in TypeUtils.EnumerateAll(x => x.IsClass && !x.IsAbstract && TypeOf<TBaseType>.Raw.IsAssignableFrom(x))) 
				_map[type.Name] = type;
		}	
		private static void CacheIdsTypes() {
			if (_ids != null) return;
			CacheTypes();
			_ids = new Dictionary<Guid, Type>();
			foreach (var type in _map.Values) {
				var attribute = type.GetCustomAttribute<TypeIdAttribute>();
				if (attribute == null) continue;
				_ids.Add(attribute.Id, type);
			}
		}

		public static TBaseType Instantiate(string shortTypeName, params object[] args) {
			var type = FirstOrDefault(shortTypeName);
			if (type == null) return default;

			return (TBaseType)Activator.CreateInstance(type, args);
		}

	}

}