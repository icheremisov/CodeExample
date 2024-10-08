using System;

namespace XLib.UI.Types {

	public class ScreenParentSceneNameAttribute : Attribute {
		public string SceneName { get; }
		public ScreenParentSceneNameAttribute() { }
		public ScreenParentSceneNameAttribute(string sceneName) => SceneName = sceneName;
	}

}