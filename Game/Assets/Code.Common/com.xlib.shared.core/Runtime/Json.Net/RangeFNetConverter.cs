using System;
using Newtonsoft.Json;
using UnityEngine;
using XLib.Core.CommonTypes;

namespace XLib.Core.Json.Net {

	public class RangeFNetConverter : JsonConverter<RangeF> {

		protected override void Serialize(RangeF range, JsonWriter writer, JsonSerializer serializer) {
			if (Mathf.Approximately(range.min, range.max))
				writer.WriteValue(range.min);
			else {
				writer.WriteStartArray();
				writer.WriteValue(range.min);
				writer.WriteValue(range.max);
				writer.WriteEndArray();
			}
		}

		protected override RangeF Deserialize(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			float from;
			float to;

			if (reader.TokenType == JsonToken.StartArray) {
				var arrayVal = serializer.Deserialize<float[]>(reader);
				if (!arrayVal.IsValidIndex(0)) throw new FormatException($"Wrong data for Range type in a json, expected Float or [min, max] but found '{reader.Value}'");

				from = arrayVal[0];
				to = arrayVal.GetOrDefault(1);
			}
			else if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float) {
				var parsedVal = Convert.ToSingle(reader.Value);
				from = to = parsedVal;
			}
			else
				throw new FormatException($"Wrong data for Range type in a json, expected Float or [min, max] but found '{reader.Value}'");

			return new RangeF(from, to);
		}

	}

}