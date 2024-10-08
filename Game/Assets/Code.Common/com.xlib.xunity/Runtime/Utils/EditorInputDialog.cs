#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace XLib.Unity.Utils {

	internal class EditorInputDialog : EditorWindow {
		private string _description, _inputText;
		private string _okButton, _cancelButton;
		private bool _initializedPosition = false;
		private Action _onOKButton;
		private bool _shouldClose = false;
		private Vector2 _maxScreenPos;

		/// <summary>
		/// Returns text player entered, or null if player cancelled the dialog.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="description"></param>
		/// <param name="inputText"></param>
		/// <param name="okButton"></param>
		/// <param name="cancelButton"></param>
		/// <returns></returns>
		public static string Show(string title, string description, string inputText, string okButton = "OK", string cancelButton = "Cancel") {
			var maxPos = GUIUtility.GUIToScreenPoint(new Vector2(Screen.width, Screen.height));

			string ret = null;
			var window = CreateInstance<EditorInputDialog>();
			window._maxScreenPos = maxPos;
			window.titleContent = new GUIContent(title);
			window._description = description;
			window._inputText = inputText;
			window._okButton = okButton;
			window._cancelButton = cancelButton;
			window._onOKButton += () => ret = window._inputText;
			window.ShowModal();

			return ret;
		}
		private void OnGUI() {
			var e = Event.current;
			if (e.type == EventType.KeyDown) {
				switch (e.keyCode) {
					case KeyCode.Escape:
						_shouldClose = true;
						e.Use();
						break;

					case KeyCode.Return:
					case KeyCode.KeypadEnter:
						_onOKButton?.Invoke();
						_shouldClose = true;
						e.Use();
						break;
				}
			}

			if (_shouldClose) Close();

			var rect = EditorGUILayout.BeginVertical();

			EditorGUILayout.Space(12);
			EditorGUILayout.LabelField(_description);

			EditorGUILayout.Space(8);
			GUI.SetNextControlName("inText");
			_inputText = EditorGUILayout.TextField("", _inputText);
			GUI.FocusControl("inText");
			EditorGUILayout.Space(12);

			var r = EditorGUILayout.GetControlRect();
			r.width /= 2;
			if (GUI.Button(r, _okButton)) {
				_onOKButton?.Invoke();
				_shouldClose = true;
			}

			r.x += r.width;
			if (GUI.Button(r, _cancelButton)) {
				_inputText = null;
				_shouldClose = true;
			}

			EditorGUILayout.Space(8);
			EditorGUILayout.EndVertical();

			if (rect.width != 0 && minSize != rect.size) 
				minSize = maxSize = rect.size;

			if (_initializedPosition || e.type != EventType.Layout) return;
			_initializedPosition = true;

			var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			mousePos.x += 32;
			if (mousePos.x + position.width > _maxScreenPos.x) mousePos.x -= position.width + 64;
			if (mousePos.y + position.height > _maxScreenPos.y) mousePos.y = _maxScreenPos.y - position.height;

			position = new Rect(mousePos.x, mousePos.y, position.width, position.height);

			Focus();
		}
	}

}
#endif