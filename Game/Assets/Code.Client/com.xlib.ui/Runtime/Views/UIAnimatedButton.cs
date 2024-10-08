using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

namespace Client.Meta.UI.Common {

	public class UIAnimatedButton : MonoBehaviour {
		[SerializeField, Required] private Button _button;
		[SerializeField, Required, ListDrawerSettings(DefaultExpandedState = true)] private Transform[] _rotate;
		[SerializeField, Required, ListDrawerSettings(DefaultExpandedState = true)] private Graphic[] _fade;
		[SerializeField] private bool _stopOnDisable = true;

		[Header("Animation")]
		[SerializeField] private float _fadeTimeSec = 0.1f;
		[SerializeField] private float _rotateSpeed = 30;

		private float _animPower;
		private float _animDirection;
		public ButtonClickedEvent OnClick => _button.onClick;

		private void Awake() {
			_rotate = _rotate.Where(x => x != null).ToArray();
			_fade = _fade.Where(x => x != null).ToArray();
		}

		private void OnDisable() {
			if (_stopOnDisable) SetAnimatingNow(false);
		}

		[PropertySpace]
		[Button]
		public void SetAnimatingNow(bool isAnimating) {
			_animDirection = isAnimating ? 1 : 0;
			_animPower = isAnimating ? 1 : 0;
			_fade.ForEach(x => x.color = x.color.SetAlpha(isAnimating ? 0 : 1));
			_rotate.ForEach(x => x.localRotation = Quaternion.identity);
		}

		[Button]
		public void StartAnim() => _animDirection = 1;

		[Button]
		public void StopAnim() => _animDirection = -1;

		public void RunAnimation(bool isAuto) {
			if (isAuto)
				StartAnim();
			else
				StopAnim();
		}

		private void Update() {
			if (_animDirection == 0) return;

			var dt = Time.unscaledDeltaTime;

			Debug.Assert(_fadeTimeSec > 0);
			var speed = 1.0f / _fadeTimeSec;

			_animPower += dt * _animDirection * speed;
			_animPower = Mathf.Clamp01(_animPower);
			if (_animPower <= 0) _animDirection = 0;

			var rotateSpeed = _animPower * _rotateSpeed * dt;

			_fade.ForEach(x => x.color = x.color.SetAlpha(1 - _animPower));
			_rotate.ForEach(x => x.transform.Rotate(Vector3.forward, rotateSpeed, Space.Self));
		}
	}

}