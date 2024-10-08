using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Client.Cheats.Internal {

	public static class CheatHelper {
		private static Regex _regex = new Regex("\\B([A-Z]+)");
		public static string PrettyName(this string name) => 
			CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_regex.Replace(name, " $1"));

		public static GUIContent PrettyContent(this string name) => new(name.PrettyName());


		public static bool KeyUp(this Event current, KeyCode key, bool useEvent = true)
		{
			var flag = current.type == EventType.KeyUp && current.keyCode == key;
			if (flag & useEvent)
				current.Use();
			return flag;
		}
		
		public static bool KeyDown(this Event current, KeyCode key, bool useEvent = true)
		{
			var flag = current.type == EventType.KeyDown && current.keyCode == key;
			if (flag & useEvent)
				current.Use();
			return flag;
		}
	}

}