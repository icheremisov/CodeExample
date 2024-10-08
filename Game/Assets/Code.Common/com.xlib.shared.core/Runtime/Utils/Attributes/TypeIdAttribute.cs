using System;

namespace XLib.Core.Utils.Attributes {

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
	public class TypeIdAttribute : Attribute {
		private readonly Guid _id;
		public Guid Id => _id;

		public TypeIdAttribute(string guid) => _id = new Guid(guid);
	}

}