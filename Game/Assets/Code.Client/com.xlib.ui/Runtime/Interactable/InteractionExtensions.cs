using XLib.UI.Interactable.Contracts;
using UnityEngine;

namespace XLib.UI.Interactable {

	internal static class InteractionExtensions {
		private static readonly Vector3[] WorldCorners = new Vector3[4];
		private static readonly Vector3[] ScreenCorners = new Vector3[4];
		
		public static Rect RectTransformToScreenSpace(this RectTransform transform, Camera cam) {
			transform.GetWorldCorners(WorldCorners);

			for (var i = 0; i < 4; i++) ScreenCorners[i] = cam.WorldToScreenPoint(WorldCorners[i]);

			return new Rect(ScreenCorners[0].x, ScreenCorners[0].y,
				ScreenCorners[2].x - ScreenCorners[0].x,
				ScreenCorners[2].y - ScreenCorners[0].y);
		}

		public static float IntersectsPercent(IInteractableView interactableView, IInteractableTargetView interactableTargetView) {
			var r1 = interactableView.ScreenRect;
			var r2 = interactableTargetView.ScreenRect;

			var intersectsArea = r1.Intersects(r2).Area();
			return Mathf.Max(intersectsArea / r1.Area(), intersectsArea / r2.Area());
		}

		private static float Area(this Rect rect) {
			return rect.width * rect.height;
		}

		private static Rect Intersects(this Rect r1, Rect r2) {
			var result = new Rect();

			if (!r2.Overlaps(r1)) return result;
			
			var x1 = Mathf.Min(r1.xMax, r2.xMax);
			var x2 = Mathf.Max(r1.xMin, r2.xMin);
			var y1 = Mathf.Min(r1.yMax, r2.yMax);
			var y2 = Mathf.Max(r1.yMin, r2.yMin);
			result.x = Mathf.Min(x1, x2);
			result.y = Mathf.Min(y1, y2);
			result.width = Mathf.Max(0.0f, x1 - x2);
			result.height = Mathf.Max(0.0f, y1 - y2);
 
			return result;

		}
	}

}