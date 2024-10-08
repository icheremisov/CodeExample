using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XLib.Unity.Utils;

namespace XLib.Localization.UI {

	internal static class LocalizationTextReplacer {

		// public static void ReplaceAllTextsWithLocTextInLoadScenes() {
		// 	var scene = SceneManager.GetActiveScene();
		// 	
		// 	EditorUtils.ChangeAllObjectTypeInScene<LocText, Text>(scene);
		// 	EditorUtils.ChangeAllObjectTypeInScene<TMPLocText, TextMeshProUGUI>(scene);
		// 	EditorUtils.ChangeAllObjectTypeInScene<TextMeshPro, LocTextMeshPro>(scene);
		// }	
		//
		// public static void ReplaceTextsWithLocTextInSelectedGameObjects() {
		// 	var objects = Selection.gameObjects;
		//
		// 	EditorUtils.ChangeAllObjectTypeInGameObject<LocText, Text>(objects);
		// 	EditorUtils.ChangeAllObjectTypeInGameObject<TMPLocText, TextMeshProUGUI>(objects);
		// 	EditorUtils.ChangeAllObjectTypeInGameObject<TextMeshPro, LocTextMeshPro>(objects);
		// }

		[MenuItem("Tools/Localization/Replace All Texts With LocText")]
		public static void ReplaceAllTextsWithLocText() {
			EditorUtils.ChangeAllObjectsType<LocText, Text>();
			EditorUtils.ChangeAllObjectsType<TMPLocText, TextMeshProUGUI>();
			EditorUtils.ChangeAllObjectsType<TextMeshPro, LocTextMeshPro>();
		}

	}

}