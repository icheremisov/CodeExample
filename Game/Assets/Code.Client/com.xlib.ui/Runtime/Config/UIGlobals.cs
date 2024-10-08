using Sirenix.OdinInspector;
using UnityEngine;
using XLib.UI.Contracts;
using XLib.UI.Controls;
using XLib.Unity.Utils;

namespace XLib.UI.Config {

	/// <summary>
	///     global singleton with settings for UI MonoBehaviours.
	/// </summary>
	[CreateAssetMenu(menuName = "Configs/UIGlobals", fileName = "UIGlobals", order = 0)]
	public class UIGlobals : ScriptableObject {
		public const string AssetName = "UIGlobals";

		[Title("Colors")]
		[SerializeField, Required] private Color _notEnoughColor = Color.red;
		[SerializeField, Required] private Color _enoughColor = Color.green;

		[Title("Scale")]
		[SerializeField, Range(0, 1.0f), Required] private float _scale189 = 1.0f;
		[SerializeField, Range(0, 1.0f), Required] private float _scale169 = 0.85f;

		public Color NotEnoughColor => _notEnoughColor;
		public Color EnoughColor => _enoughColor;

		public static UIGlobals S { get; set; }
		public void ApplyUIScale() {
			var aspectK = GfxUtils.AspectRatioK(GfxUtils.AspectRatio.Aspect_16_9, GfxUtils.AspectRatio.Aspect_18_9);
			UIScaler.GlobalUIScale = Mathf.Lerp(_scale169, _scale189, aspectK);
		}
	}

}