using System;

namespace XLib.Core.Runtime.Extensions {

	public static class GuidExtensions {
		public static string ToShortString(this Guid guid) =>
			Convert.ToBase64String(guid.ToByteArray())[..22]
				.Replace("/", "_")
				.Replace("+", "-");

		public static bool TryParseShortGuid(this string shortGuid, out Guid guid) {
			try {
				var guidString = Convert.FromBase64String(shortGuid.Replace("_", "/").Replace("-", "+") + "==");
				if (guidString.Length == 16) {
					guid = new Guid(guidString);
					return true;
				}
			}
			catch {
				// ignored
			}

			guid = Guid.Empty;
			return false;
		}
	}

}