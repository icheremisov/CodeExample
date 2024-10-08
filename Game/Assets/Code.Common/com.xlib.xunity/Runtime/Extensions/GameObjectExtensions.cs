using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLib.Unity.Core;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public static partial class GameObjectExtensions {
	/// <summary>
	///     Helper function that recursively sets all children with widgets' game objects layers to the specified value.
	/// </summary>
	public static void SetLayer(this Transform t, int layer) {
		t.gameObject.layer = layer;
		for (var i = 0; i < t.childCount; ++i) {
			var child = t.GetChild(i);
			child.gameObject.layer = layer;

			SetLayer(child, layer);
		}
	}

	public static void SetLayer(this GameObject obj, int layer) {
		obj.transform.SetLayer(layer);
	}

	public static void CopyPRS(this Component obj, GameObject item) {
		obj.transform.SetPositionAndRotation(item.transform.position, item.transform.rotation);
		obj.transform.localScale = item.transform.localScale;
	}

	public static void CopyPRS(this GameObject obj, GameObject item) {
		obj.transform.SetPositionAndRotation(item.transform.position, item.transform.rotation);
		obj.transform.localScale = item.transform.localScale;
	}

	public static void CopyPRS(this Component obj, Component item) {
		obj.transform.SetPositionAndRotation(item.transform.position, item.transform.rotation);
		obj.transform.localScale = item.transform.localScale;
	}

	public static void CopyPRS(this GameObject obj, Component item) {
		obj.transform.SetPositionAndRotation(item.transform.position, item.transform.rotation);
		obj.transform.localScale = item.transform.localScale;
	}

	public static void ResetPRS<T>(this T obj, bool resetPos = true, bool resetRot = true, bool resetScale = true) where T : Component {
		var childTm = obj.transform;

		if (resetPos) childTm.localPosition = Vector3.zero;
		if (resetRot) childTm.localRotation = Quaternion.identity;
		if (resetScale) childTm.localScale = Vector3.one;
	}

	public static void ResetPRS(this GameObject obj, bool resetPos = true, bool resetRot = true, bool resetScale = true) {
		var childTm = obj.transform;

		if (resetPos) childTm.localPosition = Vector3.zero;
		if (resetRot) childTm.localRotation = Quaternion.identity;
		if (resetScale) childTm.localScale = Vector3.one;
	}

	public static void AddChildResetPRS<T, C>(this T obj, C child, bool resetPos = true, bool resetRot = true, bool resetScale = true)
		where T : Component where C : Component {
		var childTm = child.transform;

		childTm.SetParent(obj.transform, false);
		if (resetPos) childTm.localPosition = Vector3.zero;
		if (resetRot) childTm.localRotation = Quaternion.identity;
		if (resetScale) childTm.localScale = Vector3.one;
	}

	public static void AddChildResetPRS<C>(this GameObject obj, C child, bool resetPos = true, bool resetRot = true, bool resetScale = true) where C : Component {
		var childTm = child.transform;

		childTm.SetParent(obj.transform, false);
		if (resetPos) childTm.localPosition = Vector3.zero;
		if (resetRot) childTm.localRotation = Quaternion.identity;
		if (resetScale) childTm.localScale = Vector3.one;
	}

	public static void AddChildResetPRS<T>(this T obj, GameObject child, bool resetPos = true, bool resetRot = true, bool resetScale = true) where T : Component {
		var childTm = child.transform;

		childTm.SetParent(obj.transform, false);
		if (resetPos) childTm.localPosition = Vector3.zero;
		if (resetRot) childTm.localRotation = Quaternion.identity;
		if (resetScale) childTm.localScale = Vector3.one;
	}

	public static void AddChildResetPRS(this GameObject obj, GameObject child, bool resetPos = true, bool resetRot = true, bool resetScale = true) {
		var childTm = child.transform;

		childTm.SetParent(obj.transform, false);
		if (resetPos) childTm.localPosition = Vector3.zero;
		if (resetRot) childTm.localRotation = Quaternion.identity;
		if (resetScale) childTm.localScale = Vector3.one;
	}

	public static void AlignAs(this GameObject obj, GameObject other, bool scale = true) {
		var tm = obj.transform;
		var otherTm = other.transform;

		tm.SetPositionAndRotation(otherTm.position, otherTm.rotation);
		if (scale) tm.localScale = otherTm.localScale;
	}

	public static void AlignAs<T, C>(this T obj, C other, bool scale = true) where T : Component where C : Component {
		var tm = obj.transform;
		var otherTm = other.transform;

		tm.SetPositionAndRotation(otherTm.position, otherTm.rotation);
		if (scale) tm.localScale = otherTm.localScale;
	}

	public static void AlignAs<T>(this T obj, GameObject other, bool scale = true) where T : Component {
		var tm = obj.transform;
		var otherTm = other.transform;

		tm.SetPositionAndRotation(otherTm.position, otherTm.rotation);
		if (scale) tm.localScale = otherTm.localScale;
	}

	public static void SetLocalScale(this Transform self, float s) {
		self.localScale = new Vector3(s, s, s);
	}

	public static float GetLocalScale(this Transform self) => self.localScale.x;

	public static T GetComponentInParent<T>(this Component self) => self.gameObject.GetComponentInParent<T>();

	public static T GetComponentInParent<T>(this GameObject self) {
		var p = self.transform.parent;

		while (p != null) {
			var c = p.GetComponent<T>();

			if (c != null) return c;

			p = p.parent;
		}

		return default;
	}

	public static bool HasComponent<T>(this Component self) where T : Component => self.gameObject.GetComponent<T>() != null;

	public static bool HasComponent<T>(this GameObject self) where T : Component => self.GetComponent<T>() != null;

	public static T GetExistingComponent<T>(this Component self) => self.gameObject.GetExistingComponent<T>();

	public static T GetExistingComponent<T>(this GameObject self) {
		var c = self != null ? self.GetComponent<T>() : default;

		if (c == null) throw new Exception($"{self.GetFullPath()}: required component not found: {typeof(T).Name}");

		return c;
	}

	public static T[] GetComponents<T>(this GameObject self) where T : class {
		var c = self.GetComponents(typeof(T));

		return Array.ConvertAll(c, input => input as T);
	}

	public static T GetComponent<T>(this Component self, string childName) => self.gameObject.GetComponent<T>(childName);

	public static T GetComponent<T>(this GameObject self, string childName) {
		var obj = self.transform.Find(childName);
		return obj != null ? obj.GetComponent<T>() : default;
	}

	public static T GetOrAddComponent<T>(this Component self) where T : Component => self.gameObject.GetOrAddComponent<T>();

	public static T GetOrAddComponent<T>(this GameObject self) where T : Component {
		var c = self.GetComponent<T>();
		if (c == null) c = self.AddComponent<T>();

		return c;
	}

	public static void DisableComponent<T>(this Component self) where T : Behaviour {
		self.gameObject.DisableComponent<T>();
	}

	public static void DisableComponent<T>(this GameObject self) where T : Behaviour {
		var c = self.GetComponent<T>();
		if (c != null) c.enabled = false;
	}

	public static T GetExistingComponent<T>(this Component self, string childName) where T : Component => self.gameObject.GetExistingComponent<T>(childName);

	public static T GetExistingComponent<T>(this GameObject self, string childName) where T : Component {
		var obj = self.transform.Find(childName);
		var c = obj != null ? obj.GetComponent<T>() : null;

		if (c == null) Debug.LogError(self.name + ": required component or child object not found: '" + childName + "'." + typeof(T).Name);

		return c;
	}

	public static T GetComponentInChildren<T>(this Component self, string childName) where T : Component => self.gameObject.GetComponentInChildren<T>(childName);

	public static GameObject FindGameObjectByPath(this Scene scene, string scenePath) {
		var childrenNames = scenePath.Split("/");
		if (childrenNames.IsNullOrEmpty()) {
			Debug.LogError("Path is null");
			return null;
		}

		var firstNameIdx = childrenNames.IndexOf(x => !x.IsNullOrEmpty());
		if (firstNameIdx < 0) {
			Debug.LogError("Path is null");
			return null;
		}

		var childName = childrenNames[firstNameIdx];
		return scene.GetRootGameObjects().FirstOrDefault(x => x.name == childName).FindGameObjectByPath(scenePath);
	}

	public static GameObject FindGameObjectByPath(this GameObject root, string scenePath) {
		var childrenNames = scenePath.Split("/");
		if (childrenNames.IsNullOrEmpty()) {
			Debug.LogError("Path is null");
			return null;
		}

		var firstNameIdx = childrenNames.IndexOf(x => !x.IsNullOrEmpty());
		if (firstNameIdx < 0) {
			Debug.LogError("Path is null");
			return null;
		}

		for (var i = firstNameIdx; i < childrenNames.Length; i++) {
			var childName = childrenNames[i];
			if (childName.IsNullOrEmpty() || (i == firstNameIdx && childrenNames[firstNameIdx] == root.name)) continue;
			root = root.Child(childName);
			if (root != null) continue;
			Debug.LogError($"Cannot find GameObject in path {scenePath}");
			return null;
		}

		return root;
	}

	public static T GetComponentInChildren<T>(this GameObject self, string childName) where T : Component {
		var obj = self.transform.Find(childName);
		return obj != null ? obj.gameObject.GetComponent<T>() : null;
	}

	public static IEnumerable<Transform> EnumerateParents(this Component obj) {
		foreach (var parent in obj.gameObject.EnumerateParents()) yield return parent;
	}

	public static IEnumerable<Transform> EnumerateParents(this GameObject obj) {
		var p = obj.transform.parent;

		while (p != null) {
			yield return p;
			p = p.parent;
		}
	}

	public static string GetFullPath(this GameObject obj) {
		if (obj == null) return string.Empty;

		return obj.transform.GetFullPath();
	}

	public static string GetFullPath(this Component obj) {
		if (obj == null) return string.Empty;

		var builder = new StringBuilder(256);

		foreach (var p in obj.EnumerateParents().Reverse()) {
			builder.Append('/');
			builder.Append(p.name);
		}

		builder.Append('/');
		builder.Append(obj.name);

		return builder.ToString();
	}

	public static IEnumerable<T> ChildrenOfType<T>(this Component obj, bool recursive = false) where T : Component {
		var t = obj.transform;
		var c = t.childCount;

		for (var i = 0; i < c; ++i) {
			var childTM = t.GetChild(i);
			if (!childTM) continue;

			var child = childTM.GetComponent<T>();
			if (child) yield return child;

			if (recursive)
				foreach (var childEnum in childTM.ChildrenOfType<T>(true))
					yield return childEnum;
		}
	}

	public static void DestroyAllChildren(this Transform t) {
		var isPlaying = Application.isPlaying;

		while (t.childCount != 0) {
			var child = t.GetChild(0);

			if (isPlaying) {
				child.SetParent(null);
				Object.Destroy(child.gameObject);
			}
			else
				Object.DestroyImmediate(child.gameObject);
		}
	}

	public static void DestroyAllChildren(this Transform t, Func<Transform, bool> needRemoved) {
		var isPlaying = Application.isPlaying;

		for (var i = 0; i < t.childCount;) {
			var child = t.GetChild(i);

			if (!needRemoved(child)) {
				++i;
				continue;
			}

			if (isPlaying) {
				child.SetParent(null);
				Object.Destroy(child.gameObject);
			}
			else
				Object.DestroyImmediate(child.gameObject);
		}
	}

	public static void DestroyAll(this ICollection<GameObject> list) {
		var isPlaying = Application.isPlaying;
		foreach (var o in list) {
			if (isPlaying) {
				o.transform.SetParent(null);
				Object.Destroy(o);
			}
			else
				Object.DestroyImmediate(o);
		}

		list.Clear();
	}

	public static void DestroyAllComponents<TComponent>(this ICollection<TComponent> list) where TComponent : Component {
		var isPlaying = Application.isPlaying;
		foreach (var o in list) {
			if (isPlaying) {
				Object.Destroy(o);
			}
			else
				Object.DestroyImmediate(o);
		}

		list.Clear();
	}

	public static void DestroyAllGameObjects<TComponent>(this ICollection<TComponent> list) where TComponent : Component {
		var isPlaying = Application.isPlaying;
		foreach (var o in list) {
			if (isPlaying) {
				o.transform.SetParent(null);
				Object.Destroy(o.gameObject);
			}
			else
				Object.DestroyImmediate(o.gameObject);
		}

		list.Clear();
	}

	public static Transform[] ChildrenToArray(this Transform t) {
		var c = t.childCount;
		var result = new Transform[c];

		for (var i = 0; i < c; i++) result[i] = t.GetChild(i);

		return result;
	}

	public static GameObject GetRootObject(this GameObject obj) {
		if (obj == null) return null;

		var tm = obj.transform;

		while (tm.parent != null) tm = tm.parent;

		return tm.gameObject;
	}

	/// get all root objects of all scenes 
	public static IEnumerable<GameObject> GetAllRootObjects() {
		for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++) {
			var s = SceneManager.GetSceneAt(sceneIndex);
			if (!s.isLoaded) continue;

			foreach (var rootGameObject in s.GetRootGameObjects()) {
				yield return rootGameObject;
			}
		}
	}

	/// Use this method to get all loaded objects of some type, including inactive objects. 
	/// This is an alternative to Resources.FindObjectsOfTypeAll (returns project assets, including prefabs), and GameObject.FindObjectsOfTypeAll (deprecated).
	public static T[] FindObjectsOfTypeAll<T>() where T : Component {
		var results = new List<T>(64);
		for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++) {
			var s = SceneManager.GetSceneAt(sceneIndex);
			if (!s.isLoaded) continue;

			var allGameObjects = s.GetRootGameObjects();
			for (var objIndex = 0; objIndex < allGameObjects.Length; objIndex++) {
				var go = allGameObjects[objIndex];
				results.AddRange(go.GetComponentsInChildren<T>(true));
			}
		}

		return results.ToArray();
	}

	/// Use this method to get all loaded objects of some type, including inactive objects. 
	/// This is an alternative to Resources.FindObjectsOfTypeAll (returns project assets, including prefabs), and GameObject.FindObjectsOfTypeAll (deprecated).
	public static Component[] FindObjectsOfTypeAll(Type t, Scene? limitScene = null) {
		var results = new List<Component>(64);

		void EnumObjects(Scene s) {
			var allGameObjects = s.GetRootGameObjects();
			for (var objIndex = 0; objIndex < allGameObjects.Length; objIndex++) {
				var go = allGameObjects[objIndex];
				results.AddRange(go.GetComponentsInChildren(t, true));
			}
		}

		if (limitScene.HasValue)
			EnumObjects(limitScene.Value);
		else {
			for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++) {
				var s = SceneManager.GetSceneAt(sceneIndex);
				if (!s.isLoaded) continue;

				EnumObjects(s);
			}
		}

		return results.ToArray();
	}

	/// <summary>
	///     enumerate trough all children
	/// </summary>
	public static IEnumerable<Transform> AsEnumerable(this Transform tm) {
		for (var i = 0; i < tm.childCount; i++) yield return tm.GetChild(i);
	}

	public static void FillParent(this RectTransform tm) {
		tm.anchorMin = Vector2.zero;
		tm.anchorMax = Vector2.one;
		tm.sizeDelta = Vector2.zero;
	}

	public static void SetActive(this IEnumerable<GameObject> objects, bool isActive) {
		if (objects == null) return;

		foreach (var obj in objects) {
			if (obj) obj.SetActive(isActive);
		}
	}

	public static void SetActive<T>(this IEnumerable<T> objects, bool isActive) where T : Component {
		if (objects == null) return;

		foreach (var obj in objects) {
			if (obj) obj.gameObject.SetActive(isActive);
		}
	}

	public static void SetEnabled<T>(this IEnumerable<T> objects, bool enabled) where T : Behaviour {
		if (objects == null) return;

		foreach (var obj in objects) {
			if (obj) obj.enabled = enabled;
		}
	}

	public static void SetActive(this Component component, bool isActive) {
		if (component == null) return;
		var go = component.gameObject;
		if (go == null) return;
		if (go.activeSelf != isActive) go.SetActive(isActive);
	}

	public static GameObject ToGameObject(this Component component) => component == null ? null : component.gameObject;

	public static Bounds CalculateBounds(
		this GameObject go,
		bool spriteVertices = false,
		bool local = false,
		bool includeInactive = false) {
		if (go == null) return new Bounds();

		var transform = go.transform;
		var rotation = transform.rotation;
		if (local) transform.rotation = Quaternion.identity;

		var bounds = new Bounds { extents = Vector3.zero, center = transform.position };

		foreach (var renderer in go.GetComponentsInChildren<Renderer>(includeInactive)) {
			switch (renderer) {
				case ParticleSystemRenderer: continue;

				case SpriteRenderer spriteRenderer when spriteRenderer.sprite != null && spriteVertices: {
					var spriteTransform = spriteRenderer.transform;
					foreach (var vertex in spriteRenderer.sprite.vertices) bounds.Encapsulate(spriteTransform.TransformPoint(vertex));
					continue;
				}

				default:
					bounds.Encapsulate(renderer.bounds);
					continue;
			}
		}

		foreach (var provider in go.GetComponentsInChildren<IBoundsProvider>(includeInactive)) {
			bounds.Encapsulate(provider.GetBounds(local));
		}

		if (!local) return bounds;

		bounds.center -= transform.position;
		transform.rotation = rotation;
		return bounds;
	}

	public static Rect BoundsToScreen(this GameObject go, Camera objectCamera) {
		var bounds = go.CalculateBounds(true);

		var position = bounds.center;
		var extents = bounds.extents;

		var p1 = objectCamera.WorldToScreenPoint(position + new Vector3(extents.x, extents.y, extents.z));
		var p2 = objectCamera.WorldToScreenPoint(position + new Vector3(extents.x, extents.y, -extents.z));
		var p3 = objectCamera.WorldToScreenPoint(position + new Vector3(extents.x, -extents.y, extents.z));
		var p4 = objectCamera.WorldToScreenPoint(position + new Vector3(extents.x, -extents.y, -extents.z));
		var p5 = objectCamera.WorldToScreenPoint(position + new Vector3(-extents.x, extents.y, extents.z));
		var p6 = objectCamera.WorldToScreenPoint(position + new Vector3(-extents.x, extents.y, -extents.z));
		var p7 = objectCamera.WorldToScreenPoint(position + new Vector3(-extents.x, -extents.y, extents.z));
		var p8 = objectCamera.WorldToScreenPoint(position + new Vector3(-extents.x, -extents.y, -extents.z));

		var maxX = Mathf.Max(p1.x, p2.x, p3.x, p4.x, p5.x, p6.x, p7.x, p8.x);
		var minX = Mathf.Min(p1.x, p2.x, p3.x, p4.x, p5.x, p6.x, p7.x, p8.x);
		var maxY = Mathf.Max(p1.y, p2.y, p3.y, p4.y, p5.y, p6.y, p7.y, p8.y);
		var minY = Mathf.Min(p1.y, p2.y, p3.y, p4.y, p5.y, p6.y, p7.y, p8.y);

		var leftBottom = new Vector2(minX, minY);
		var rightTop = new Vector2(maxX, maxY);

		return new Rect(leftBottom, rightTop - leftBottom);
	}
}