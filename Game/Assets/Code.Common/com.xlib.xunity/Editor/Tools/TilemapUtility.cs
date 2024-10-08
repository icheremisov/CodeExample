using System.Linq;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace XLib.Unity.Tools {

	public static class TilemapUtility {

		[MenuItem("Tools/Tilemap/Clear selected Tilemap", true)]
		public static bool ClearTilemapCheck() {
			return Selection.gameObjects.All(x => x.GetComponent<Tilemap>() != null);
		}

		[MenuItem("Tools/Tilemap/Clear selected Tilemap", false)]
		public static void ClearTilemap() {
			if (!EditorUtility.DisplayDialog("Clear", "Clear all contents on selected tilemaps?", "Clear", "Cancel")) return;

			foreach (var t in Selection.gameObjects.Select(x => x.GetExistingComponent<Tilemap>())) {
				Undo.RecordObject(t.gameObject, "Clear tilemap");
				t.ClearAllTiles();
				EditorUtility.SetDirty(t);
			}
		}

	}

}