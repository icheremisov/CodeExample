using System;
using Newtonsoft.Json;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.Unity.Json.Net {

	public class QuaternionNetConverter : JsonConverter {

		public override bool CanConvert(Type objectType) => objectType == TypeOf<Quaternion>.Raw || objectType == TypeOf<Quaternion?>.Raw;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (objectType == TypeOf<Quaternion?>.Raw && reader.TokenType == JsonToken.Null) return null;

			try {
				if (reader.TokenType == JsonToken.StartArray) {
					var arrayVal = serializer.Deserialize<float[]>(reader);
					if (arrayVal?.Length != 3) throw new FormatException($"Wrong data for Quaternion type in a json, expected [y, p, r] but found  '{reader.Value}'");

					return Quaternion.Euler(arrayVal[0], arrayVal[1], arrayVal[2]);
				}

				throw new FormatException($"Wrong data for Quaternion type in a json, expected [y, p, r] but found '{reader.Value}'");
			}
			catch (Exception ex) {
				throw new FormatException($"Error parsing Quaternion from '{reader.Value}'", ex);
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (value == null) {
				writer.WriteNull();
				return;
			}

			var v = ((Quaternion)value).eulerAngles;
			writer.WriteStartArray();
			writer.WriteValue(v.x);
			writer.WriteValue(v.y);
			writer.WriteValue(v.z);
			writer.WriteEndArray();
		}

	}

}