using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Sheets.Contracts;
using XLib.Configs.Sheets.Core;
using XLib.Configs.Utils;
using XLib.Core.Reflection;
using XLib.Core.Utils;

namespace XLib.Configs.Sheets.Serialize {

	public interface ISheetTypeSchema {
		public SheetRowProperty KeyProperty { get; }

		public IEnumerable<SheetRowProperty> Properties { get; }

		public SheetRowProperty FindProperty(string name);
	}

	public class SheetTypeSchema : ISheetTypeSchema {
		protected List<SheetRowProperty> _properties = new();

		public SheetTypeSchema(Type type) {
			ExtractProperties(type);
			Prepare();
		}

		protected SheetTypeSchema() { }

		protected void Prepare() {
			KeyProperty = _properties.FirstOrDefault(property => property.IsKey);
			_properties.Sort((p1, p2) => p2.Priority == p1.Priority ? ((p2.IsKey ? 1 : 0) - (p1.IsKey ? 1 : 0)) : p1.Priority - p2.Priority);
			var idx = 0;
			foreach (var property in _properties) {
				property.Index = idx;
				++idx;
			}
		}

		protected void ExtractProperties(Type type) {

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) {
				_properties.Add(SheetRowProperty.Create(type.GetProperty("Key"), Array.Empty<Attribute>()));
				_properties.Add(SheetRowProperty.Create(type.GetProperty("Value"), Array.Empty<Attribute>()));
				return;
			}
			
			var all = type.GetMembers(BindingFlags.Instance | BindingFlags.Public)
				.Concat(type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic)
					.Where(info => info.HasAttribute<SerializeField>() || info.HasAttribute<ShtSerializeAttribute>()))
				.ToList();

			var baseType = type.BaseType;
			while (baseType != null) { // GetMembers отдается приватные поля только своего родительского типа
				all.AddRange(baseType
					.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic)
					.Where(memberInfo => memberInfo.HasAttribute<SerializeField>() || memberInfo.HasAttribute<ShtSerializeAttribute>())
					.Where(memberInfo => all.All(info => info.Name != memberInfo.Name)));
				baseType = baseType.BaseType;
			}

			var keys = new HashSet<string>();
			foreach (var memberInfo in all) {
				if ((memberInfo.MemberType & (MemberTypes.Property | MemberTypes.Field)) == 0) continue;

				if (memberInfo.HasAttribute<ShtIgnoreAttribute>()) continue;

				SheetRowProperty property = null;

				if (memberInfo is PropertyInfo propertyInfo && (propertyInfo.CanRead && propertyInfo.GetMethod.GetParameters().Length == 0) &&
					((propertyInfo.CanWrite && propertyInfo.SetMethod.GetParameters().Length == 1) || propertyInfo.HasAttribute<ShtSerializeAttribute>()))
					property = SheetRowProperty.Create(propertyInfo, memberInfo.GetCustomAttributes());
				if (memberInfo is FieldInfo fieldInfo && !fieldInfo.HasAttribute<CompilerGeneratedAttribute>())
					property = SheetRowProperty.Create(fieldInfo, memberInfo.GetCustomAttributes());

				if (property != null && CanSerializeType(property)) {
					if (keys.Contains(property.Name)) {
						Debug.LogError($"Multiple properties with the same name {property.Name} of type {type.Name}");
						continue;
					}

					keys.Add(property.Name);
					_properties.Add(property);
				}
			}
		}

		private bool CanSerializeType(SheetRowProperty property) {
			if (property.Type == TypeOf<HideFlags>.Raw) return false;
			return true;
		}

		public SheetRowProperty KeyProperty { get; private set; }

		public IEnumerable<SheetRowProperty> Properties => _properties;

		public SheetRowProperty FindProperty(string name) {
			name = ConfigUtils.PrettyName(name);
			return _properties.FirstOrDefault(s => s.Names.Any(ss => ss.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
		}
	}

	public class DynamicSheetTypeRowProperties : SheetTypeSchema {
		public DynamicSheetTypeRowProperties(IDynamicSheetRow dynamicRow, ISheetTypeSchema properties) {
			_properties.AddRange(properties.Properties);
			_properties.AddRange(dynamicRow.GetColumns());
			Prepare();
		}
	}

}