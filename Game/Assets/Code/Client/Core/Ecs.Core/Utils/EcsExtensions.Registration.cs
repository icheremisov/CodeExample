using System;
using System.Linq;
using Client.Core.Ecs.Configs;
using Client.Core.Ecs.Types;
using Client.Ecs.Core.Internal;
using Zenject;

// ReSharper disable once CheckNamespace
public static partial class EcsExtensions {
	/// <summary>
	///     register feature in Ecs runner and add all systems from it
	/// </summary>
	public static void RegisterEcsFeature(this DiContainer container, FeatureId id, Feature systems = null) {
		var ecsConfig = container.Resolve<EcsConfig>();
		var featureConfig = ecsConfig.Features.FirstOrDefault(x => x.Id == id);

		if (featureConfig == null) throw new Exception($"Unknown ECS feature '{id}' - you must add feature with 'ECS/Add Feature' menu!");

		foreach (var systemType in featureConfig.Types) container.BindInterfacesAndSelfTo(systemType).AsSingle();

		container.BindInterfacesTo<EcsFeatureLifetime>().FromMethod(_ => new EcsFeatureLifetime(container, featureConfig, systems));
	}
}