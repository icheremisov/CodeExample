using System.Collections.Generic;
using XLib.Configs.Sheets.Types;

namespace XLib.Configs.Sheets.Contracts {

	public class ValueValidationRule {
		public ConditionType ConditionType { get; }
		public IEnumerable<object> Values { get; }
		public bool Strict { get; }
		public bool IsRequired { get; }

		public ValueValidationRule(ConditionType conditionType, IEnumerable<object> values, bool strict = false, bool isRequired = false) {
			ConditionType = conditionType;
			Values = values;
			Strict = strict;
			IsRequired = isRequired;
		}
	}

}