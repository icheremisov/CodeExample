using System;
using Client.Core.Ecs.Configs;
using Client.Ecs.Core.Contracts;
using Entitas;
using Zenject;

namespace Client.Ecs.Core.Internal {

	internal class EcsFeatureLifetime : IInitializable, IDisposable {

		private readonly DiContainer _container;
		private readonly EcsFeatureConfig _featureConfig;
		private readonly Feature _systems;

		public EcsFeatureLifetime(DiContainer container, EcsFeatureConfig featureConfig, Feature systems = null) {
			_container = container;
			_featureConfig = featureConfig;
			_systems = systems;
		}

		public void Dispose() {
			var runner = _container.Resolve<IEcsRunner>();
			runner.Destroy(_featureConfig.Id);
		}

		public void Initialize() {
			var runner = _container.Resolve<IEcsRunner>();

			if (_systems != null) {
				runner.Set(_featureConfig.Id, _systems);
			}
			else {
				var systems = _featureConfig.Types.SelectToArray(systemType => (ISystem)_container.Resolve(systemType));
				runner.Add(_featureConfig.Id, systems);
			}
		}

	}

}