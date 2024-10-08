using RectEx;
using UnityEditor;
using UnityEngine;
using XLib.Core.CommonTypes;
using XLib.Unity.Extensions;

namespace XLib.Unity.Editors {

	[CustomPropertyDrawer(typeof(Duration))]
	public class DurationDrawer : PropertyDrawer {
		private enum Period
		{
			Seconds = 1,
			Minutes = Duration.MinuteSeconds,
			Hours = Duration.HourSeconds,
			Days = Duration.DaySeconds
		}
        
		public override void OnGUI(Rect position, SerializedProperty rootProp, GUIContent label)
		{
			this.SetTooltip(label);

			var property = rootProp.FindPropertyRelative(nameof(Duration.Value));
			EditorGUI.BeginProperty(position, label, property);

			var rect = EditorGUI.PrefixLabel(position, label);

			var time = property.intValue;
			var period = (Period) 1;
			if (time == 0)
				period = Period.Minutes;
			else if (time % Duration.DaySeconds == 0)
				period = Period.Days;
			else if (time % Duration.HourSeconds == 0)
				period = Period.Hours;
			else if (time % Duration.MinuteSeconds == 0)
				period = Period.Minutes;
			else if (time % 1 == 0)
				period = Period.Seconds;

			PropertyDrawerUtils.ClearIndent();
			var column = rect.Row( 2);
			time = EditorGUI.IntField(column[0], (time / (int) period));
			period = (Period) EditorGUI.EnumPopup(column[1], period);
			property.intValue = time * (int) period;

			PropertyDrawerUtils.EndProperty();
		}
	}

}