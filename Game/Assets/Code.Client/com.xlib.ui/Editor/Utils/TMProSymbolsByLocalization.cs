using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
// using XLib.Localization.Internal.LanguageSource;

namespace XLib.UI.Utils {

	public static class TMProSymbolsByLocalization {

		private const string IgnoreSymbols = "";

		private static readonly string ForceRequiredSymbols =
			"∞1234567890,./%()+-ъёЪЁ?!" +
			"$¢£¤¥֏؋߾߿৲৳৻૱௹฿៛₠₡₢₣₤₥₦₧₨₩₪₫€₭₮₯₰₱₲₳₴₵₶₷₸₹₺₻₼₽₾₿꠸﷼﹩＄￠￡￥￦";

		static TMProSymbolsByLocalization() {
			var sb = new StringBuilder(1024);

			foreach (var i in Enumerable.Range('a', 'z' - 'a' + 1)) sb.Append((char)i);
			foreach (var i in Enumerable.Range('а', 'я' - 'а' + 1)) sb.Append((char)i);

			ForceRequiredSymbols += sb.ToString();
		}

		private static string FilePath => Path.Combine(Application.dataPath, "generatedFontSymbols.txt");

		
		// [MenuItem("Tools/Font/GenerateSymbolsByLocalization")]
		// public static void GenerateSymbolsByLocalization() {
		// 	var localizationFile = Resources.Load<LanguageSourceAsset>("I2Languages");
		// 	var source = localizationFile.SourceData;
 	//
		// 	var CSVstring = source.Export_CSV(null, ';');
		//
		// 	ProcessSymbols(ref CSVstring);
		// 	SaveFile(CSVstring);
		// 	Debug.Log($"Success GenerateSymbolsByLocalization: {FilePath}");
		// }

		private static void ProcessSymbols(ref string source) {
			var includedChars = new HashSet<char>();

			foreach (var ch in ForceRequiredSymbols.Concat(source)) AddSymbol(ch);

			void AddSymbol(char ch) {
				if (IgnoreSymbols.Contains(ch.ToString())) return;

				var chLow = char.ToLower(ch);
				var chHi = char.ToUpper(ch);

				if (!includedChars.Contains(chLow)) {
					includedChars.Add(chLow);

					if (chLow != chHi) includedChars.Add(chHi);
				}
			}

			source = new string(includedChars.ToArray());
		}

		private static void SaveFile(string symbols) {
			File.WriteAllText(FilePath, symbols);
		}

	}

}