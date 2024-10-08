using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.UI.Utils {

	public static class HandlerUtils {
		private static RaycastHit2D[] _hit2DCache = new RaycastHit2D[16];
		private static RaycastHit[] _hitCache = new RaycastHit[16];
		public static T Raycast<T>(Ray ray, Func<T, bool> filter = null) where T : class {
			var count = Physics2D.GetRayIntersectionNonAlloc(ray, _hit2DCache);
			var minDistance = float.MaxValue;
			T result = default;
			for (var i = 0; i < count; ++i) {
				var hit2D = _hit2DCache[i];
				var target = hit2D.collider.GetComponentInParent<T>();
				if (target == null) continue;
				if (hit2D.distance < minDistance && (filter == null || filter.Invoke(target))) {
					minDistance = hit2D.distance;
					result = target;
				}
			}

			count = Physics.RaycastNonAlloc(ray, _hitCache);
			for (var i = 0; i < count; ++i) {
				var hit = _hitCache[i];
				var target = hit.collider.GetComponentInParent<T>();
				if (target == null) continue;
				if (hit.distance < minDistance && (filter == null || filter.Invoke(target))) {
					minDistance = hit.distance;
					result = target;
				}
			}

			return result;
		}
	}

}