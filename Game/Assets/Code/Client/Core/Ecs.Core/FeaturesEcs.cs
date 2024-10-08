using Client.Core.Ecs.Types;

namespace Client.Ecs.Core {

	public static class FeaturesEcs {

		public static class Feature {

			public static readonly FeatureId Global = new("Global");
			public static readonly FeatureId Level = new("Level");
		}

	}

}