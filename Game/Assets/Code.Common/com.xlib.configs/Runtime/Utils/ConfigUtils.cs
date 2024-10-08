using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace XLib.Configs.Utils {

	public static partial class ConfigUtils {
		public static string CalculateConfigHash(string configDirectory) => 
			CalculateHash(GetAllAssets(configDirectory));

		public static string[] GetAllAssets(string configDirectory) {
			var allAssets = Directory.EnumerateFiles(configDirectory, "*.asset", SearchOption.AllDirectories)
				.OrderBy(s => s, StringComparer.Ordinal).ToArray();
			return allAssets;
		}

		public static string CalculateHash(IEnumerable<string> files)
		{
			var list = files.ToList();
			list.Sort(string.CompareOrdinal);
			using var hasher = MD5.Create();
			foreach (var sourceBytes in list.Select(File.ReadAllBytes)) 
				hasher.TransformBlock(sourceBytes, 0, sourceBytes.Length, sourceBytes, 0);
			hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
			return BitConverter.ToString(hasher.Hash)
				.Replace("-", string.Empty)
				.ToLower();
		}
		
		private static readonly Regex _regex = new("\\B([A-Z]+)");
		public static string PrettyName(string name) {
			name = name.Split("_")
				.JoinToString(" ")
				.Split("  ")
				.JoinToString(" ");
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_regex.Replace(name, " $1")).Trim();
		}
	}

}