using System;
using System.Collections.Generic;
using Client.Core.Ecs.Configs;
using Client.Core.Ecs.Types;
using Entitas;
using Entitas.VisualDebugging.Unity;
using UnityEngine;

namespace Client.Ecs.Core.Internal {

	internal class EcsFeature {

		private Feature _systems;

		private List<(int order, ISystem system)> _constructing = new(16);

		private bool _started;

		public EcsFeature(EcsFeatureConfig config) {
			Config = config;
			Id = new FeatureId(config.name);
			SortingOrder = config.SortingOrder;
		}

		public FeatureId Id { get; }
		public EcsFeatureConfig Config { get; }
		public int SortingOrder { get; }

		public void Set(Feature systems) {
			if (_constructing == null) throw new Exception("Cannot add new systems after start!");

			_systems = systems;
			_constructing = null;
		}

		public void Add(params ISystem[] systems) {
			if (systems.IsNullOrEmpty()) return;

			if (_constructing == null) throw new Exception("Cannot add new systems after start!");

			foreach (var system in systems) _constructing.Add((Config != null ? Config.GetOrder(system.GetType()) : 0, system));
		}

		public void Start() {
			if (_started) return;

			_started = true;

			if (_systems == null) Create();

			_systems.ActivateReactiveSystems();
			_systems.Initialize();

#if UNITY_EDITOR
			var allObjects = GameObject.FindObjectsOfType<DebugSystemsBehaviour>();
			allObjects.ForEach(x => {
				if (x.transform.parent == null) GameObject.DontDestroyOnLoad(x.gameObject);
			});
#endif
		}

		private void Create() {
			if (_constructing == null) throw new Exception("Cannot recreate Feature!");

			_systems = new Feature(Id.ToString());

			if (Config != null) _constructing.Sort((a, b) => a.order.CompareTo(b.order));

			foreach (var data in _constructing) _systems.Add(data.system);
			_constructing = null;
		}

		public void Update(bool force) {
			if (!force && !_started) return;

			if (_systems == null && force) Create();

			if (_systems == null) throw new Exception("Cannot update not started or destroyed systems!");

			_systems.Execute();
			_systems.Cleanup();
		}

		public void Stop() {
			if (!_started) return;

			_started = false;

			_systems.DeactivateReactiveSystems();
			_systems.ClearReactiveSystems();
			_systems.TearDown();
		}

		public void Destroy() {
			Stop();
			_systems = null;
		}

#if UNITY_EDITOR && !ENTITAS_DISABLE_VISUAL_DEBUGGING
		public override string ToString() => $"[#{Id} count={_systems?.totalSystemsCount ?? 0}]";
#else
		public override string ToString() => $"[#{Id}]";
#endif
	}

}