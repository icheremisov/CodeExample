using System;
using Newtonsoft.Json;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.Core.Json.Net {

	public class Vector3NetConverter : JsonConverter {

		public override bool CanConvert(Type objectType) => objectType == TypeOf<Vector3>.Raw || objectType == TypeOf<Vector3?>.Raw;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (objectType == TypeOf<Vector3?>.Raw && reader.TokenType == JsonToken.Null) return null;

			try {
				if (reader.TokenType == JsonToken.StartArray) {
					var arrayVal = serializer.Deserialize<float[]>(reader);
					if (arrayVal?.Length != 3) throw new FormatException($"Wrong data for Vector3 type in a json, expected [x, y, z] but found  '{reader.Value}'");

					return new Vector3(arrayVal[0], arrayVal[1], arrayVal[2]);
				}

				throw new FormatException($"Wrong data for Vector3 type in a json, expected [x, y, z] but found '{reader.Value}'");
			}
			catch (Exception ex) {
				throw new FormatException($"Error parsing Vector3 from '{reader.Value}'", ex);
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (value == null) {
				writer.WriteNull();
				return;
			}

			var v = (Vector3)value;

			writer.WriteStartArray();
			writer.WriteValue(v.x);
			writer.WriteValue(v.y);
			writer.WriteValue(v.z);
			writer.WriteEndArray();
		}

	}

}