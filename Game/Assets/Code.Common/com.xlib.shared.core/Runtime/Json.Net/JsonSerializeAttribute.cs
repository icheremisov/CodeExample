using System;
using XLib.Core.Parsers.Base;

namespace XLib.Core.Json.Net {

	/// <summary>
	///     serialize class with custom json serialization
	///     GUID: string with uid which never changes
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class JsonSerializeAttribute : Attribute {

		public JsonSerializeAttribute(string guid) {
			Guid = $"#{Base64Encoder.ToBase64String(new Guid(guid).ToByteArray())}";
		}

		public string Guid { get; }

	}

}