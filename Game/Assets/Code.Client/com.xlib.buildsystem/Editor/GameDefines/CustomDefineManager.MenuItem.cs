using UnityEditor;

namespace XLib.BuildSystem.GameDefines {

	public partial class CustomDefineManager {
		[MenuItem("Build/Defines/Custom Define Manager")]
		static void Init() {
			// Get existing open window or if none, make a new one:
			var window = GetWindow<CustomDefineManager>("Custom Define Manager", true, typeof(SceneView));
			window.Show();
		}
	}

}