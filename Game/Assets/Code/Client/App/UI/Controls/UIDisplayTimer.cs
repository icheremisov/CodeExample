using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using XLib.Core.CommonTypes;
using XLib.UI.Controls;
using Zenject;

namespace Client.App.UI.Controls {

	public class UIDisplayTimer : MonoBehaviour {

		private static readonly WaitForSeconds WaitFor1S = new(1f);
		private static readonly int Expiring = Animator.StringToHash("Expiring");

		[SerializeField]
		private UIProgressBar _progressBar;

		[SerializeField]
		private Animator _timerAnimator;

		private TimeRange _range;
		private bool _isActive;
		private bool _disableTick;
		private bool _playExpiringAnimation;
		private GlobalContext _globalContext;

		public bool IsActive => _isActive;

		[UsedImplicitly] 
		public event Action OnTimer;

		[Inject]
		private void Construct(GlobalContext globalContext) => _globalContext = globalContext;

		public TimeRange Range {
			get => _range;
			set {
				_range = value;
				_disableTick = false;

				if (isActiveAndEnabled) {
					_isActive = false;
					StopAllCoroutines();

					if (!UpdateView()) {
						_isActive = true;
						this.StartThrowingCoroutine(TickCoroutine());
					}
				}
			}
		}

		public void SetExpiringState(bool expiring) {
			_playExpiringAnimation = expiring;
			UpdateAnimatorState();
		}

		public void StopAt(TimeRange range) {
			_range = range;
			_disableTick = true;

			Stop();
			// _progressBar.SetProgress(range, ~_globalContext.metaHost);
		}

		public void Stop() {
			_isActive = false;
			StopAllCoroutines();
		}

		public Timestamp Target { get => _range.End; set => Range = new TimeRange(value, value); }

		private void OnEnable() {
			UpdateAnimatorState();
			if (_disableTick) return;
			if (!UpdateView()) {
				_isActive = true;
				this.StartThrowingCoroutine(TickCoroutine());
			}
		}

		private IEnumerator TickCoroutine() {
			while (true) {
				yield return WaitFor1S;
				if (UpdateView()) yield break;
			}
		}

		private bool UpdateView() {
			// if (_globalContext is not { hasMetaHost: true }) return false;
			
			// var time = (~_globalContext.metaHost).CurrentTime;
			// if (_range.End > Timestamp.Null && time >= _range.End) {
			// 	_progressBar.SetProgress((int)_range.Duration, (int)_range.Duration);
			//
			// 	_range = default;
			// 	OnTimer?.Invoke();
			// 	return true;
			// }
			//
			// _progressBar.SetProgress(Range, ~_globalContext.metaHost);
			return false;
		}

		private void UpdateAnimatorState() {
			if (_timerAnimator == null || !gameObject.activeSelf) return;
			_timerAnimator.SetBool(Expiring, _playExpiringAnimation);
			_playExpiringAnimation = false;
		}
	}

}