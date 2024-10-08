using System;
using System.Text;
using UnityEngine;

namespace XLib.Core.Utils.Attributes {

	[AttributeUsage(AttributeTargets.Field)]
	public class EnumFormatAttribute : Attribute {

		public string TextColor { get; }
		public string Alias { get; }
		public Color Color => ColorUtility.TryParseHtmlString(TextColor, out var color) ? color : throw new InvalidCastException();
		public int Size { get; }
		public bool Bold { get; }
		public bool Italics { get; }
		public string AsLogFormat() => AsLogFormat(Alias);
		public string AsLogFormat(string message) => RichLog.Default.Format(TextColor, message, RichLog.Default);

		private static StringBuilder _sb = new(1024);
		public string AsRichFormat() => AsRichFormat(Alias);

		public string AsRichFormat(string message) {
			lock (_sb) {
				try {
					_sb.Clear();
					if (Bold) _sb.Append("<b>");
					if (Italics) _sb.Append("<i>");
					if (!string.IsNullOrEmpty(TextColor)) _sb.Append($"<color={TextColor}>");
					if (Size > 0) _sb.Append($"<size={Size}>");
					_sb.Append(message);
					if (Size > 0) _sb.Append($"</size>");
					if (!string.IsNullOrEmpty(TextColor)) _sb.Append("</color>");
					if (Italics) _sb.Append("</i>");
					if (Bold) _sb.Append("</b>");
					return _sb.ToString();
				}
				finally {
					_sb.Clear();
				}
			}
		}

		public EnumFormatAttribute(string color, string alias, bool bold = false, bool italics = false, int size = -1) {
			TextColor = color;
			Alias = alias;
			Size = size;
			Bold = bold;
			Italics = italics;
		}
	}

	public static class EnumAliasExtension {

		public static Color AsColor<TEnum>(this TEnum flag, Color def) where TEnum : Enum => flag.GetAttributeOfType<EnumFormatAttribute>()?.Color ?? def;
		public static string AsAlias<TEnum>(this TEnum flag) where TEnum : Enum => flag.GetAttributeOfType<EnumFormatAttribute>()?.Alias ?? flag.ToString();
		public static string AsRichFormat<TEnum>(this TEnum flag, string message) where TEnum : Enum => flag.GetAttributeOfType<EnumFormatAttribute>()?.AsRichFormat(message) ?? message;
		public static string AsRichFormat<TEnum>(this TEnum flag) where TEnum : Enum => flag.GetAttributeOfType<EnumFormatAttribute>().AsRichFormat();
		public static string AsLogFormat<TEnum>(this TEnum flag, string message) where TEnum : Enum => flag.GetAttributeOfType<EnumFormatAttribute>()?.AsLogFormat(message) ?? message;
		public static string AsLogFormat<TEnum>(this TEnum flag) where TEnum : Enum => flag.GetAttributeOfType<EnumFormatAttribute>().AsLogFormat();

	}

}