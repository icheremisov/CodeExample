using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace XLib.Unity.Utils {

	public partial class ObjectPool {

		private void RemovePool(int instanceId, bool destroyObjects = true) {
			var hasPool = _pooledStructs.ContainsKey(instanceId);
			if (!hasPool) return;

			if (destroyObjects) {
				var instancePooledStruct = _pooledStructs[instanceId];
				foreach (var go in instancePooledStruct) Object.Destroy(go.GameObject);

				foreach (var objectValue in Instance._spawnedObjects.Values)
					if (objectValue.Component != null)
						Object.Destroy(objectValue.Component.gameObject);
			}

			_spawnedObjects.RemoveAll(x => x.Value.Id == instanceId);
			_pooledStructs.Remove(instanceId);
		}

		public void CreatePool<T>(T component, int initialPoolSize = 0, PoolParams poolParams = PoolParams.Default) where T : Component {
			var instanceId = component.GetInstanceID();
			if (_pooledStructs.ContainsKey(instanceId)) return;

			var objList = new List<PooledObject>(DefaultQueueCapacity);
			_pooledStructs.Add(instanceId, objList);

			if (initialPoolSize <= 0) return;

			var tr = component.transform;

			for (var i = 0; i < initialPoolSize; ++i) {
				var t = InstantiateObject(tr, poolParams.Has(PoolParams.DoNotDestroyOnLoad) ? GlobalRoot : LocalRoot);
				t.gameObject.SetActive(false);
				var genericComponent = t.GetComponent<T>();

				var poolable = genericComponent as IPoolable;
				poolable?.OnSpawn();

				var poolObject = new PooledObject(genericComponent, t.gameObject, t, poolable);

				objList.Add(poolObject);
			}
		}

		public void RemovePool<T>(T component, bool destroyObjects = true) where T : Component {
			RemovePool(component.GetInstanceID(), destroyObjects);
		}

		public T Spawn<T>(T component, PoolParams poolParams = default, Transform parent = null, Vector3 position = default,
			Quaternion rotation = default) where T : Component {
			var instanceId = component.GetInstanceID();
			T componentInstance;
			var hasPool = _pooledStructs.ContainsKey(instanceId);
			if (!hasPool) CreatePool(component, DefaultPoolSize);

			GameObject gameObject;
			Transform transform;
			IPoolable poolable;

			if (_pooledStructs[instanceId].Count == 0) SpawnMore();

			var poolableObjects = _pooledStructs[instanceId];
			for (var i = poolableObjects.Count - 1; i >= 0; i--) {
				if (poolableObjects[i].Component != null) break;

				poolableObjects.RemoveAt(i);
			}

			if (poolableObjects.Count == 0) SpawnMore();

			var pooledObject = _pooledStructs[instanceId].RemoveLast();

			Object tmp = pooledObject.Component;
			gameObject = pooledObject.GameObject;
			transform = pooledObject.Transform;
			poolable = pooledObject.Poolable;

			if (tmp == null) return null;

			componentInstance = (T)tmp;
			var scale = transform.localScale;
			parent = parent ? parent : GetDefaultRootTransform(poolParams.Has(PoolParams.DoNotDestroyOnLoad));
			transform.SetParent(parent);
			var world = poolParams.Has(PoolParams.WorldPosition);
			if (world) {
				transform.position = position;
				transform.rotation = rotation;
			}
			else {
				transform.localPosition = position;
				transform.localRotation = rotation;
			}

			transform.localScale = scale;

			gameObject.SetActive(true);
			_spawnedObjects[componentInstance.GetInstanceID()] =
				new SpawnedObject(instanceId, poolParams.Has(PoolParams.DoNotDestroyOnLoad), componentInstance);
			poolable?.OnSpawn();
			return componentInstance;

			void SpawnMore() {
				transform = InstantiateObject(component.transform, parent, position, rotation);
				gameObject = transform.gameObject;
				componentInstance = gameObject.GetComponent<T>();
				poolable = componentInstance as IPoolable;
				_spawnedObjects[componentInstance.GetInstanceID()] =
					new SpawnedObject(instanceId, poolParams.Has(PoolParams.DoNotDestroyOnLoad), componentInstance);
				_pooledStructs[instanceId].Add(new PooledObject(componentInstance, gameObject, transform, poolable));
			}
		}

		public void Recycle(Component component, float delay = 0, PoolParams recycleParams = PoolParams.Default) {
			if (component == null) {
				Debug.LogWarning("Recycle component is null!");
				return;
			}

			var instanceId = component.GetInstanceID();
			if (_spawnedObjects.TryGetValue(instanceId, out var spawnedObj)) {
				void DoRecycle() {
					if (!_pooledStructs.ContainsKey(spawnedObj.Id)) {
						if (_spawnedObjects.ContainsKey(instanceId)) _spawnedObjects.Remove(instanceId);

						Debug.LogWarning("Pool dont exist this spawn instance!");
						Object.Destroy(component.gameObject, delay);
						return;
					}

					var gameObject = component.gameObject;
					var transform = component.transform;

					var poolable = component as IPoolable;
					poolable?.OnDespawn();
					var poolObject = new PooledObject(component, gameObject, transform, poolable);

					_pooledStructs[spawnedObj.Id].Add(poolObject);
					_spawnedObjects.Remove(instanceId);
					if (recycleParams.Has(PoolParams.ClearParent))
						transform.SetParent(GetDefaultRootTransform(spawnedObj.IsPersistent), recycleParams.Has(PoolParams.WorldPosition));

					gameObject.SetActive(false);
				}

				if (delay > 0)
					UniTask.Delay(TimeSpan.FromSeconds(delay), true).ContinueWith(DoRecycle).Forget();
				else
					DoRecycle();
			}
			else {
				if (_pooledStructs.Values.Any(pooledStruct =>
						pooledStruct.Any(pooledObject => pooledObject.Component.GetInstanceID() == instanceId))) {
					Debug.LogError("You are trying to recycle a previously recycled object. Set null your variable after recycle to avoid this error");
					return;
				}

				Object.Destroy(component.gameObject, delay);
			}
		}

		public void Cleanup() {
			var instance = Instance;

			foreach (var pooledDict in instance._pooledStructs) {
				for (var index = 0; index < pooledDict.Value.Count; index++) {
					var pooledObject = pooledDict.Value[index];
					if (pooledObject.Component == null) pooledDict.Value.Remove(pooledObject);
				}
			}

			for (var i = 0; i < instance._spawnedObjects.Count; i++) {
				var objectInstance = instance._spawnedObjects.ElementAt(i);
				if (objectInstance.Value.Component == null) instance._spawnedObjects.Remove(objectInstance.Key);
			}
		}

		private void Cleanup(UnityEngine.SceneManagement.Scene arg0, LoadSceneMode arg1) {
			Cleanup();
		}

	}

}