using System;
using Newtonsoft.Json;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.Unity.Json.Net {

	public class ColorNetConverter : JsonConverter {

		public override bool CanConvert(Type objectType) => objectType == TypeOf<Color>.Raw || objectType == TypeOf<Color?>.Raw;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (objectType == TypeOf<Color?>.Raw && reader.TokenType == JsonToken.Null) return null;

			try {
				if (reader.TokenType == JsonToken.StartArray) {
					var arrayVal = serializer.Deserialize<byte[]>(reader);
					if (arrayVal?.Length == 4) return (Color)new Color32(arrayVal[0], arrayVal[1], arrayVal[2], arrayVal[3]);
				}
				else if (ColorUtility.TryParseHtmlString((string)reader.Value, out var result)) return result;

				throw new FormatException("Wrong data for Color type in a json, expected [r, g, b, a] or '#RRGGBB' or '#RRGGBBAA'");
			}
			catch (Exception ex) {
				throw new FormatException($"Error parsing Color from '{reader.Value}'", ex);
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (value == null) {
				writer.WriteNull();
				return;
			}

			var v = (Color32)(Color)value;

			writer.WriteStartArray();
			writer.WriteValue(v.r);
			writer.WriteValue(v.g);
			writer.WriteValue(v.b);
			writer.WriteValue(v.a);
			writer.WriteEndArray();
		}

	}

}