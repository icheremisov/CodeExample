using System;
using Newtonsoft.Json;
using XLib.Core.CommonTypes;

namespace XLib.Core.Json.Net {

	public class TimestampConverter : JsonConverter<Timestamp> {

		protected override void Serialize(Timestamp value, JsonWriter writer, JsonSerializer serializer) => writer.WriteValue(value.Value);

		protected override Timestamp Deserialize(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (reader.TokenType != JsonToken.Integer) throw new FormatException($"Wrong data for {nameof(Timestamp)} type in a json, expected Integer but found '{reader.Value}'");

			return new Timestamp(Convert.ToInt32(reader.Value));
		}

	}

}