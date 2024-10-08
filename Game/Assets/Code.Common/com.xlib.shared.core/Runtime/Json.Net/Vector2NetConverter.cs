using System;
using Newtonsoft.Json;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.Core.Json.Net {

	public class Vector2NetConverter : JsonConverter {

		public override bool CanConvert(Type objectType) => objectType == TypeOf<Vector2>.Raw || objectType == TypeOf<Vector2?>.Raw;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (objectType == TypeOf<Vector2?>.Raw && reader.TokenType == JsonToken.Null) return null;

			try {
				if (reader.TokenType == JsonToken.StartArray) {
					var arrayVal = serializer.Deserialize<float[]>(reader);
					if (arrayVal?.Length != 2) throw new FormatException($"Wrong data for Vector2 type in a json, expected [x, y] but found  '{reader.Value}'");

					return new Vector2(arrayVal[0], arrayVal[1]);
				}

				throw new FormatException($"Wrong data for Vector2 type in a json, expected [x, y] but found '{reader.Value}'");
			}
			catch (Exception ex) {
				throw new FormatException($"Error parsing Vector2 from '{reader.Value}'", ex);
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (value == null) {
				writer.WriteNull();
				return;
			}

			var v = (Vector2)value;

			writer.WriteStartArray();
			writer.WriteValue(v.x);
			writer.WriteValue(v.y);
			writer.WriteEndArray();
		}

	}

}