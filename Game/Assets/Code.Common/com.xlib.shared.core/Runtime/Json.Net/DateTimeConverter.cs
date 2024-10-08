using Newtonsoft.Json.Converters;

namespace XLib.Core.Runtime.Json.Net {

	public class DateTimeConverter : IsoDateTimeConverter {

		public DateTimeConverter(string format) {
			DateTimeFormat = format;
		}

	}

}