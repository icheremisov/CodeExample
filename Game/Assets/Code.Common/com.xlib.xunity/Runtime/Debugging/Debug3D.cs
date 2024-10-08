using System;
using UnityEngine;

namespace XLib.Unity.Debugging {

	// ReSharper disable once UnusedType.Global
	public static class Debug3D {

		public static void DrawCircleXZ(Vector2 center, float r, int sides, Color color) {
			DrawCircle(new Vector3(center.x, 0, center.y), Vector3.right, Vector3.forward, r, sides, color);
		}

		public static void DrawCircle(Vector3 center, Vector3 axisX, Vector3 axisY, float r, int sides, Color color) {
			if (sides < 3) return;

			var matrix = Gizmos.matrix;

			var step = 2.0f * Mathf.PI / sides;
			var a = 0.0f;
			var startPos = matrix.MultiplyPoint(center + axisX * (Mathf.Cos(a) * r) + axisY * (Mathf.Sin(a) * r));
			var prevPos = startPos;
			while (a < 2.0f * Mathf.PI) {
				var pos = matrix.MultiplyPoint(center + axisX * (Mathf.Cos(a) * r) + axisY * (Mathf.Sin(a) * r));
				Debug.DrawLine(prevPos, pos, color);
				a += step;

				prevPos = pos;
			}

			Debug.DrawLine(prevPos, startPos, color);
		}

		/// <summary>
		///     draw cylinder with height aligned along Y axis
		/// </summary>
		public static void DrawCylinder(Vector3 center, Vector3 axisX, Vector3 axisZ, float r, float h, int sides, Color color) {
			if (sides < 3) return;

			if (Mathf.Abs(h) <= 0.0001f) {
				DrawCircle(center, axisX, axisZ, r, sides, color);
				return;
			}

			var matrix = Gizmos.matrix;

			var step = 2.0f * Mathf.PI / sides;
			var a = 0.0f;

			var axisY = Vector3.Cross(axisZ, axisX);

			var startPosL = matrix.MultiplyPoint(center + axisX * (Mathf.Cos(a) * r) + axisZ * (Mathf.Sin(a) * r));
			var prevPosL = startPosL;

			var startPosH = matrix.MultiplyPoint(center + axisX * (Mathf.Cos(a) * r) + axisZ * (Mathf.Sin(a) * r) + axisY * h);
			var prevPosH = startPosH;

			while (a < 2.0f * Mathf.PI) {
				var posL = matrix.MultiplyPoint(center + axisX * (Mathf.Cos(a) * r) + axisZ * (Mathf.Sin(a) * r));
				var posH = matrix.MultiplyPoint(center + axisX * (Mathf.Cos(a) * r) + axisZ * (Mathf.Sin(a) * r) + axisY * h);
				Debug.DrawLine(prevPosL, posL, color);
				Debug.DrawLine(prevPosH, posH, color);
				Debug.DrawLine(posL, posH, color);
				a += step;

				prevPosL = posL;
				prevPosH = posH;
			}

			Debug.DrawLine(prevPosL, startPosL, color);
			Debug.DrawLine(prevPosH, startPosH, color);
		}

		public static void DrawArcXZ(Vector2 center, float r, float fromAdeg, float toAdeg, int sides, Color color) {
			DrawArc(new Vector3(center.x, 0, center.y), Vector3.right, Vector3.forward, r, fromAdeg, toAdeg, sides, color);
		}

		public static void DrawArc(Vector3 center, Vector3 axisX, Vector3 axisY, float r, float fromAdeg, float toAdeg, int sides, Color color) {
			if (sides < 3) return;

			var matrix = Gizmos.matrix;

			var a = Mathf.Deg2Rad * fromAdeg;
			var endA = Mathf.Deg2Rad * toAdeg;

			var step = (endA - a) / sides;
			var startPos = matrix.MultiplyPoint(center + axisX * (Mathf.Cos(a) * r) + axisY * (Mathf.Sin(a) * r));
			var prevPos = startPos;
			while (a < endA) {
				var pos = matrix.MultiplyPoint(center + axisX * (Mathf.Cos(a) * r) + axisY * (Mathf.Sin(a) * r));
				Debug.DrawLine(prevPos, pos, color);
				a += step;

				prevPos = pos;
			}

			if (Math.Abs(fromAdeg - toAdeg) < 0.01f) Debug.DrawLine(prevPos, startPos, color);
		}

		public static void DrawPoint(Vector3 pos, Color color, float size) {
			var matrix = Gizmos.matrix;

			size *= 0.5f;
			Debug.DrawLine(matrix.MultiplyPoint(pos + Vector3.back * size), matrix.MultiplyPoint(pos + Vector3.forward * size), color);
			Debug.DrawLine(matrix.MultiplyPoint(pos + Vector3.up * size), matrix.MultiplyPoint(pos + Vector3.down * size), color);
			Debug.DrawLine(matrix.MultiplyPoint(pos + Vector3.left * size), matrix.MultiplyPoint(pos + Vector3.right * size), color);
		}

	}

}