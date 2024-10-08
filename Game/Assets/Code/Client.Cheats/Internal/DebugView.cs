using Client.Cheats.Contracts;
using UnityEngine;
using UnityEngine.EventSystems;
using XLib.UI.Controls;

namespace Client.Cheats.Internal {

	public class DebugView : MonoBehaviour, IBeginDragHandler, IDragHandler {
		private bool _expanded;

		[SerializeField] private GameObject _collapsed;
		[SerializeField] private UIRaycastRedirect _redirect;
		[SerializeField] private GameObject _expandedBg;
		[SerializeField] private GameObject _collapsedView;
		[SerializeField] private GUISkin _skin;

		[SerializeField] private float _width = 700;
		[SerializeField] private float _height = 350;
		[SerializeField] private float _scale;
		private bool _short;
		private ICheatSystem _system;
		private void Awake() {
			_redirect.PointerClick = OnSwitch;
		}

		public void SetCheatSystem(ICheatSystem system) => _system = system;
		private void OnSwitch(PointerEventData eventdata) {
			if (!_expanded)
				_system.Maximize();
			else
				_system.Minimize(false);
		}

		public void OnBeginDrag(PointerEventData eventData) { }

		public void OnDrag(PointerEventData eventData) {
			if (_expanded) _system.Scroll(eventData.delta);
		}

		private void OnGUI() {
			if (!_expanded) {
				var @event = Event.current;
				if (@event.KeyUp(KeyCode.BackQuote)) {
					_short = false;
				}
				else if(@event.KeyDown(KeyCode.BackQuote)) {
					_short = true;
				}

				if (_short) {
					DrawHotkeyPanel();
					if (@event.KeyUp(KeyCode.Alpha1)) 
						_system.Maximize();
				}
			}
			else
				DrawCheatPanel();
		}

		private void DrawHotkeyPanel() {
			GUI.skin = _skin;

			_scale = Mathf.Min(Screen.width / _width, Screen.height / _height);
			var size = new Vector2(Screen.width / _scale, Screen.height / _scale);
			GUI.matrix = Matrix4x4.Scale(new Vector3(_scale, _scale, _scale));
			GUILayout.BeginArea(new Rect(size.x * 0.06f, size.y * 0.05f, size.x * 0.88f, size.y * 0.9f), "Hotkeys", GUI.skin.box);
			GUILayout.Space(20);
			_system?.DoHotkeyGui();
			GUILayout.EndArea();

			GUI.skin = null;

		}

		public void SetHidden(bool hidden) => _collapsedView.SetActive(!hidden);

		public void SetExpanded(bool enable) {
			_expanded = enable;
			if (_expanded)
				_short = false;
			_collapsed.SetActive(!enable);
			_expandedBg.SetActive(enable);
		}

		private void DrawCheatPanel() {
			GUI.skin = _skin;

			_scale = Mathf.Min(Screen.width / _width, Screen.height / _height);
			var size = new Vector2(Screen.width / _scale, Screen.height / _scale);
			GUI.matrix = Matrix4x4.Scale(new Vector3(_scale, _scale, _scale));
			var rect = new Rect(size.x * 0.06f, size.y * 0.05f, size.x * 0.88f, size.y * 0.9f);
			GUILayout.BeginArea(rect);
			_system?.DoGui(rect);
			GUILayout.EndArea();

			GUI.skin = null;
		}
	}

}