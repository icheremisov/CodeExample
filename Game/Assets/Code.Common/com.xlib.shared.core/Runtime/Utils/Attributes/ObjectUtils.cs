namespace XLib.Core.Utils.Attributes {

	public static class ObjectUtils {
		
		public static T Or<T>(this T value, T defaultValue) => value == null ? defaultValue : value;
	}

}