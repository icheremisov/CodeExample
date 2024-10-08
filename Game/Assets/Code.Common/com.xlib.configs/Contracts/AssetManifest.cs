using UnityEngine;

namespace XLib.Configs.Contracts {

	public abstract class AssetManifest : ScriptableObject {
	}

#if UNITY_EDITOR
	public interface IEditorAssetManifest {
		void EditorInitialize();
	}
#endif

}