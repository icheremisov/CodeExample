using System;

namespace Client.Core.Ecs.Attributes {

	[AttributeUsage(AttributeTargets.Class)]
	public class FeatureAttribute : Attribute {

		public FeatureAttribute(string feature) {
			Feature = feature;
		}

		public string Feature { get; }

	}

}