using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace XLib.BuildSystem.GameDefines {

	public partial class CustomDefineManager : EditorWindow {
		public static List<Directive> GetDirectivesFromXmlFile() {
			var directives = new List<Directive>();

			var path = GetXmlAssetPath();
			if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
				try {
					var serializer = new XmlSerializer(typeof(List<Directive>));
					using (TextReader reader = new StreamReader(GetXmlAssetPath())) {
						directives = (List<Directive>)serializer.Deserialize(reader);
					}
				}
				catch (Exception ex) {
					Debug.LogException(ex);
				}
			}

			return directives;
		}

		public static void SaveDataToXmlFile(List<Directive> directives) {
			Directory.CreateDirectory(Path.GetDirectoryName(GetXmlAssetPath()));

			var serializer = new XmlSerializer(typeof(List<Directive>));
			using (TextWriter writer = new StreamWriter(GetXmlAssetPath())) {
				serializer.Serialize(writer, directives);
			}

			AssetDatabase.Refresh();
		}

		private static string GetXmlAssetPath() {
			var assetFile = Resources.Load<TextAsset>("CustomDefineManagerData");

			return assetFile == null ? "Resources/CustomDefineManagerData.xml" : AssetDatabase.GetAssetPath(assetFile);
		}
	}

	[Serializable]
	public class CustomDefineManagerData {
		[FormerlySerializedAs("directives")] public List<Directive> _directives = new List<Directive>();
	}

}