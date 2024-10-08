using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace XLib.Unity.Utils {

	public partial class ObjectPool {

		[Flags]
		public enum PoolParams {

			/// <summary>
			///     Reset position
			/// </summary>
			Default = 1,

			/// <summary>
			///     Leave WorldPosition as is
			/// </summary>
			WorldPosition = 1 << 2,

			/// <summary>
			///     Make object persistent
			/// </summary>
			DoNotDestroyOnLoad = 1 << 3,

			/// <summary>
			///     Clear parent
			/// </summary>
			ClearParent = 1 << 4

		}

		private const int DefaultPoolSize = 1;
		private const int DefaultQueueCapacity = 8;

		// ReSharper disable once MemberCanBePrivate.Global
		// ReSharper disable once InconsistentNaming
		private static ObjectPool _instance;

		// Prefab ID - Component
		private readonly Dictionary<int, List<PooledObject>> _pooledStructs = new(8);

		// Instantiated ID - Prefab ID
		private readonly Dictionary<int, SpawnedObject> _spawnedObjects = new(8);
		private Transform _globalRoot;

		private Transform _localRoot;

		internal Transform LocalRoot {
			get {
				if (_localRoot == null) _localRoot = new GameObject("_LocalObjectPool").transform;

				return _localRoot;
			}
		}

		internal Transform GlobalRoot {
			get {
				if (_globalRoot == null) {
					_globalRoot = new GameObject("_GlobalObjectPool").transform;
					Object.DontDestroyOnLoad(_globalRoot.gameObject);
				}

				return _globalRoot;
			}
		}

		// ReSharper disable once ConvertToNullCoalescingCompoundAssignment
		public static ObjectPool Instance => _instance ?? (_instance = GetOrCreateInstance());

		internal Transform GetDefaultRootTransform(bool isDontDestroyOnLoad) => isDontDestroyOnLoad ? GlobalRoot : LocalRoot;

		private static ObjectPool GetOrCreateInstance() {
			var pool = new ObjectPool();
			SceneManager.sceneLoaded += pool.Cleanup;
			return pool;
		}

		private Transform InstantiateObject(Transform tr, Transform parent = null,
			Vector3 position = default, Quaternion rotation = default) {
			var trans = Object.Instantiate(tr, position, rotation);
			if (trans == null) return trans;

			var scale = trans.localScale;
			trans.SetParent(parent);
			trans.localScale = scale;
			trans.localPosition = position;
			trans.localRotation = rotation;

			return trans;
		}

		public static void DestroyPool() {
			if (_instance == null) return;

			var keys = _instance._pooledStructs.Keys.ToArray();
			foreach (var t in keys) _instance.RemovePool(t);

			_instance._pooledStructs.Clear();

			if (_instance._globalRoot != null) Object.Destroy(_instance._globalRoot.gameObject);

			if (_instance._localRoot != null) Object.Destroy(_instance._localRoot.gameObject);

			_instance = null;
		}

		public struct SpawnedObject {

			public int Id { get; }
			public bool IsPersistent { get; }
			public Component Component { get; }

			public SpawnedObject(int id, bool isPersistent, Component component) {
				IsPersistent = isPersistent;
				Component = component;
				Id = id;
			}

		}

		public struct PooledObject {

			public Component Component { get; }
			public GameObject GameObject { get; }
			public Transform Transform { get; }
			public IPoolable Poolable { get; }

			public PooledObject(Component component, GameObject gameObject, Transform transform, IPoolable poolable) {
				Component = component;
				GameObject = gameObject;
				Transform = transform;
				Poolable = poolable;
			}

		}

#if UNITY_EDITOR
		// ReSharper disable once ConvertToAutoPropertyWhenPossible
		internal Dictionary<int, List<PooledObject>> PooledStructs => _pooledStructs;

		// ReSharper disable once ConvertToAutoProperty
		internal Dictionary<int, SpawnedObject> SpawnedObjects => _spawnedObjects;
#endif

	}

}