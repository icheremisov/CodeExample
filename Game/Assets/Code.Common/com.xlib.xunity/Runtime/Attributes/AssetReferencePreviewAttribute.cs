using System;
using UnityEngine;

namespace XLib.Unity.Attributes {

	[AttributeUsage(AttributeTargets.Field)]
	public class AssetReferencePreviewAttribute : PropertyAttribute {
		public AssetReferencePreviewAttribute(int size, bool expanded = true, bool inline = false) {
			Size = size;
			Expanded = expanded;
			Inline = inline;
		}

		public float Size { get; }
		public bool Expanded { get; }
		public bool Inline { get; }
	}

}