using UnityEngine;

// ReSharper disable MethodOverloadWithOptionalParameter
// ReSharper disable once MethodOverloadWithOptionalParameter

namespace XLib.Unity.Utils {

	public static class ObjectPoolExtensions {

		public static void CreatePool<T>(this T obj, int initialPoolSize = 0) where T : Component {
			ObjectPool.Instance.CreatePool(obj, initialPoolSize);
		}

		public static void CreatePool(this GameObject obj, int initialPoolSize = 0) {
			ObjectPool.Instance.CreatePool(obj.transform, initialPoolSize);
		}

		public static void RemovePool<T>(this T obj) where T : Component {
			ObjectPool.Instance.RemovePool(obj);
		}

		public static void RemovePool(this GameObject obj) {
			ObjectPool.Instance.RemovePool(obj.transform);
		}

		public static T Spawn<T>(this T component, ObjectPool.PoolParams poolParams)
			where T : Component =>
			ObjectPool.Instance.Spawn(component, poolParams, null, Vector3.zero, Quaternion.identity);

		public static T Spawn<T>(this T component, Transform parent, ObjectPool.PoolParams poolParams)
			where T : Component =>
			ObjectPool.Instance.Spawn(component, poolParams, parent, Vector3.zero, Quaternion.identity);

		public static T Spawn<T>(this T component, Transform parent, Vector3 position, ObjectPool.PoolParams poolParams)
			where T : Component =>
			ObjectPool.Instance.Spawn(component, poolParams, parent, position, Quaternion.identity);

		public static T Spawn<T>(this T component, Transform parent = null, Vector3 position = default, Quaternion rotation = default,
			ObjectPool.PoolParams poolParams = ObjectPool.PoolParams.Default)
			where T : Component =>
			ObjectPool.Instance.Spawn(component, poolParams, parent, position, rotation);

		public static Transform Spawn(this GameObject gameObject, Transform parent = null, Vector3 position = default,
			Quaternion rotation = default,
			ObjectPool.PoolParams poolParams = default) =>
			ObjectPool.Instance.Spawn(gameObject.transform, poolParams, parent, position, rotation);

		public static void Recycle(this Component obj, float delay = 0, ObjectPool.PoolParams poolParams = default) {
			ObjectPool.Instance.Recycle(obj, delay, poolParams);
		}

		public static void Recycle(this GameObject obj, float delay = 0, ObjectPool.PoolParams poolParams = default) {
			ObjectPool.Instance.Recycle(obj.transform, delay, poolParams);
		}

		public static void RecycleAll(this Transform tr, float delay = 0, ObjectPool.PoolParams poolParams = default) {
			var childCount = tr.childCount;
			for (var i = childCount - 1; i >= 0; --i) {
				var t = tr.GetChild(i);
				if (t == null) continue;

				t.Recycle(delay, poolParams);
			}
		}

	}

}