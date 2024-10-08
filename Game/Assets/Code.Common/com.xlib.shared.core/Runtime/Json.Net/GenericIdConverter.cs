using System;
using System.Reflection;
using Newtonsoft.Json;
using XLib.Core.Utils;

namespace XLib.Core.Json.Net {

	/// <summary>
	///     serialize ID to Json
	///     T must have public T(string id) ctor and ToString() for serialization
	/// </summary>
	public class GenericIdConverter<T> : JsonConverter
		where T : struct {

		private static readonly ConstructorInfo CreateCtor = TypeOf<T>.Raw.GetConstructor(new[] { typeof(string) });

		public override bool CanConvert(Type objectType) => objectType == TypeOf<T>.Raw;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (reader.TokenType == JsonToken.Null) return default(T);

			if (reader.TokenType != JsonToken.String)
				throw new JsonSerializationException($"Expected {JsonToken.String} but found {reader.TokenType} while reading {objectType.FullName}");
			var str = (string)reader.Value;
			return !str.IsNullOrEmpty() ? CreateCtor?.Invoke(new object[] { str }) : default(T);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var v = (T)value;
			writer.WriteValue(v.ToString());
		}

	}

}