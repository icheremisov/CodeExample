using System.IO;
using UnityEditor;
using UnityEngine;

namespace XLib.Unity.Editors {

	public static class CreateMaterialFromTexture {

		[MenuItem("Assets/Create Materials")]
		private static void CreateMaterials() {
			foreach (var o in Selection.objects) {
				if (o.GetType() != typeof(Texture2D)) {
					Debug.LogError("This isn't a texture: " + o);
					continue;
				}

				Debug.Log("Creating material from: " + o);

				var tex = (Texture2D)o;

				var material = new Material(Shader.Find("Base/Lit")) { mainTexture = tex };

				var savePath = AssetDatabase.GetAssetPath(tex);
				savePath = Path.GetDirectoryName(savePath);

				var newAssetName = Path.Combine(savePath, $"{tex.name}.mat");

				if (AssetDatabase.LoadAssetAtPath<Object>(newAssetName) != null) continue;

				AssetDatabase.CreateAsset(material, newAssetName);
				AssetDatabase.SaveAssets();
			}

			Debug.Log("Done!");
		}

	}

}