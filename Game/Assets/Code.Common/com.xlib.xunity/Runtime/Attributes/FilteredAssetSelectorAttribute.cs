using System;
using UnityEngine;

namespace XLib.Unity.Attributes {

	[AttributeUsage(AttributeTargets.Field)]
	public class FilteredAssetSelectorAttribute : PropertyAttribute {

		/// <summary>
		/// filter type or method name for execution. method must be with signature 'System.Type Method()'
		/// examples:
		///		FilteredAssetSelector(Filter = typeof(CustomType))
		///		FilteredAssetSelector(Filter = "Method")
		/// </summary>
		public object Filter { get; set; }

		public FilteredAssetSelectorAttribute(object filter = null) {
			Filter = filter;
		}

	}

}