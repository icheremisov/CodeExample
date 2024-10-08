using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using XLib.Configs.Contracts;
using XLib.Configs.Core;
using XLib.Unity.Extensions;

namespace XLib.Configs.Odin {

	public class GameItemBaseLokiEditor : LokiEditor<GameItemBase> {
		protected class Attribute : LokiAttribute {
			public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<System.Attribute> attributes) {
				if (member.GetAttribute<IgnoreTabDecorator>() != null) return;
				if (member.DeclaringType == null) return;
				
				var tabAttr = member.DeclaringType.GetAttribute<WithTabNameAttribute>();
				if (tabAttr == null) return;
				var tabName = tabAttr.Name;

				foreach (var attribute in attributes.OfType<PropertyGroupAttribute>()) {
					if (!attribute.GroupID.StartsWith($"_A_")) 
						attribute.GroupID = $"_A_/{tabName}/{attribute.GroupID}";
				}

				attributes.Insert(0, new TabGroupAttribute($"_A_", tabName, false, 2));
			}
		}
	}

}