using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Controls {

	public class UIColorTransition : MonoBehaviour {

		[SerializeField, Required] private Image _image;
		[SerializeField] private float _time = 0.1f;
		[SerializeField] private Color _normalColor = Color.white.ReplaceA(0.25f);
		[SerializeField] private Color _activeColor = Color.white;

		private bool _target;
		private Tween _tween;

		private void SetState(bool active) {
			_tween?.Kill();

			if (_image) _image.color = active ? _activeColor : _normalColor;
		}

		private void OnEnable() {
			if (_tween == null || !_tween.IsPlaying()) SetState(_target);
		}

		private void OnDisable() {
			SetState(_target);
			_tween?.Kill();
			_tween = null;
		}

		public void SetActiveState(bool active, bool instant = false) {
			if (instant) {
				_tween?.Kill();
				SetState(active);
				return;
			}

			if (_target == active) return;
			_target = active;
			
			_tween?.Kill();
			_tween = _image.DOColor(active ? _activeColor : _normalColor, _time)
				.SetEase(Ease.Linear)
				.SetUpdate(true);
		}
	}

}