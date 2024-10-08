using UnityEditor;
using UnityEngine;

namespace XLib.BuildSystem {

	public class VersionWindow : EditorWindow {

		private VersionService.VersionStorage _storage;

		private void Awake() {
			_storage = VersionService.LoadInEditor(true);
		}

		private void OnGUI() {
			if (_storage == null) _storage = VersionService.LoadInEditor(true);
			
			GUILayout.Space(10);
			GUILayout.Label("Set version code for manual build");
			GUILayout.Space(10);
			
			_storage.versionCode = (short)EditorGUILayout.IntField("Bundle Version Code (int)", _storage.versionCode);

			if (GUILayout.Button("Save")) VersionService.SaveInEditor(_storage);
		}

		public static void ShowWindow() {
			GetWindow(typeof(VersionWindow), false, "Setup Version Code");
		}

	}

}