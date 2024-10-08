using System;
using System.Globalization;
using RectEx;
using UnityEditor;
using UnityEngine;
using XLib.Core.CommonTypes;
using XLib.Unity.Extensions;

namespace XLib.Unity.Editors {

	[CustomPropertyDrawer(typeof(Timestamp))]
	public class TimestampDrawer : PropertyDrawer {
		private static readonly float[] _widthes = {30f, 60f, 40f, 10f, 25f, 25f, 25f, 40f, 40f};

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => property.hasMultipleDifferentValues ? 16 : 16 * 3;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (property.hasMultipleDifferentValues) {
				EditorGUI.PropertyField(position, property, GUIContent.none);
				return;
			}

			this.SetTooltip(label);
			EditorGUI.BeginChangeCheck();

			var rect = EditorGUI.PrefixLabel(position, label);
			var column = rect.RowSmart(_widthes, 2f);
			var nrect = position.CutFromRight(rect.width)[0].Column(3);
			var pv = property.FindPropertyRelative(nameof(Timestamp.Value));
			var tm = (new Timestamp(pv.intValue));

			EditorGUI.LabelField(nrect[1], tm.ToString());

			var indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var dateTime = tm.ToDateTime;
			var brect = column[7].Column(2);

			if (GUI.Button(brect[0], "NOW")) {
				dateTime = DateTime.UtcNow;
				GUI.FocusControl(null);
			}

			if (GUI.Button(brect[1], "NEWER")) {
				dateTime = Timestamp.Never.ToDateTime;
				GUI.FocusControl(null);
			}

			var year = dateTime.Year;
			var month = dateTime.Month;
			var day = dateTime.Day;
			var hour = dateTime.Hour;
			var minutes = dateTime.Minute;
			var seconds = dateTime.Second;

			var timeBrect = column[8].Column(2);
			if (GUI.Button(timeBrect[0], "HOUR")) {
				minutes = 0;
				seconds = 0;
				GUI.FocusControl(null);
			}

			if (GUI.Button(timeBrect[1], "DAY")) {
				hour = 0;
				minutes = 0;
				seconds = 0;
				GUI.FocusControl(null);
			}

			IntProperty(ref year, column[2], 1, 1970, 2050);
			month = 1 + EditorGUI.Popup(column[1].Column(new[] { 0.3f, 0.4f, 0.3f })[1], month - 1, CultureInfo.CurrentCulture.DateTimeFormat.MonthNames);
			IntProperty(ref day, column[0], 1, 1, DateTime.DaysInMonth(year, month));
			IntProperty(ref hour, column[4], 1, 0, 23);
			IntProperty(ref minutes, column[5], 1, 0, 59);
			IntProperty(ref seconds, column[6], 1, 0, 59);

			if (EditorGUI.EndChangeCheck()) {
				var timeDate = new DateTime(year, month, day, hour, minutes, seconds, DateTimeKind.Utc);
				pv.intValue = new Timestamp(timeDate).Value;

				property.serializedObject.ApplyModifiedProperties();
			}

			EditorGUI.indentLevel = indentLevel;
		}

		private void IntProperty(ref int value, Rect rect, int step = 1, int min = int.MinValue, int max = int.MaxValue) {
			var column = rect.Column(new[] { 0.3f, 0.4f, 0.3f });

			var str = value.ToString("D2");
			var vstr = EditorGUI.TextField(column[1], str);
			if (vstr != str) int.TryParse(vstr, out value);

			if (GUI.Button(column[0], " ▲", (GUIStyle)"MiniToolbarButton")) {
				value += step;
				if (value > max) value = min;
				GUI.FocusControl(null);
			}
			else if (GUI.Button(column[2], " ▼", (GUIStyle)"MiniToolbarButton")) {
				value -= step;
				if (value < min) value = max;
				GUI.FocusControl(null);
			}

			value = Math.Max(value, min);
			value = Math.Min(value, max);
		}
	}

}