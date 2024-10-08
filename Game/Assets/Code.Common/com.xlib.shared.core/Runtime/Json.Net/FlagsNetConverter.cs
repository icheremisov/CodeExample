using System;
using Newtonsoft.Json;
using XLib.Core.Reflection;

namespace XLib.Core.Json.Net {

	public class FlagsNetConverter : JsonConverter {

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (reader.TokenType != JsonToken.StartArray) throw new Exception($"StartArray required but found {reader.TokenType}");

			long rawValue = 0;

			while (reader.TokenType != JsonToken.EndArray) {
				var item = reader.ReadAsString();

				try {
					var v = Enum.Parse(objectType, item);
					rawValue |= Convert.ToInt64(v);
				}
				catch (Exception) {
					// ignore
				}
			}

			return Enum.ToObject(objectType, rawValue);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var rawValue = Convert.ToInt64(value);

			writer.WriteStartArray();

			foreach (var v in Enum.GetValues(value.GetType())) {
				var rawItem = Convert.ToInt64(v);
				if ((rawValue & rawItem) == rawItem) writer.WriteValue(v.ToString());
			}

			writer.WriteEndArray();
		}

		public override bool CanConvert(Type objectType) => objectType.IsEnum && objectType.HasAttribute<FlagsAttribute>();

	}

}