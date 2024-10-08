using System;
using Newtonsoft.Json;
using XLib.Core.CommonTypes;

namespace XLib.Core.Json.Net {

	public class DurationConverter : JsonConverter<Duration> {

		protected override void Serialize(Duration value, JsonWriter writer, JsonSerializer serializer) {
			writer.WriteValue(value.Value);
		}

		protected override Duration Deserialize(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (reader.TokenType != JsonToken.Integer) throw new FormatException($"Wrong data for {nameof(Duration)} type in a json, expected Integer but found '{reader.Value}'");

			return new Duration(Convert.ToInt32(reader.Value));
		}

	}

}