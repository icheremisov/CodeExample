using System.Text.RegularExpressions;
using UnityEngine;

namespace XLib.Core.Utils {

	public static class RichTag {

		public static string StripTags(string message) => Regex.Replace(message, "<.*?>", string.Empty);
		public static string StripEmoji(string message) => Regex.Replace(message, @"[\u2200-\uffff]", string.Empty);

#if UNITY3D

		public static string RelScale(float percent, object text) => $"<size={percent}%>{text}</size>";

		public static string FontSize(float fontSize, object text) => $"<size={fontSize}>{text}</size>";
		
		public static string Font(string fontName, object text) => $"<font={fontName}>{text}</font>";

		public static string VOffset(float offset, object text) => $"<voffset={offset}om>{text}</voffset>";

		public static string Style(string value) => $"<style={value}>";

		public static string Bold(string value) => $"<b>{value}</b>";

		public static string Italic(string value) => $"<i>{value}</i>";

		public static string Underline(string value) => $"<u>{value}</u>";

		public static string Strikethrough(string value) => $"<s>{value}</s>";

		public static string Colored(string htmlStringRgb, object text) {
			if (!htmlStringRgb.StartsWith('#')) htmlStringRgb = $"#{htmlStringRgb}"; 
			return $"<color={htmlStringRgb}>{text}</color>";
		}

		public static string Colored(Color c, string text) => $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{text}</color>";
		
		public static string ColoredWithAlpha(Color c, string text) => $"<color=#{ColorUtility.ToHtmlStringRGBA(c)}>{text}</color>";

		public static string Sprite(string value) => $"<sprite name=\"{value}\">";

#else
		public static string RelScale(float percent, object text) => $"{text}";

		public static string FontSize(float fontSize, object text) => $"{text}";

		public static string VOffset(float offset, object text) => $"{text}";

		public static string Style(object text) => $"{text}";

		public static string Bold(object text) =>  $"{text}";

		public static string Italic(object text) =>  $"{text}";

		public static string Underline(object text) =>  $"{text}";

		public static string Strikethrough(string value) => $"{value}";

		public static string Colored(string htmlStringRgb, object text) => $"{text}";

		public static string Colored(Color c, string text) => $"{text}";

		public static string ColoredWithAlpha(Color c, string text) => $"{text}";

		public static string Sprite(string text) => string.Empty;

#endif

	}

}