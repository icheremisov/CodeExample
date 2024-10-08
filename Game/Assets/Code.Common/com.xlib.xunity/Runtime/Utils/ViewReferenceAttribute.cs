using System;

namespace XLib.Unity.Utils {

	[AttributeUsage(AttributeTargets.Field)]
	public class ViewReferenceAttribute : Attribute {

		public string SearchMask { get; set; }

		public ViewReferenceAttribute(string searchMask = null) {
			SearchMask = searchMask;
		}

	}

}