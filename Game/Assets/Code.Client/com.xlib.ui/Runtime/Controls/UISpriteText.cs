using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using XLib.Core.Utils;

namespace XLib.UI.Controls {

	[RequireComponent(typeof(TextMeshProUGUI)), ExecuteInEditMode]
	public class UISpriteText : MonoBehaviour {

		[SerializeField, OnValueChanged(nameof(UpdateView))] private string _text;

		[Space, SerializeField, Required, OnValueChanged(nameof(UpdateView))]
		private CharMap[] _symbols;

		private readonly StringBuilder _sb = new(64);

		private Dictionary<char, string> _remap;

		private TextMeshProUGUI _view;

		public string Text {
			get => _text;
			set {
				if (_text != value) {
					_text = value;
					UpdateView();
				}
			}
		}

		private void Awake() {
			_view = GetComponent<TextMeshProUGUI>();
		}

		private void UpdateMap() {
			if (!Application.isPlaying) {
				_remap = new Dictionary<char, string>(_symbols.Length);
				foreach (var ch in _symbols) {
					if (ch.ch == '\0') continue;
					if (ch.symbol.IsNullOrEmpty()) continue;

					if (_remap.ContainsKey(ch.ch)) continue;

					_remap.Add(ch.ch, RichTag.Sprite(ch.symbol));
				}
			}
			else
				_remap = _symbols.ToDictionary(x => x.ch, x => RichTag.Sprite(x.symbol));
		}

		private void UpdateView() {
			if (_remap == null || !Application.isPlaying) UpdateMap();

			if (!_view) _view = GetComponent<TextMeshProUGUI>();

			var txt = _text ?? string.Empty;

			foreach (var ch in txt) {
				if (_remap.TryGetValue(ch, out var s))
					_sb.Append(s);
				else
					_sb.Append(ch);
			}

			var remappedText = _sb.ToString();
			_view.text = remappedText;

#if UNITY_EDITOR
			if (!Application.isPlaying) _view.ForceMeshUpdate(true, true);
#endif
			_sb.Clear();
		}

		[Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
		private struct CharMap {

			public char ch;
			public string symbol;

		}

	}

}