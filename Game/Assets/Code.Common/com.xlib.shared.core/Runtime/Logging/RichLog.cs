using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using XLib.Core.Utils;
using Object = UnityEngine.Object;
// ReSharper disable once CheckNamespace
public static class RichLog {

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum Color {

		green,
		yellow,
		red,
		blue,
		magenta,
		cyan,
		white,
		darkred,
		darkgreen,
		darkmagenta,
		darkcyan,
		// ReSharper disable once IdentifierTypo
		darkyellow,
		darkblue,
		gray,
	}

	public interface IRichFormatter : IFormatProvider, ICustomFormatter { }

	public static readonly IRichFormatter Default = new UnityRichLog();

	public class UnityRichLog : IRichFormatter {

		private static readonly IReadOnlyDictionary<string, uint> Foreground = new Dictionary<string, uint>() {
			{ Color.darkred.ToString(), 0x800000 },
			{ Color.darkgreen.ToString(), 0x008000 },
			{ Color.darkyellow.ToString(), 0x808000 },
			{ Color.darkblue.ToString(), 0x000080 },
			{ Color.darkmagenta.ToString(), 0x800080 },
			{ Color.darkcyan.ToString(), 0x008080 },
			{ Color.gray.ToString(), 0xC0C0C0 },
			{ Color.red.ToString(), 0xFF0000 },
			{ Color.green.ToString(), 0x00FF00 },
			{ Color.yellow.ToString(), 0xFFFF00 },
			{ Color.blue.ToString(), 0x0000FF },
			{ Color.magenta.ToString(), 0xFF00FF },
			{ Color.cyan.ToString(), 0x00FFFF },
			{ Color.white.ToString(), 0xFFFFFF },
		};

		object IFormatProvider.GetFormat(Type formatType) => TypeOf<ICustomFormatter>.Raw == formatType ? this : null;

		string ICustomFormatter.Format(string format, object arg, IFormatProvider formatProvider) {
			if (format == null) return arg?.ToString();

			if (Foreground.TryGetValue(format, out var color)) return $"<color=#{color:X6}>{arg}</color>";

			format = $"{{0:{format}}}";
			return string.Format(format, arg);
		}

	}

	
	public class ConsoleRichLog : IRichFormatter {

		private static readonly string _defaultForeground = "\x1B[39m\x1B[22m\x1B[0m";

		private static readonly IReadOnlyDictionary<string, string> Foreground = new Dictionary<string, string>() {
			{ Color.darkred.ToString(), "\x1B[31m" },
			{ Color.darkgreen.ToString(), "\x1B[32m" },
			{ Color.darkyellow.ToString(), "\x1B[33m" },
			{ Color.darkblue.ToString(), "\x1B[34m" },
			{ Color.darkmagenta.ToString(), "\x1B[35m" },
			{ Color.darkcyan.ToString(), "\x1B[36m" },
			{ Color.gray.ToString(), "\x1B[37m" },
			{ Color.red.ToString(), "\x1B[1m\x1B[31m" },
			{ Color.green.ToString(), "\x1B[1m\x1B[32m" },
			{ Color.yellow.ToString(), "\x1B[1m\x1B[33m" },
			{ Color.blue.ToString(), "\x1B[1m\x1B[34m" },
			{ Color.magenta.ToString(), "\x1B[1m\x1B[35m" },
			{ Color.cyan.ToString(), "\x1B[1m\x1B[36m" },
			{ Color.white.ToString(), "\x1B[1m\x1B[37m" },
		};

		object IFormatProvider.GetFormat(Type formatType) => TypeOf<ICustomFormatter>.Raw == formatType ? this : null;

		string ICustomFormatter.Format(string format, object arg, IFormatProvider formatProvider) {
			if (format == null) return arg?.ToString();

			if (Foreground.TryGetValue(format, out var color)) return color + arg + _defaultForeground;

			format = $"{{0:{format}}}";
			return string.Format(format, arg);
		}

	}

	public class NoColorLog : IRichFormatter {

		private HashSet<string> _values;
		public NoColorLog() => _values = new HashSet<string>(Enum.GetNames(typeof(Color)));
		object IFormatProvider.GetFormat(Type formatType) => TypeOf<ICustomFormatter>.Raw == formatType ? this : null;

		string ICustomFormatter.Format(string format, object arg, IFormatProvider formatProvider) {
			if (format == null) return arg?.ToString();
			if (_values.Contains(format)) return arg.ToString();
			format = $"{{0:{format}}}";
			return string.Format(format, arg);
		}

	}

	public static void Log(FormattableString message, Object context = null) => Debug.Log(message.ToString(Default), context);
	public static void LogWarning(FormattableString message, Object context = null) => Debug.LogWarning(message.ToString(Default), context);
	public static void LogError(FormattableString message, Object context = null) => Debug.LogError(message.ToString(Default), context);
	public static string Format(FormattableString message) => message.ToString(Default);
	public static string WithColor(this string message, Color color) => Default.Format(color.ToString(), message, null);

}