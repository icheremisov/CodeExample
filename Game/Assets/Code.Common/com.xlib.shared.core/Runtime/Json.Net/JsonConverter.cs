using System;
using Newtonsoft.Json;
using XLib.Core.Utils;

namespace XLib.Core.Json.Net {

	public abstract class JsonConverter<T> : JsonConverter {

		public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => Serialize((T)value, writer, serializer);

		public sealed override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			try {
				return Deserialize(reader, objectType, existingValue, serializer);
			}
			catch (Exception ex) {
				throw new FormatException($"{GetType().Name}: Error parsing {typeof(T).Name} from '{reader.Value}' {ex.Message} {ex.StackTrace}", ex);
			}
		}

		public override bool CanConvert(Type objectType) => objectType == typeof(T);

		protected abstract void Serialize(T value, JsonWriter writer, JsonSerializer serializer);
		protected abstract T Deserialize(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer);

	}

}