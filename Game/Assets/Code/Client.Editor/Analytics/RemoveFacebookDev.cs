#if UNITY_ANDROID && !FEATURE_PRODUCTION && !FEATURE_STAGING

using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Android;
using XLib.BuildSystem;

namespace Client.Analytics {

		// internal class RemoveFacebookDev : IPostGenerateGradleAndroidProject {
		//
		// 	public int callbackOrder => 999999;
		//
		// 	public void OnPostGenerateGradleAndroidProject(string path) {
		//
		// 		var fileName = GetManifestPath(path);
		//
		// 		var doc = XDocument.Load(fileName);
		// 		foreach (var element in doc.Descendants("provider")) {
		// 			//https://stackoverflow.com/questions/30723802/install-failed-conflicting-provider-with-facebook-sdk-when-i-build-multiple-prod
		// 			if (element.Attributes().Any(attr => attr.Value == "com.facebook.FacebookContentProvider")) {
		// 				element.ReplaceWith(new XComment(element.ToString()));
		// 				break;
		// 			}
		// 		}
		//
		// 		doc.Save(fileName);
		// 	}
		// 	
		// 	private string GetManifestPath(string basePath) => Path.Combine(basePath, "src", "main", "AndroidManifest.xml");
		// }

}

#endif