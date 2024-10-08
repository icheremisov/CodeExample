using System;
using UnityEngine;

namespace XLib.Core.Utils.Attributes {

	[AttributeUsage(AttributeTargets.Field)]
	public class AssetGroupAttribute : PropertyAttribute {
		public AssetGroupAttribute(string groupName) {
			GroupName = groupName;
		}
		public string GroupName { get; }
	}

}