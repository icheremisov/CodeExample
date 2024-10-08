using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using XLib.Core.CommonTypes;
using XLib.Core.Utils;

namespace Client.Cheats.Internal {

	public static class CheatGui {
		public const string StandardDateTimeFormat = "dd.MM.yyyy HH:mm";

		public static bool Input(string previous, out string changed, bool withApprove = true) {
			var control = GUIUtility.GetControlID(FocusType.Passive); // next control
			var wrapper = GUIUtility.GetStateObject(TypeOf<StringEditTempData>.Raw, control) as StringEditTempData;
			if (wrapper.initial != previous) {
				wrapper.initial = wrapper.value = previous;
			}

			var prev = GUI.color;
			var lastControl = control + 1;

			if (previous != wrapper.value) GUI.color = Color.cyan;
			var value = GUILayout.TextField(wrapper.value);
			GUI.color = prev;

			if (withApprove) {
				changed = wrapper.value = value;
				var approve = (lastControl == GUIUtility.keyboardControl &&
					Event.current.keyCode is KeyCode.KeypadEnter or KeyCode.Return);
				if (wrapper.value != previous && lastControl != GUIUtility.keyboardControl) {
					wrapper.initial = wrapper.value = changed;
					approve = true;
				}

				approve |= GUILayout.Button("Set", GUILayout.Width(30));

				return approve;
			}
			else {
				var result = value != wrapper.value;
				changed = wrapper.value = value;
				return result;
			}
		}

		public static bool Input(string label, string previous, out string changed) {
			using (HorizontalGroup(label)) {
				if (Input(previous, out changed)) return true;
			}

			changed = previous;
			return false;
		}

		public static bool Input(string label, float previous, out float changed) {
			using (HorizontalGroup(label)) {
				if (Input(previous.ToString(CultureInfo.InvariantCulture), out var changedValue) &&
					float.TryParse(changedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out changed))
					return true;
			}

			changed = previous;
			return false;
		}

		public static bool Input(string label, int previous, out int changed) {
			using (HorizontalGroup(label)) {
				if (Input(previous.ToString(CultureInfo.InvariantCulture), out var changedValue) && int.TryParse(changedValue, out changed)) return true;
			}

			changed = previous;
			return false;
		}

		public static bool Input(string label, Timestamp previous, out Timestamp changed) {
			var time = previous.ToLocalDateTime.ToString(StandardDateTimeFormat);
			if (Input(label, time, out var newTime) &&
				DateTime.TryParseExact(newTime, StandardDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var newDateTime)) {
				changed = new Timestamp(newDateTime);
				return true;
			}

			changed = previous;
			return false;
		}

		public static bool InputDelta(string label, out int changed, params int[] values) {
			using (HorizontalGroup(label)) {
				foreach (var delta in values) {
					if (!GUILayout.Button((delta > 0) ? $"+{delta}" : delta.ToString())) continue;
					changed = delta;
					return true;
				}
			}

			changed = 0;
			return false;
		}

		public static bool Input(string label, int previous, out int changed, RangeInt range, params int[] values) => Input(label, null, previous, out changed, range, values);

		public static bool Input(string label, Sprite sprite, int previous, out int changed, RangeInt range, params int[] values) {
			using (HorizontalGroup(label)) {
				Sprite(30, 30, sprite);
				if (Input(previous.ToString(), out var changedS) && int.TryParse(changedS, out changed)) {
					changed = Mathf.Clamp(changed, range.start, int.MaxValue);
					return true;
				}

				if (GUILayout.Button(range.start.ToString())) {
					changed = range.start;
					return true;
				}

				var end = range.end;
				var hasLimit = end > 0;
				if (end <= 0) end = int.MaxValue;

				foreach (var delta in values) {
					if (previous + delta <= end && GUILayout.Button($"+{delta}")) {
						changed = previous + delta;
						return true;
					}
				}

				if (hasLimit && GUILayout.Button($"max({range.end})")) {
					changed = range.end;
					return true;
				}
			}

			changed = previous;
			return false;
		}

		public static bool Input(int previous, out int changed, bool withApprove = true) {
			if (Input(previous.ToString(), out var changedS, withApprove) && int.TryParse(changedS, out changed)) return true;
			changed = previous;
			return false;
		}

