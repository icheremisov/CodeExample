using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using XLib.Core.Utils;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

[SuppressMessage("ReSharper", "CheckNamespace")]
public static class StringExtensions {

	/// <summary>
	///     return true, if string is null or empty
	/// </summary>
	[ContractAnnotation("s:null => true; s:notnull=>false")]
	public static bool IsNullOrEmpty([NotNullWhen(false)] this string s) => string.IsNullOrEmpty(s);

	/// <summary>
	///     return true, if string is not null or empty
	/// </summary>
	[ContractAnnotation("s:null => false; s:notnull=>true")]
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string s) => !string.IsNullOrEmpty(s);

	/// <summary>
	///     clamp string value to maximum length
	/// </summary>
	public static string Take(this string s, int maxLength) {
		if (s.IsNullOrEmpty()) return string.Empty;

		return s.Length > maxLength ? s[..maxLength] : s;
	}
	
	/// <summary>
	///		remove richtags (<color>, and etc.) from string
	/// </summary>
	/// <param name="text"></param>
	public static string RemoveRichTags(this string text) {
		var rich = new Regex(@"<[^>]*>");
		return rich.IsMatch (text) ? rich.Replace(text, string.Empty) : text;
	}

	/// <summary>
	///     clamp string value to maximum length with '...'
	/// </summary>
	public static string ClampWithEllipsis(this string s, int maxLength) {
		if (s.IsNullOrEmpty() || maxLength <= 0) return string.Empty;

		maxLength = Math.Min(s.Length, maxLength);

		if (maxLength <= 3) return s[..maxLength];

		return s.Length > maxLength ? $"{s[..(maxLength - 3)]}..." : s;
	}

	/// <summary>
	///     replace character in a string (slow)
	/// </summary>
	public static string ReplaceAt(this string input, int index, char newChar) {
		if (input == null) throw new ArgumentNullException(nameof(input));

		var chars = input.ToCharArray();
		chars[index] = newChar;
		return new string(chars);
	}

	public static string ReplaceAll(this string input, char from, char to = '\0') {
		if (input == null) throw new ArgumentNullException(nameof(input));
		return input.Split(from).JoinToString(to);
	}

	public static string ReplaceAll(this string input, string from, string to) {
		if (input == null) throw new ArgumentNullException(nameof(input));
		return input.Split(from).JoinToString(to);
	}

	public static string ReplaceAll(this string input, char[] from, string to) {
		if (input == null) throw new ArgumentNullException(nameof(input));
		return input.Split(from).JoinToString(to);
	}

	public static string RemoveAll(this string input, params string[] from) {
		if (input == null) throw new ArgumentNullException(nameof(input));
		return input.Split(from, StringSplitOptions.RemoveEmptyEntries)
			.JoinToString(string.Empty);
	}
	
	/// <summary>
	/// parse generic types from a string
	/// </summary>
	public static T Parse<T>(this string value) {

		if (value == null) throw new ArgumentNullException(nameof(value));
		
		if (TypeOf<T>.Raw == TypeOf<string>.Raw) return (T)(object)value;

		value = value.Trim();
		if (value.IsNullOrEmpty()) throw new ArgumentException("Expected non-empty string", nameof(value));
		
		if (TypeOf<T>.Raw.IsEnum) {
			return (T)Enums.ToEnum(TypeOf<T>.Raw, value);
		}

		if (TypeOf<T>.Raw == TypeOf<bool>.Raw) {
			var v = value.Trim().ToLowerInvariant();

			return v switch {
				"true" or "1" or "yes" or "y" or "+" or "x" => (T)(object)true,
				"false" or "0" or "no" or "n" or "-"        => (T)(object)false,
				_                                           => throw new ArgumentException($"Cannot convert '{value}' into a bool value")
			};
		}

		if (TypeOf<T>.Raw == TypeOf<float>.Raw) {
			value = value.Replace(',', '.');

			try {
				return (T)(object)float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
			}
			catch (Exception e) {
				throw new ArgumentException(e.Message);
			}
		}

		if (TypeOf<T>.Raw == TypeOf<double>.Raw) {
			value = value.Replace(',', '.');
			try {
				return (T)(object)double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
			}
			catch (Exception e) {
				throw new ArgumentException(e.Message);
			}
		}

		return (T)TypeDescriptor.GetConverter(TypeOf<T>.Raw).ConvertFromString(value);
	}

	/// <summary>
	/// parse generic types from a string
	/// </summary>
	public static bool TryParse<T>(this string value, out T result) {

		if (value == null) throw new ArgumentNullException(nameof(value));

		if (TypeOf<T>.Raw == TypeOf<string>.Raw) {
			result = (T)(object)value;
			return true;
		}

		value = value.Trim();
		if (value.IsNullOrEmpty()) {
			result = default;
			return false;
		}
		
		if (TypeOf<T>.Raw.IsEnum) {
			var res = Enums.TryToEnum(TypeOf<T>.Raw, value, out var resultObj);
			if (res && resultObj is T obj) {
				result = obj;
				return true;
			}
			result = default;
			return false;
		}

		if (TypeOf<T>.Raw == TypeOf<bool>.Raw) {
			var v = value.Trim().ToLowerInvariant();

			switch (v) {
				case "true" or "1" or "yes" or "y" or "+" or "x":
					result = (T)(object)true;
					return true;

				case "false" or "0" or "no" or "n" or "-":
					result = (T)(object)false;
					return true;

				default:
					result = default;
					return false;
			}
		}

		if (TypeOf<T>.Raw == TypeOf<float>.Raw) {
			value = value.Replace(',', '.');
			if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var resultObj)) {
				result = default;
				return false;
			}

			result = (T)(object)resultObj;
			return true;
		}

		if (TypeOf<T>.Raw == TypeOf<double>.Raw) {
			value = value.Replace(',', '.');
			if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var resultObj)) {
				result = default;
				return false;
			}

			result = (T)(object)resultObj;
			return true;
		}

		try {
			result = (T)TypeDescriptor.GetConverter(TypeOf<T>.Raw).ConvertFromString(value);
			return true;
		}
		catch (Exception) {
			result = default;
			return false;
		}
	}

	
	/// <summary>
	///     Tests whether specified string can be matched against provided pattern string. Pattern may contain single- and
	///     multiple-replacing
	///     wildcard characters.
	/// </summary>
	public static bool IsMatch(this string input, string pattern, char singleWildcard = '?', char multipleWildcard = '*') {

		var patternLength = pattern.Length;
		if (patternLength == 0) return input.Length == 0;
		
		var hasRepeatMultiWildcard = false;
		for (var i = 1; i < patternLength; i++) {
			if (pattern[i] == multipleWildcard && pattern[i - 1] == multipleWildcard) {
				hasRepeatMultiWildcard = true;
				break;
			}
		}

		if (hasRepeatMultiWildcard) {
			var sb = new StringBuilder(patternLength - 1);
			sb.Append(pattern[0]);
			var lastChar = pattern[0];
			
			for (var i = 1; i < patternLength; i++) {
				var ch = pattern[i]; 
				if (ch == multipleWildcard && lastChar == multipleWildcard) continue;
				sb.Append(ch);
				lastChar = ch;
			}

			pattern = sb.ToString();
			patternLength = pattern.Length;
		}
		
		var inputPosStack = new int[(input.Length + 1) * (patternLength + 1)]; // Stack containing input positions that should be tested for further matching
		var patternPosStack = new int[inputPosStack.Length]; // Stack containing pattern positions that should be tested for further matching
		var stackPos = -1; // Points to last occupied entry in stack; -1 indicates that stack is empty
		var pointTested =
			new bool[input.Length + 1, patternLength + 1]; // Each true value indicates that input position vs. pattern position has been tested

		var inputPos = 0; // Position in input matched up to the first multiple wildcard in pattern
		var patternPos = 0; // Position in pattern matched up to the first multiple wildcard in pattern

		// Match beginning of the string until first multiple wildcard in pattern
		while (inputPos < input.Length && patternPos < patternLength && pattern[patternPos] != multipleWildcard &&
			   (input[inputPos] == pattern[patternPos] || pattern[patternPos] == singleWildcard)) {
			inputPos++;
			patternPos++;
		}

		// Push this position to stack if it points to end of pattern or to a general wildcard
		if (patternPos == patternLength || pattern[patternPos] == multipleWildcard) {
			pointTested[inputPos, patternPos] = true;
			inputPosStack[++stackPos] = inputPos;
			patternPosStack[stackPos] = patternPos;
		}

		var matched = false;

		// Repeat matching until either string is matched against the pattern or no more parts remain on stack to test
		while (stackPos >= 0 && !matched) {
			inputPos = inputPosStack[stackPos]; // Pop input and pattern positions from stack
			patternPos = patternPosStack[stackPos--]; // Matching will succeed if rest of the input string matches rest of the pattern

			if (inputPos == input.Length && (patternPos == patternLength || (patternPos == patternLength - 1 && pattern[patternPos] == multipleWildcard)))
				matched = true; // Reached end of both pattern and input string, hence matching is successful
			else {
				// First character in next pattern block is guaranteed to be multiple wildcard
				// So skip it and search for all matches in value string until next multiple wildcard character is reached in pattern

				for (var curInputStart = inputPos; curInputStart < input.Length; curInputStart++) {
					var curInputPos = curInputStart;
					var curPatternPos = patternPos + 1;

					if (curPatternPos == patternLength) {
						// Pattern ends with multiple wildcard, hence rest of the input string is matched with that character
						curInputPos = input.Length;
					}
					else {
						while (curInputPos < input.Length && curPatternPos < patternLength && pattern[curPatternPos] != multipleWildcard &&
							   (input[curInputPos] == pattern[curPatternPos] || pattern[curPatternPos] == singleWildcard)) {
							curInputPos++;
							curPatternPos++;
						}
					}

					// If we have reached next multiple wildcard character in pattern without breaking the matching sequence, then we have another candidate for full match
					// This candidate should be pushed to stack for further processing
					// At the same time, pair (input position, pattern position) will be marked as tested, so that it will not be pushed to stack later again
					if (((curPatternPos == patternLength && curInputPos == input.Length) ||
							(curPatternPos < patternLength && pattern[curPatternPos] == multipleWildcard))
						&& !pointTested[curInputPos, curPatternPos]) {
						pointTested[curInputPos, curPatternPos] = true;
						inputPosStack[++stackPos] = curInputPos;
						patternPosStack[stackPos] = curPatternPos;
					}
				}
			}
		}

		return matched;
	}

	public static T IsNotNull<T>(this object obj, T notNull, T isNull = default) => 
		obj == null ? isNull : notNull;
}