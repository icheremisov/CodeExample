using System;
using Newtonsoft.Json;
using Range = XLib.Core.CommonTypes.Range;

namespace XLib.Core.Json.Net {

	public class RangeNetConverter : JsonConverter<Range> {

		protected override void Serialize(Range range, JsonWriter writer, JsonSerializer serializer) {
			if (range.min == range.max)
				writer.WriteValue(range.min);
			else {
				writer.WriteStartArray();
				writer.WriteValue(range.min);
				writer.WriteValue(range.max);
				writer.WriteEndArray();
			}
		}

		protected override Range Deserialize(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			int from;
			int to;

			if (reader.TokenType == JsonToken.StartArray) {
				var arrayVal = serializer.Deserialize<int[]>(reader);
				if (!arrayVal.IsValidIndex(0)) throw new FormatException($"Wrong data for Range type in a json, expected Integer or [min, max] but found '{reader.Value}'");

				from = arrayVal[0];
				to = arrayVal.GetOrDefault(1);
			}
			else if (reader.TokenType == JsonToken.Integer) {
				var parsedVal = Convert.ToInt32(reader.Value);
				from = to = parsedVal;
			}
			else
				throw new FormatException($"Wrong data for Range type in a json, expected Integer or [min, max] but found '{reader.Value}'");

			return new Range(from, to);
		}

	}

}