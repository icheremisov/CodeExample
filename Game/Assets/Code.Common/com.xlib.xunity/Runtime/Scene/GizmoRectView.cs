using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

#endif

namespace XLib.Unity.Scene {

#if UNITY_EDITOR
	[DrawGizmo(GizmoType.Pickable)]
#endif
	public class GizmoRectView : MonoBehaviour {

#if UNITY_EDITOR
		public bool selectedOnly = true;
		public Color color = Color.yellow;
		private readonly Vector3[] _corners = new Vector3[4];

		private void Start() {
			// dont delete this
		}

		private void OnDrawGizmos() {
			if (selectedOnly) return;

			DrawGizmos();
		}

		private void OnDrawGizmosSelected() {
			if (!selectedOnly) return;

			DrawGizmos();
		}

		private void DrawGizmos() {
			if (!enabled) return;

			var tm = transform as RectTransform;
			if (tm == null) return;

			// Draw the gizmo
			var selected = Selection.gameObjects.Any(x => x == gameObject || (transform.parent != null && x == transform.parent.gameObject));

			Gizmos.color = selected ? color : color * 0.75f;

			tm.GetWorldCorners(_corners);

			Gizmos.DrawLine(_corners[0], _corners[1]);
			Gizmos.DrawLine(_corners[1], _corners[2]);
			Gizmos.DrawLine(_corners[2], _corners[3]);
			Gizmos.DrawLine(_corners[3], _corners[0]);
		}
#endif

	}

}