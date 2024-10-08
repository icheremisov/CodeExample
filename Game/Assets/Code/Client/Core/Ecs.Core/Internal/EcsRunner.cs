using System;
using System.Collections.Generic;
using System.Linq;
using Client.Core.Ecs.Configs;
using Client.Core.Ecs.Types;
using Client.Ecs.Core.Contracts;
using Entitas;
using UnityEngine;
using Zenject;

namespace Client.Ecs.Core.Internal {

	internal class EcsRunner : IEcsRunner, ITickable, IDisposable {

		private readonly EcsConfig _ecsConfig;
		
		private readonly List<EcsFeature> _features = new(8);
		private readonly Dictionary<FeatureId, EcsFeature> _map = new(8);
		
		private EcsFeatureConfig _config;
		
		public EcsRunner(EcsConfig ecsConfig) {
			_ecsConfig = ecsConfig;
		}
		
		public void Dispose() {
			Debug.Log("[EcsRunner] Dispose");
		
			IsPaused = true;
		
			foreach (var feature in _features) feature.Destroy();
		
			_features.Clear();
			_map.Clear();
		}
		
		public bool IsPaused { get; set; } = true;
		
		public void Add(FeatureId id, params ISystem[] systems) {
			GetOrAddFeature(id).Add(systems);
		}

		public void Set(FeatureId id, Feature systems) {
			GetOrAddFeature(id).Set(systems);
		}

		public void Destroy(FeatureId id) {
			var feature = _map.FirstOrDefault(id);
			if (feature == null) return;
		
			feature.Stop();
			feature.Destroy();
		
			var index = _features.IndexOf(feature);
			Debug.Assert(index >= 0);
		
			_map.Remove(id);
			_features.RemoveAll(x => x.Id == id);
		}
		
		public void Start(FeatureId id) {
			if (!_map.TryGetValue(id, out var feature)) return;
		
			Debug.Log($"[EcsRunner] Start {id}");
		
			feature.Start();
		}
		
		public void ForceUpdate(FeatureId id) {
			if (!_map.TryGetValue(id, out var feature)) return;
		
			Debug.Log($"[EcsRunner] ForceUpdate {id}");
		
			feature.Update(true);
		}
		
		public void Stop(FeatureId id) {
			if (!_map.TryGetValue(id, out var feature)) return;
		
			Debug.Log($"[EcsRunner] Stop {id}");
		
			feature.Stop();
		}
		
		private EcsFeature GetOrAddFeature(FeatureId id) {
			var result = _map.FirstOrDefault(id);
		
			if (result == null) {
				var config = _ecsConfig.Features.FirstOrDefault(x => x.Id == id) ??
					throw new Exception($"Unknown ECS feature '{id}' - you must add feature with 'ECS/Add Feature' menu!");
		
				result = new EcsFeature(config);
				_map.Add(id, result);
				_features.Add(result);
				_features.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));
			}
		
			return result;
		}

		public void Tick() {
			foreach (var feature in _features) {
				feature.Update(false);
			}
		}

	}

}