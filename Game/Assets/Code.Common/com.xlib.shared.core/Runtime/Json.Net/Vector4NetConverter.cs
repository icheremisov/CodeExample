using System;
using Newtonsoft.Json;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.Core.Json.Net {

	public class Vector4NetConverter : JsonConverter {

		public override bool CanConvert(Type objectType) => objectType == TypeOf<Vector4>.Raw || objectType == TypeOf<Vector4?>.Raw;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (objectType == TypeOf<Vector4?>.Raw && reader.TokenType == JsonToken.Null) return null;

			try {
				if (reader.TokenType == JsonToken.StartArray) {
					var arrayVal = serializer.Deserialize<float[]>(reader);
					if (arrayVal?.Length != 4) throw new FormatException($"Wrong data for Vector4 type in a json, expected [x, y, z, w] but found  '{reader.Value}'");

					return new Vector4(arrayVal[0], arrayVal[1], arrayVal[2], arrayVal[3]);
				}

				throw new FormatException($"Wrong data for Vector4 type in a json, expected [x, y, z, w] but found '{reader.Value}'");
			}
			catch (Exception ex) {
				throw new FormatException($"Error parsing Vector4 from '{reader.Value}'", ex);
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (value == null) {
				writer.WriteNull();
				return;
			}

			var v = (Vector4)value;

			writer.WriteStartArray();
			writer.WriteValue(v.x);
			writer.WriteValue(v.y);
			writer.WriteValue(v.z);
			writer.WriteValue(v.w);
			writer.WriteEndArray();
		}

	}

}