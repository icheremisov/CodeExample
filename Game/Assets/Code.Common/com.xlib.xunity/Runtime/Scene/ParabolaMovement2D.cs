using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Unity.Scene {

	public class ParabolaMovement2D : MonoBehaviour {

		[Header("Parabola"), SerializeField, Required]
		private Transform _start;

		[SerializeField, Required] private Transform _end;
		[SerializeField, Required] private float _height = 50;

		[Header("Animation"), SerializeField]
		private Transform _movableObject;

		[SerializeField, Required] private float _duration = 10;
		[SerializeField, Required] private bool _autoplay = true;
		[SerializeField, Required] private Ease _ease = Ease.Linear;

		private Tween _tween;

		private void OnEnable() {
			if (_autoplay) Play();
		}

		private void OnDisable() {
			Stop();
		}

		[Button]
		public void Play() {
			if (!Application.isPlaying) return;

			Play(_start.position, _end.position);
		}

		public void Play(Vector2 worldStart, Vector2 worldEnd) {
			Stop();

			var tm = _movableObject != null ? _movableObject : transform;

			_tween = tm.DOParabola2DMove(worldStart, worldEnd, _height, _duration).SetEase(_ease);
		}

		public void Stop() {
			_tween?.Kill();
			_tween = null;
		}

	}

}