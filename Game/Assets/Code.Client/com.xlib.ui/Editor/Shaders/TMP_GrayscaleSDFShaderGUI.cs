using TMPro.EditorUtilities;
using UnityEditor;

namespace XLib.UI.Shaders {

	// ReSharper disable once InconsistentNaming
	// ReSharper disable once UnusedType.Global
	public class TMP_GrayscaleSDFShaderGUI : TMP_SDFShaderGUI {

		private static bool s_Grayscale = true;

		protected override void DoGUI() {
			s_Grayscale = BeginPanel("Grayscale", s_Grayscale);
			if (s_Grayscale) DoGrayscalePanel();

			EndPanel();

			base.DoGUI();
		}

		private void DoGrayscalePanel() {
			EditorGUI.indentLevel += 1;
			DoColor("_Color", "Tint");
			DoSlider("_Saturation", "Saturation");

			EditorGUI.indentLevel -= 1;
			EditorGUILayout.Space();
		}

	}

}