		private static readonly GUILayoutOption[] LabelWidth = { GUILayout.MinWidth(100) };

		public static IDisposable HorizontalGroup(string label = null, params GUILayoutOption[] options) {
			GUILayout.BeginHorizontal(options);
			if (label != null) GUILayout.Label(label, LabelWidth);
			return HorizontalGroupUsing.Instance;
		}

		public static EnableScopeUsing EnableScope(bool enabled) => new(enabled);

		public struct EnableScopeUsing : IDisposable {
			private readonly bool _enabled;

			public EnableScopeUsing(bool enabled) {
				_enabled = GUI.enabled;
				GUI.enabled = enabled;
			}

			public void Dispose() => GUI.enabled = _enabled;
		}

		public static IDisposable VerticalGroup(string label = null, params GUILayoutOption[] options) {
			GUILayout.BeginVertical(options);
			if (label != null) GUILayout.Label(label, LabelWidth);
			return VerticalGroupUsing.Instance;
		}
		
		public static IDisposable AutoGroup(bool horizontal, string label = null, params GUILayoutOption[] options) => 
			horizontal ? HorizontalGroup(label, options) : VerticalGroup(label, options);

		public static IDisposable HorizontalGroup(Sprite sprite, string label) {
			GUILayout.BeginHorizontal();
			Sprite(30, 30, sprite);
			GUILayout.Label(label, LabelWidth);
			return HorizontalGroupUsing.Instance;
		}

		private class HorizontalGroupUsing : IDisposable {
			public static readonly HorizontalGroupUsing Instance = new();
			public void Dispose() => GUILayout.EndHorizontal();
		}

		private class VerticalGroupUsing : IDisposable {
			public static readonly VerticalGroupUsing Instance = new();
			public void Dispose() => GUILayout.EndVertical();
		}

		private class StringEditTempData {
			public string initial;
			public string value;
		}

		public static TEnum Flags<TEnum>(string label, TEnum value, Func<TEnum, string> getName = null, bool horizontal = true) where TEnum : unmanaged, Enum {
			getName ??= e => e.ToString();
			using (AutoGroup(horizontal, label)) {
				var isFlag = Enums.IsFlagEnum<TEnum>();
				foreach (var v in Enums.Values<TEnum>()) {
					if (isFlag && v.AsLong() == 0) continue;
					var active = isFlag ? value.Has(v) : v.AsLong() == value.AsLong();
					var nvalue = GUILayout.Toggle(active, getName(v), GUI.skin.button);
					if (nvalue != active) value = isFlag ? value.With(v, nvalue) : v;
				}
			}

			return value;
		}

		public static void Sprite(Rect position, Sprite sprite) {
			if (sprite == null) return;
			var rc = sprite.GetTextureRect();
			var aspect = rc.width * sprite.texture.width / (rc.height * sprite.texture.height);

			var size =
				(position.size.y < position.size.x / aspect)
					? new Vector2(position.size.y * aspect, position.size.y)
					: new Vector2(position.size.x, position.size.x / aspect);
			position = new Rect(position.position, size);
			// GUI.Box(position, string.Empty);
			GUI.DrawTextureWithTexCoords(position, sprite.texture, rc);
		}

		public static void Sprite(float width, float height, Sprite sprite) => Sprite(GUILayoutUtility.GetRect(width, height), sprite);

		public static void Sprite(Sprite sprite) => Sprite(GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none), sprite);

		private static Rect GetTextureRect(this Sprite sprite) {
			var spriteTexture = sprite.texture;
			if (!sprite.packed || sprite.packingMode == SpritePackingMode.Rectangle) {
				var rc = sprite.textureRect;
				return new Rect(rc.x / spriteTexture.width, rc.y / spriteTexture.height, rc.width / spriteTexture.width, rc.height / spriteTexture.height);
			}
			else {
				var x1 = float.MaxValue;
				var x2 = float.MinValue;
				var y1 = float.MaxValue;
				var y2 = float.MinValue;

				foreach (var v in sprite.uv) {
					x1 = Mathf.Min(x1, v.x);
					x2 = Mathf.Max(x2, v.x);
					y1 = Mathf.Min(y1, v.y);
					y2 = Mathf.Max(y2, v.y);
				}

				return new Rect(x1, y1, x2 - x1, y2 - y1);
			}
		}

