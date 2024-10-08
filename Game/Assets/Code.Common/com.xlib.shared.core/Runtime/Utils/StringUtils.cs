using System.Globalization;
using UnityEngine;

namespace XLib.Core.Utils {

	public static class StringUtils {
		public static string FloatToPercentString(this float dec) {
			return $"{Mathf.Round(dec * 100)}%";
		}
		
		public static string FloatCeilToPercentString(this float dec) {
			return $"{Mathf.Ceil(dec * 100)}%";
		}

		public static string FloatToString(this float value, int rounding = 100) => (Mathf.RoundToInt(value * rounding) / rounding).ToString(NumberFormatInfo.InvariantInfo);

		public static string CropByLength(this string str, int length) => str.Length <= length ? str : str[..length];
		public static string CropByLength(this string str, int minLength, int maxLength) {
			if(str.Length < minLength) return str;
			var index = str.LastIndexOfAny(new [] {' ', '\t', '\n'}, minLength, Mathf.Max(0, Mathf.Min(str.Length, maxLength) - minLength));
			return index == -1 ? str[..maxLength] : str[..index];
		}
	}

}