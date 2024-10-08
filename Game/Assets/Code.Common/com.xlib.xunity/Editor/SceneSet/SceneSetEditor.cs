using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLib.Unity.Core;
using XLib.Unity.Scene;

namespace XLib.Unity.SceneSet {

	[CustomEditor(typeof(Scene.SceneSet))]
	public class SceneSetEditor : Editor {

		[OnOpenAsset(1)]
		public static bool OpenAssetHandler(int instanceID, int line) {
			var obj = EditorUtility.InstanceIDToObject(instanceID);

			if (obj is Scene.SceneSet) {
				Open((Scene.SceneSet)obj, false);
				return true; // open handled
			}

			return false; // open not handled
		}

		[MenuItem("Assets/Create/SceneSet")]
		private static void CreateMultiScene() {
			var multiScene = New();

			// ProjectWindowUtil is not fully documented, but is the only way I could figure out
			// to get the Right Click > Create asset behavior to match what the
			// CreateAssetMenu attribute does. I couldn't use CreateAssetMenu because 
			// I needed to do some work to initialize the instance outside of the constructor.

			ProjectWindowUtil.CreateAsset(multiScene, "SceneSet.asset");
		}

		private static Scene.SceneSet New() {
			var conf = CreateInstance<Scene.SceneSet>();
			Update(conf);
			return conf;
		}

		private static void Update(Scene.SceneSet sceneSet) {
			sceneSet.SceneSetups = UpdateSceneSetups(EditorSceneManager.GetSceneManagerSetup());
			EditorUtility.SetDirty(sceneSet);
		}

		public static void Open(Scene.SceneSet sceneSet, bool additive) {
			var cancelled = !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
			if (cancelled) return;

			var setup = ToSceneSetups(sceneSet);
			if (additive) {
				setup.ForEach(x => x.isActive = false);
				setup = EditorSceneManager.GetSceneManagerSetup().Concat(setup).ToArray();
			}
			
			EditorSceneManager.RestoreSceneManagerSetup(setup);
		}

		private void Undo(string name) {
			UnityEditor.Undo.RecordObject(target, name);
			EditorUtility.SetDirty(target);
		}

		public override void OnInspectorGUI() {
			var multiScene = (Scene.SceneSet)target;

			using (new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("Open", GUILayout.ExpandWidth(false))) Open(multiScene, false);

				if (GUILayout.Button("Update", GUILayout.ExpandWidth(false))) {
					var confirm = EditorUtility.DisplayDialog("Update Existing Configuration?", "Are you sure you want to overwrite the existing scene configuration?", "Update",
						"Cancel");

					if (confirm) {
						Undo("Update Multiscene");
						Update(multiScene);
					}
				}
			}

			GUILayout.Label(string.Format("{0} Scenes", multiScene.SceneSetups.Length), EditorStyles.boldLabel);
			foreach (var sceneSetup in multiScene.SceneSetups) {
				using (var sceneSetupScope = new EditorGUILayout.VerticalScope()) {
					var scenePath = AssetDatabase.GUIDToAssetPath(sceneSetup.Guid);
					var filename = Path.GetFileNameWithoutExtension(scenePath);
					GUILayout.Label(filename, EditorStyles.boldLabel);
					GUILayout.Label(string.Format("path: {0}", scenePath));
					GUILayout.Label(string.Format("Name: {0}", sceneSetup.Name));
					GUILayout.Label(string.Format("Active: {0}", sceneSetup.IsActive ? "Yes" : "No"));
					GUILayout.Label(string.Format("Loaded: {0}", sceneSetup.IsLoaded ? "Yes" : "No"));
					GUILayout.Space(10);
				}
			}
		}

		private static GuidSceneSetup[] UpdateSceneSetups(SceneSetup[] newSceneSetups) {
			var ss = new GuidSceneSetup[newSceneSetups.Length];
			for (var i = 0; i < newSceneSetups.Length; i++) {
				var scene = newSceneSetups[i];
				ss[i] = new GuidSceneSetup(SceneManager.GetSceneByPath(scene.path).name, AssetDatabase.AssetPathToGUID(scene.path), scene.isActive, scene.isLoaded);
			}

			AddNewScenesToBuildSettings(newSceneSetups);
			return ss;
		}

		private static void AddNewScenesToBuildSettings(SceneSetup[] newSceneSetups) {
			var oldScenes = EditorBuildSettings.scenes;

			var loadedScenes = newSceneSetups.Where(x => x.isLoaded);

			var newScenes = oldScenes.ToList();

			foreach (var loadedScene in loadedScenes)
				if (oldScenes.All(x => x.path != loadedScene.path))
					newScenes.Add(new EditorBuildSettingsScene(loadedScene.path, true));

			EditorBuildSettings.scenes = newScenes.ToArray();
		}

		private static SceneSetup[] ToSceneSetups(Scene.SceneSet sceneSet) =>
			sceneSet.SceneSetups
				.Select(ToSceneSetup)
				.Where(IsSceneValid)
				.ToArray();

		private static bool IsSceneValid(SceneSetup arg) {
			if (arg.path.IsNullOrEmpty()) return false;

			if (!File.Exists(arg.path)) {
				Debug.LogError($"Scene not found: {arg.path}!");
				return false;
			}

			return true;
		}

		private static SceneSetup ToSceneSetup(GuidSceneSetup guidSceneSetup) {
			var sceneSetup = new SceneSetup { path = AssetDatabase.GUIDToAssetPath(guidSceneSetup.Guid), isActive = guidSceneSetup.IsActive, isLoaded = guidSceneSetup.IsLoaded };
			return sceneSetup;
		}

	}

}