		public static void DrawList<T>(IEnumerable<T> elements, Action<T> draw) {
			foreach (var element in elements) {
				using (new GUILayout.HorizontalScope()) draw(element);
			}
		}

		public static void DrawList<T>(IEnumerable<T> elements, int width, int height, Action<T> draw) {
			foreach (var element in elements) draw(element);
		}

		private class EnumSelectorState {
			public bool Show;
		}

		public static TEnum EnumSelector<TEnum>(string label, TEnum enumValue) where TEnum : unmanaged, Enum {
			var control = GUIUtility.GetControlID(FocusType.Passive);
			var state = (EnumSelectorState)GUIUtility.GetStateObject(TypeOf<EnumSelectorState>.Raw, control);

			GUILayout.BeginHorizontal();
			GUILayout.Label(label);
			if (GUILayout.Button(enumValue.ToString())) state.Show = !state.Show;
			GUILayout.EndHorizontal();

			if (!state.Show) return enumValue;

			GUILayout.BeginVertical();

			foreach (var value in Enums.Values<TEnum>()) {
				if (!GUILayout.Button(value.ToString())) continue;
				state.Show = false;
				return value;
			}

			GUILayout.EndVertical();

			return enumValue;
		}

		private class LazyUpdateData {
			public string data;
			public float time;
			public string label;
		}

		public static void LazyLabel(string label, Func<string> getValue, bool force = false) {
			var refresh = force;
			using var _ = new GUILayout.VerticalScope();
			using (new GUILayout.HorizontalScope()) {
				var color = GUI.color;
				GUI.color = Color.yellow;
				GUILayout.Label(label);
				GUILayout.FlexibleSpace();
				GUI.color = Color.green;
				if (GUILayout.Button("Refresh")) refresh = true;
				GUI.color = Color.cyan;
				if (GUILayout.Button("Copy")) GUIUtility.systemCopyBuffer = getValue();
				GUI.color = color;
			}

			var control = GUIUtility.GetControlID(FocusType.Passive); // next control
			var wrapper = GUIUtility.GetStateObject(TypeOf<LazyUpdateData>.Raw, control) as LazyUpdateData;
			if (refresh) wrapper.data = null;
			if (wrapper.label != label || wrapper.data == null || (wrapper.time + 3 < Time.time)) {
				wrapper.data = getValue();
				wrapper.time = Time.time;
				wrapper.label = label;
			}

			GUILayout.TextArea(wrapper.data);
		}

		// public class DropdownData {
		// 	public int ControlId { get; set; }
		// 	public Rect Rect { get; set; }
		// 	public List<(object, string)> List { get; set; }
		// 	public List<int> Select { get; set; }
		// 	public bool Show { get; set; }
		// }
		//
		// private static DropdownData CurrentDropdown;
		// public static IEnumerable<T> DropdownMulti<T>(string name, IEnumerable<T> select, Func<IEnumerable<T>> allList, Func<T, string> getName, bool search) {
		// 	var control = GUIUtility.GetControlID(FocusType.Passive); // next control
		// 	var wrapper = GUIUtility.GetStateObject(TypeOf<DropdownData>.Raw, control) as DropdownData;
		// 	if (GUILayout.Button(name)) {
		// 		wrapper.ControlId = control;
		// 		wrapper.Rect = GUILayoutUtility.GetLastRect();
		// 		wrapper.List = allList().Select(arg => ((object)arg, getName(arg))).ToList();
		// 		wrapper.Select = select.Select(arg => wrapper.List.IndexOf(tuple => tuple.Item1 == (object)arg)).ToList();
		// 		wrapper.Show = true;
		// 		CurrentDropdown = wrapper;
		// 	}
		// 	else {
		// 		if (CurrentDropdown == wrapper && !wrapper.Show) {
		//
		// 			return wrapper.Select.Select(i => wrapper.List[i].Item1).ToList();
		// 		}
		// 		return select;
		// 	}
		// }
	}

}