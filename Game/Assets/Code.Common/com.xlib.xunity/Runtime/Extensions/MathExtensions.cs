using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

[SuppressMessage("ReSharper", "InconsistentNaming")]
// ReSharper disable once CheckNamespace
public static class MathExtensions {

	public static double ClampInfinity(this double v) {
		if (v >= double.PositiveInfinity) v = double.MaxValue;

		return v;
	}

	public static float ClampInfinity(this float v) {
		if (v >= float.PositiveInfinity) v = float.MaxValue;

		return v;
	}

	public static BigInteger ToBigInteger(this double v) => new(v.ClampInfinity());

	public static BigInteger ToBigInteger(this float v) => new(v.ClampInfinity());

	public static Vector2 ToXY(this Vector3 v) => new(v.x, v.y);

	public static Vector2 ToXZ(this Vector3 v) => new(v.x, v.z);

	public static Vector2 ToYZ(this Vector3 v) => new(v.y, v.z);

	public static Vector3 To0YZ(this Vector3 v, float x = 0.0f) => new(x, v.y, v.z);

	public static Vector3 ToX0Z(this Vector2 v, float y = 0.0f) => new(v.x, y, v.y);

	public static Vector3 ToX0Z(this Vector3 v, float y = 0.0f) => new(v.x, y, v.z);

	public static Vector3 ToXY0(this Vector3 v, float z = 0.0f) => new(v.x, v.y, z);

	public static Vector3 ToXY0(this Vector2 v, float z = 0.0f) => new(v.x, v.y, z);

	public static Vector2 To0Y(this Vector2 v, float x = 0.0f) => new(x, v.y);

	public static Vector2 ToX0(this Vector2 v, float y = 0.0f) => new(v.x, y);
	public static Vector3 FlipX(this Vector3 v) => new(-v.x, v.y, v.z);
	public static Vector3 FlipY(this Vector3 v) => new(v.x, -v.y, v.z);
	public static Vector3 FlipZ(this Vector3 v) => new(v.x, v.y, -v.z);

	public static bool SameAs(this Vector2 v, Vector2 other, float epsilon = 0.0001f) {
		return Mathf.Abs(v.x - other.x) <= epsilon && Mathf.Abs(v.y - other.y) <= epsilon;
	}

	public static bool SameAs(this Vector3 v, Vector3 other, float epsilon = 0.0001f) {
		return Mathf.Abs(v.x - other.x) <= epsilon && Mathf.Abs(v.y - other.y) <= epsilon && Mathf.Abs(v.z - other.z) <= epsilon;
	}

	public static bool SameAs(this Vector4 v, Vector4 other, float epsilon = 0.0001f) {
		return Mathf.Abs(v.x - other.x) <= epsilon && Mathf.Abs(v.y - other.y) <= epsilon && Mathf.Abs(v.z - other.z) <= epsilon && Mathf.Abs(v.w - other.w) <= epsilon;
	}

	public static Color ReplaceA(this Color self, float a) {
		self.a = a;
		return self;
	}

	public static Color SetAlpha(this Color self, float a) {
		self.a = a;
		return self;
	}
	
	public static Color MulA(this Color self, float a) {
		self.a *= a;
		return self;
	}

	public static Color MulRGB(this Color self, float k) {
		self.r *= k;
		self.g *= k;
		self.b *= k;
		return self;
	}
	
	public static Color RGBMultiplied(this Color self, Color multiplier) => 
		new Color(self.r * multiplier.r, self.g * multiplier.g, self.b * multiplier.b, self.a);

	public static Vector4 ToVector4(this Color self) => new(self.r, self.g, self.b, self.a);

	public static bool Contains(this Rect self, Rect rect) => self.Contains(rect.min) && self.Contains(rect.max);

	public static Vector2 GetRotated(this Vector2 v, float degrees) {
		var sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
		var cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

		var tx = v.x;
		var ty = v.y;
		v.x = cos * tx - sin * ty;
		v.y = sin * tx + cos * ty;
		return v;
	}

	public static bool Raycast(this Plane p, Ray ray, out Vector3 position) {
		if (!p.Raycast(ray, out var rayDist)) {
			position = Vector3.zero;
			return false;
		}

		position = ray.GetPoint(rayDist);

		return true;
	}

	public static bool PointInFrustum(this Camera c, Vector3 worldPos) {
		var p = c.WorldToViewportPoint(worldPos);
		return p.x is >= 0 and <= 1 && p.y is >= 0 and <= 1 && p.z > 0;
	}

	public static float GetRandom(this Vector2 vector) => Random.Range(vector.x, vector.y);

	public static int GetRandom(this Vector2Int vector) => Random.Range(vector.x, vector.y);

}