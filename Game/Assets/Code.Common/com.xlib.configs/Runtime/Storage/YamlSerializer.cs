#if !UNITY3D
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;
using XLib.Core.Collections;
using XLib.Core.Reflection;
using YamlDotNet.RepresentationModel;

namespace XLib.Configs.Storage {

	public class YamlSerializer {

		private readonly IYamlEntryReference _reference;
		private readonly IUnityLogger _logger;

		private readonly Dictionary<Type, Func<string, object>> _parsers = new(64) { };
		private readonly Dictionary<Type, OrderedDictionary<string, FieldInfo>> _types = new();
		private string _currentGUID;
		private Dictionary<long,YamlFileReferenceEntry> _currentInternalReferences;

		public YamlSerializer(IYamlEntryReference reference, IUnityLogger logger) {
			_reference = reference;
			_logger = logger;
			RegisterParser(byte.Parse);
			RegisterParser(sbyte.Parse);
			RegisterParser(short.Parse);
			RegisterParser(ushort.Parse);
			RegisterParser(int.Parse);
			RegisterParser(uint.Parse);
			RegisterParser(long.Parse);
			RegisterParser(ulong.Parse);
			RegisterParser(v => (char)ushort.Parse(v));
			RegisterParser(v => v is "1" or "01");
			RegisterParser(v => v);
			RegisterParser(v => float.Parse(v, CultureInfo.InvariantCulture));
			RegisterParser(v => double.Parse(v, CultureInfo.InvariantCulture));
		}

		private void RegisterParser<T>(Func<string, T> func) {
			_parsers.Add(typeof(T), v => (object)func(v));
			_parsers.Add(typeof(T[]), StringToArray<T>);
			_parsers.Add(typeof(List<>).MakeGenericType(typeof(T)), v => Activator.CreateInstance(typeof(List<T>), StringToArray<T>(v)));
		}

		private void RegisterEnumArrayParser(Type enumType) {
			IList StringToEnumArray(string x) {
				var intArr = StringToArray<int>(x);
				var enumArr = Array.CreateInstance(enumType, intArr.Length) as IList;
				for (var i = 0; i < intArr.Length; i++)
					enumArr[i] =
						Enum.ToObject(enumType, intArr[i]);
				return enumArr;
			}

			_parsers[enumType.MakeArrayType()] = StringToEnumArray;
			_parsers[typeof(List<>).MakeGenericType(enumType)] =
				x => Activator.CreateInstance(typeof(List<>).MakeGenericType(enumType), StringToEnumArray(x));
		}

		private T[] StringToArray<T>(string v) {
			var size = Marshal.SizeOf<T>();
			var byteCount = v.Length / 2;
			var count = byteCount / size;
			var bytes = new byte[byteCount];
			var arr = new T[count];
			var stringIdx = 0;
			for (var i = 0; i < byteCount; i++) {
				char c1 = v[stringIdx++], c2 = v[stringIdx++];
				var b = (c1 >= 'a' ? c1 - ('a' - 0xA) : c1 - '0') << 4 | (c2 >= 'a' ? c2 - ('a' - 0xA) : c2 - '0');
				bytes[i] = (byte)b;
			}

			Buffer.BlockCopy(bytes, 0, arr, 0, byteCount);
			return arr;
		}

		private object DeserializeScalar(YamlScalarNode scalar, Type type) {
			if (type.IsEnum) return Enum.ToObject(type, long.Parse(scalar.Value));
			if (_parsers.ContainsKey(type)) return _parsers[type](scalar.Value);

			Type elementType = null;

			if (type.IsArray)
				elementType = type.GetElementType();
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
				elementType = type.GetGenericArguments()[0];
			else
				throw new ConfigParsingException("Unknown scalar type");

			if (elementType.IsEnum && elementType.GetEnumUnderlyingType() == typeof(int))
				RegisterEnumArrayParser(elementType);
			else
				throw new ConfigParsingException("Scalar type is not supported");

			return _parsers[type](scalar.Value);
		}

		private object DeserializeObject(YamlMappingNode node, Type type, bool isReference) {
			if (isReference) {
				var rid = long.Parse((string)node["rid"]);
				return _currentInternalReferences[rid].Obj;
			}
			else {
				var obj = Activator.CreateInstance(type);
				DeserializeObjectInPlace(node, obj, type);
				return obj;
			}
		}

