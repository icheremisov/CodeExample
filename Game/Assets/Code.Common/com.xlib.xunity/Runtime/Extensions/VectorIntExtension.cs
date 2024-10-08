using UnityEngine;

// ReSharper disable once CheckNamespace
public static class VectorIntExtension {

	public static Vector3Int ToVector3Int(this Vector2Int vector) => new(vector.x, vector.y, 0);

	public static Vector2Int ToVector2Int(this Vector3Int vector) => new(vector.x, vector.y);

}