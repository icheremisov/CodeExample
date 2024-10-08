using System;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Client.Odin {

	public class ConfigsEditorWindow : OdinMenuEditorWindow {
		private const string AssetsConfigs = "Assets/Configs/";

		[MenuItem("Game/Configs/Editor", false, 100)]
		private static void OpenWindow() => GetWindow<ConfigsEditorWindow>().Show();

		private static OdinMenuStyle _windowStyle;

		protected override OdinMenuTree BuildMenuTree() {
			var tree = new OdinMenuTree { DefaultMenuStyle = GetStyle() };

			var allAssets = AssetDatabase.GetAllAssetPaths()
				.Where(x => x.StartsWith(AssetsConfigs))
				.OrderBy(x => x);

			tree.Config.DrawSearchToolbar = true;

			foreach (var path in allAssets) tree.AddAssetAtPath(path.Substring(AssetsConfigs.Length).Replace(".asset", string.Empty, StringComparison.OrdinalIgnoreCase), path);
			tree.EnumerateTree().AddThumbnailIcons();
			return tree;
		}

		private OdinMenuStyle GetStyle() {
			if (_windowStyle == null)
				_windowStyle = new() {
					Height = 30,
					Offset = 20.00f,
					IndentAmount = 15.00f,
					IconSize = 25.00f,
					IconOffset = 0.00f,
					NotSelectedIconAlpha = 0.34f,
					IconPadding = 2.00f,
					TriangleSize = 16.00f,
					TrianglePadding = 0.00f,
					AlignTriangleLeft = false,
					Borders = true,
					BorderPadding = 10.00f,
					BorderAlpha = 0.25f,
					SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f),
					SelectedColorLightSkin = new Color(0.243f, 0.490f, 0.900f, 1.000f)
				};

			return _windowStyle;
		}
	}

}