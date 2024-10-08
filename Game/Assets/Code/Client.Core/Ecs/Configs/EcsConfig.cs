using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Client.Core.Ecs.Configs {

	[Serializable]
	public class EcsConfig {

		[SerializeField, InlineProperty, ListDrawerSettings(DefaultExpandedState = true, NumberOfItemsPerPage = 32)]
		private EcsFeatureConfig[] _features;

		private bool _initialized;

		private EcsFeatureConfig[] _runtimeFeatures;

		public EcsFeatureConfig[] Features {
			get {
				Initialize();
				return _runtimeFeatures;
			}
		}

		private void Initialize() {
			if (_initialized) return;

			_initialized = true;

			_runtimeFeatures = _features.Where(x => x != null).ToArray();

			for (var i = 0; i < _runtimeFeatures.Length; i++) _runtimeFeatures[i].SortingOrder = i;
		}

		public void OnEnable() => _initialized = false;
		
	}

}