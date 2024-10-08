using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLib.Unity.Utils {

	public static class GuiEx {
		public readonly struct UsingCallback : IDisposable {
			public static UsingCallback None = new(null);
			private readonly Action _disposeCallback;
			public UsingCallback(Action disposeCallback) => _disposeCallback = disposeCallback;
			public void Dispose() => _disposeCallback?.Invoke();
		}

		public static UsingCallback BackgroundColor(Color color) {
			var prev = GUI.backgroundColor;
			GUI.backgroundColor = color;
			return new UsingCallback(() => GUI.backgroundColor = prev);
		}

		public static UsingCallback ContentColor(Color color) {
			var prev = GUI.contentColor;
			GUI.contentColor = color;
			return new UsingCallback(() => GUI.contentColor = prev);
		}

		public static UsingCallback Color(Color color) {
			var prev = GUI.color;
			GUI.color = color;
			return new UsingCallback(() => GUI.color = prev);
		}

		public static void Label(string label, Color color) {
			using var _ = Color(color);
			GUILayout.Label(label);
		}

		public static void Box(string label, Color color) {
			using var _ = Color(color);
			GUILayout.Box(label);
		}

		public static bool Button(string label, Color color) {
			using var _ = Color(color);
			return GUILayout.Button(label);
		}
		
		public static bool Button(GUIContent context, Color color) {
			using var _ = Color(color);
			return GUILayout.Button(context);
		}
		
		
		public static int Toolbar(int select, IEnumerable<string> labels, Color color) {
			using var c = BackgroundColor(new Color(0.3f, 0.5f, 0));
			using var s = new GUILayout.HorizontalScope(GUI.skin.box);
			using var c2 = Color(color);
			var i = -1;
			foreach (var label in labels) {
				++i;
				if(select == i) GUILayout.Label(label);
				else if(GUILayout.Button(label)) select = i;
			}
			return select;
		}
		
		public static void Error(string label) {
        	GUILayout.Box(label, "error");
        }

		public static void Tooltip(string message) => 
			GUILayout.Button(new GUIContent("i", message), GUILayout.Width(10));
	}

}