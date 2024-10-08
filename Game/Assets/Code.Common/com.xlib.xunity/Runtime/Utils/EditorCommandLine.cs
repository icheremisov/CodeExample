using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XLib.Unity.Utils {

	public static class EditorCommandLine {
		private static Dictionary<string, string> _args;

		public static IReadOnlyDictionary<string, string> Args {
			get {
				if (_args == null) ParseArgs();
				return _args;
			}
		}

		private static void ParseArgs() {
			_args = new(StringComparer.InvariantCultureIgnoreCase);

			var args = System.Environment.GetCommandLineArgs();

			foreach (var argSrc in args.Where(x => x.StartsWith("--"))) {
				var key = argSrc;
				while (key.StartsWith("-")) key = key[1..];

				var value = string.Empty;
				var separator = key.IndexOf('=');
				if (separator >= 0) {
					value = key[(separator + 1) ..];
					key = key[.. separator];
				}

				if (_args.ContainsKey(key)) throw new Exception($"Duplicated command line key: '{key}' ({argSrc})");

				_args.Add(key, value);
			}
		}

		public static void Set(string key) {
			if (_args == null) ParseArgs();
			_args[key] = string.Empty;
		}

		public static void Set(string key, string value) {
			if (_args == null) ParseArgs();
			_args[key] = value;
		}

		public static void Remove(string key) {
			if (_args == null) ParseArgs();
			_args.Remove(key);
		}

		public static bool Has(string key) {
			if (_args == null) ParseArgs();
			return _args.ContainsKey(key);
		}

		public static string GetValue(string key) {
			if (_args == null) ParseArgs();
			return _args.TryGetValue(key, out var value) ? value : null;
		}
	}

}