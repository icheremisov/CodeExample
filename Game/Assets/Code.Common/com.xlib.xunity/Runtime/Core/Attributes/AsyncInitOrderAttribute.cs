using System;

namespace XLib.Unity.Core.Attributes {

	[AttributeUsage(AttributeTargets.Class)]
	public class AsyncInitOrderAttribute : Attribute {

		public AsyncInitOrderAttribute(int order) {
			Order = order;
		}

		public int Order { get; }

	}

}