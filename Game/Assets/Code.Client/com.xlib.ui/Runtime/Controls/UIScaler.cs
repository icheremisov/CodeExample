using UnityEngine;

namespace XLib.UI.Controls {

	[ExecuteInEditMode]
	public class UIScaler : MonoBehaviour {
		public static float GlobalUIScale = 1.0f;
		
		private void Awake() {
			transform.SetLocalScale(GlobalUIScale);
#if UNITY_EDITOR
			_lastScale = transform.localScale.x;
#endif			
		}
		
#if UNITY_EDITOR
		private float _lastScale;
		
		private void Update() {
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (_lastScale != GlobalUIScale) {
				_lastScale = GlobalUIScale;
				transform.SetLocalScale(GlobalUIScale);
			}
		}
#endif
	}
}