		private object DeserializeList(YamlSequenceNode sequence, Type type, bool isReference) {
			var length = sequence.Children.Count;
			var fieldElementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
			var arr = Array.CreateInstance(fieldElementType, length);
			for (var i = 0; i < length; i++) {
				try {
					var value = Deserialize(sequence[i], fieldElementType, isReference);
					arr.SetValue(value, i);
				}
				catch (Exception e) {
					ProcessException(e, $"[{i}]");
					throw;
				}
			}

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return Activator.CreateInstance(type, arr);
			return arr;
		}

		private void ProcessException(Exception ex, string pathSegment) {
			if (ex is ConfigParsingException cex) cex.Path = string.IsNullOrEmpty(cex.Path) ? pathSegment : $"{pathSegment}::{cex.Path}";
		}

		private object Deserialize(YamlNode node, Type type, bool isReference) {
			return node switch {
				YamlScalarNode scalar => DeserializeScalar(scalar, type),
				YamlSequenceNode arr  => DeserializeList(arr, type, isReference),
				YamlMappingNode obj   => type.IsSubclassOf(typeof(ScriptableObject)) ? GetReferenceObject(obj) : DeserializeObject(obj, type, isReference),
				_                     => null
			};
		}

		private object GetReferenceObject(YamlMappingNode node) => 
			_reference.GetReference(node, _currentGUID)?.Obj;

		public object DeserializeEntry(YamlFileEntry entry) {
			try {
				var obj = entry.Obj;
				if (obj == null) return null;
				_currentGUID = entry.GUID;
				_currentInternalReferences = entry.References;
				DeserializeObjectInternalReferences(entry);

				_logger.LogVerbose($"Deserializing {obj.GetType().Name} at {entry.FileName}");
				DeserializeObjectInPlace(entry.YamlNode["MonoBehaviour"] as YamlMappingNode, obj, obj.GetType());
				return obj;
			}
			catch (Exception e) {
				ProcessException(e, entry.FileName);
				Debug.LogError($"Deserializing {entry.Obj.GetType().Name} at {entry.FileName} => {e}");
				throw;
			}
		}

		private void DeserializeObjectInternalReferences(YamlFileEntry entry) {
			if (entry.References == null) return;
			foreach (var reference in entry.References.Values) {
				if (reference.YamlNode is YamlMappingNode node)
					DeserializeObjectInPlace(node, reference.Obj, reference.Type);
				else if (reference.YamlNode is YamlSequenceNode sequenceNode) {
					if (sequenceNode.Children.Count > 0) {
						throw new NotSupportedException("Not support reference sequence object");
					}
				} else if (reference.YamlNode is YamlScalarNode scalarNode) {
					if (scalarNode.Value.Length > 0)
						throw new NotSupportedException("Not support reference scalar object");
				}
			}
		}

		private void DeserializeObjectInPlace(YamlMappingNode node, object target, Type type) {
			var map = GetObjectSerializationMap(type);
			foreach (var kv in node) {
				var key = ((YamlScalarNode)kv.Key).Value;
				try {
					if (map.TryGetValue(key, out var field)) {
						field.SetValue(target, Deserialize(kv.Value, field.FieldType, field.GetAttribute<SerializeReference>() != null));
					}
				}
				catch (Exception e) {
					ProcessException(e, key);
					throw;
				}
			}

			foreach ((_, var field) in map) {
				var t = field.FieldType;
				if ((!t.IsArray && !t.IsGenericList()) || field.GetValue(target) != null) continue;
				field.SetValue(target, t.IsArray ? Array.CreateInstance(t.GetElementType(), 0) : Activator.CreateInstance(t));
			}
			
			// if (target is IValidateConfig validate && !validate.Validate()) throw new ConfigParsingException("Object validation failed.");

			if (target is ISerializationCallbackReceiver receiver) receiver.OnAfterDeserialize();
		}

		private OrderedDictionary<string, FieldInfo> GetObjectSerializationMap(Type type) {
			if (_types.TryGetValue(type, out var map)) return map;
			_types[type] = map = new OrderedDictionary<string, FieldInfo>();
			
			foreach (var field in type.GetInstanceFields()) {
				if (!field.IsPublic && !(field.HasAttribute<SerializeField>() || field.HasAttribute<SerializeReference>())) continue;
				
				map[field.Name] = field;
				foreach (var formerlySerializedAsAttribute in field.GetCustomAttributes<FormerlySerializedAsAttribute>()) map[formerlySerializedAsAttribute.serializedAs] = field;
			}

			return map;
		}

	}

}
#endif