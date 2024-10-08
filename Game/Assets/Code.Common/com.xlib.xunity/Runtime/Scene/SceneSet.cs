using System;
using UnityEngine;

namespace XLib.Unity.Scene {

	[Serializable]
	public class SceneSet : ScriptableObject {

		public GuidSceneSetup[] SceneSetups;

	}

	// MultiScene's SceneSetup uses an AssetDatabase GUID instead of a Scene path.
	[Serializable]
	public struct GuidSceneSetup {

		public string Name;
		public string Guid;
		public bool IsActive;
		public bool IsLoaded;

		public GuidSceneSetup(string name, string guid, bool isActive, bool isLoaded) {
			Name = name;
			Guid = guid;
			IsActive = isActive;
			IsLoaded = isLoaded;
		}

	}

}