using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.UI.Controls {

	/// <summary>
	/// scale component to fill parent
	/// </summary>
	[ExecuteInEditMode]
	public class UIScaleToFitParent : MonoBehaviour {
		
		private static readonly Vector2 Center = new(0.5f, 0.5f);
		
		[SerializeField] private bool _preserveAspect = true;
		
		private void OnEnable() {
			UpdateScale();
		}

#if UNITY_EDITOR
		private void Update() {
			if (!Application.isPlaying) UpdateScale();
		}
#endif		

		[Button]
		private void UpdateScale() {
			var tm = (RectTransform)transform;
			var parent = (RectTransform)tm.parent;
			if (parent == null) return;
			
			tm.anchorMin = Center;
			tm.anchorMax = Center;
			tm.pivot = Center;
			tm.anchoredPosition = Vector2.zero;

			var size = tm.sizeDelta;
			var parentSize = parent.rect.size;
			if (size.x <= 0.001f || size.y <= 0.001f) return;
			if (parentSize.x <= 0.001f || parentSize.y <= 0.001f) return;

			var scale = (parentSize / size).ToXY0(1);

			if (_preserveAspect) {
				if (scale.x < scale.y) scale.y = scale.x;
				else if (scale.y < scale.x) scale.x = scale.y;
			}

			if (!scale.SameAs(tm.localScale)) tm.localScale = scale;
		}
	}

}