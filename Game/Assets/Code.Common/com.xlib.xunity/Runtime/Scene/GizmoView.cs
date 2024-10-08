using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

#endif

namespace XLib.Unity.Scene {

#if UNITY_EDITOR
	[DrawGizmo(GizmoType.Pickable)]
#endif
	public class GizmoView : MonoBehaviour {

		public float size = 1.0f;
		public string text = "";
		public bool wireframe = true;
		public bool selectedOnly = true;

		public bool showAxes;

		public Color color = Color.yellow;
#if UNITY_EDITOR
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

			// Draw the gizmo
			Gizmos.matrix = transform.localToWorldMatrix;
			var selected = Selection.gameObjects.Any(x => x == gameObject || (transform.parent != null && x == transform.parent.gameObject));

			Gizmos.color = selected ? color : color * 0.75f;

			if (wireframe)
				Gizmos.DrawWireSphere(Vector3.zero, size);
			else
				Gizmos.DrawSphere(Vector3.zero, size);

			var sz = Vector3.Scale(transform.lossyScale, new Vector3(size, size, size));

			Handles.Label(transform.position + sz, text.Length == 0 ? name : text);

			if (showAxes) {
				Gizmos.color = Color.red;
				Gizmos.DrawLine(Vector3.zero, Vector3.right * size * 2);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(Vector3.zero, Vector3.up * size * 2);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(Vector3.zero, Vector3.forward * size * 2);
			}
		}
#endif

	}

}