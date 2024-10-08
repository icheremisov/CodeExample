using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using XLib.Core.CommonTypes;
using XLib.Core.RandGen;

namespace XLib.UI.Controls {

	public class UISpriteSheetAnimation : BaseMeshEffect {
		private static readonly SystemRandom Rnd = new SystemRandom();

		[SerializeField] private Vector2Int _dimensions = new Vector2Int(2, 2);
		[SerializeField] private float _animTimeSec = 1;
		[SerializeField] private bool _autostart = false;

		[Space]
		[SerializeField] private bool _repeat = true;
		
		[Space]
		[SerializeField, ShowIf(nameof(_repeat))] private bool _repeatPause;
		[SerializeField, HideLabel, ShowIf("@(_repeatPause && _repeat)")] private RangeF _repeatPauseSec = new RangeF(0.5f, 1.0f);

		private readonly List<UIVertex> _vertices = new(6);

		private Image _image;
		private Image Owner => _image != null ? _image : _image = this.GetComponent<Image>();

		private enum State {
			Stopped,
			Pause,
			Animation,
		}

		private State _state = State.Stopped;
		private float _animTime;
		private int _currentFrame;
		private float _pauseTime;

		[Button]
		public void Play() {
			_state = State.Animation;
			_animTime = 0;

			_currentFrame = 0;
			RequestRedraw();
		}

		[Button]
		public void Stop() {
			_state = State.Stopped;

			_currentFrame = -1;
			RequestRedraw();
		}

		private void Pause() {
			_state = State.Pause;
			_pauseTime = _repeatPauseSec.GetRandom(Rnd);

			_currentFrame = -1;
			RequestRedraw();
		}

		protected override void OnEnable() {
			base.OnEnable();
			if (_state == State.Stopped && _autostart && Application.isPlaying)
				Play();
			else
				Stop();
		}

		protected override void OnDisable() {
			Stop();
			base.OnDisable();
		}

		private void Update() {
			switch (_state) {
				case State.Stopped: break;

				case State.Pause:
					StatePause();
					break;

				case State.Animation:
					StateAnimation();
					break;

				default: throw new ArgumentOutOfRangeException();
			}
		}

		private void StatePause() {
			_pauseTime -= Time.unscaledDeltaTime;
			if (_pauseTime <= 0) Play();
		}

		private void StateAnimation() {
			if (_animTimeSec <= 0) return;

			var numFrames = _dimensions.x * _dimensions.y;
			if (numFrames <= 0) return;

			_animTime += Time.unscaledDeltaTime;
			var frameTime = _animTimeSec / numFrames;
			var passedFrames = Mathf.FloorToInt(_animTime / frameTime);
			if (passedFrames <= 0) return;

			_animTime -= passedFrames * frameTime;

			if (_repeat) {
				if (!_repeatPause) {
					_currentFrame = (_currentFrame + passedFrames) % numFrames;
				}
				else {
					_currentFrame += passedFrames;
					if (_currentFrame >= numFrames) Pause();
				}
			}
			else {
				if (_currentFrame + passedFrames >= numFrames)
					Stop();
				else
					_currentFrame += passedFrames;
			}

			RequestRedraw();
		}

#if UNITY_EDITOR
		[Button]
		private void NextFrame() {
			var numFrames = _dimensions.x * _dimensions.y;
			if (numFrames <= 0) return;
			_currentFrame = (_currentFrame + 1) % numFrames;
			RequestRedraw();
		}

		[Button]
		private void SetFrame(int frame) {
			_currentFrame = frame;
			RequestRedraw();
		}
#endif

		private void RequestRedraw() {
			if (!Owner) return;
			Owner.SetVerticesDirty();
		}

		public override void ModifyMesh(VertexHelper vertexHelper) {
			if (!IsActive()) return;

			vertexHelper.GetUIVertexStream(_vertices);
			ModifyVertices();

			vertexHelper.Clear();
			vertexHelper.AddUIVertexTriangleStream(_vertices);
		}

		private static readonly Vector3[] Pos = new[] {
			new Vector3(-1, -1),
			new Vector3(1, -1),
			new Vector3(1, 1),
			new Vector3(-1, -1),
			new Vector3(1, 1),
			new Vector3(-1, 1),
		};
		private static readonly Vector2[] UV = new[] {
			new Vector2(0, 0),
			new Vector2(1, 0),
			new Vector2(1, 1),
			new Vector2(0, 0),
			new Vector2(1, 1),
			new Vector2(0, 1),
		};

		private void ModifyVertices() {
			if (_currentFrame < 0) {
				_vertices.Clear();
				return;
			}

			if (!Owner) return;

			var color = (Color32)Owner.color;
			var posScale = Owner.rectTransform.rect.size * 0.5f;

			var frame = new Vector2Int(_currentFrame % _dimensions.x, _dimensions.x - _currentFrame / _dimensions.x - 1);

			var frameSize = new Vector2(1.0f / _dimensions.x, 1.0f / _dimensions.y);
			var framePos = frameSize * frame;

			_vertices.Clear();
			for (var index = 0; index < Pos.Length; index++) {
				var v = new UIVertex();
				v.position = Pos[index] * posScale;
				v.uv0 = UV[index] * frameSize + framePos;
				v.color = color;
				_vertices.Add(v);
			}
		}
	}

}