using System;
using UnityEngine;

namespace XLib.Configs.Sheets.Contracts {

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class NotInRangeAttribute : PropertyAttribute {
		public float Min { get; }
		public float Max { get; }

		public NotInRangeAttribute(float min, float max) {
			Min = min;
			Max = max;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class ExceptValueAttribute : PropertyAttribute {
		public float ValueToExcept { get; }

		public ExceptValueAttribute(float valueToExcept) {
			ValueToExcept = valueToExcept;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class StringContainsAttribute : PropertyAttribute {
		public string Substring { get; }

		public StringContainsAttribute(string substring) {
			Substring = substring;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class StringNotContainsAttribute : PropertyAttribute {
		public string Substring { get; }

		public StringNotContainsAttribute(string substring) {
			Substring = substring;
		}
	}

}