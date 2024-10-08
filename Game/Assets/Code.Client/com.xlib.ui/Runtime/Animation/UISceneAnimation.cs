using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.UI.Animation.Contracts;

namespace XLib.UI.Animation {

	public abstract partial class UISceneAnimation : UIAnimationSavableValues, IUISceneAnimation {
		[SerializeField, BoxGroup("Show animation"), LabelText("Enabled")]
		private bool _showAnimationEnabled = true;
		[SerializeField, BoxGroup("Show animation"), LabelText("Animation"), ShowIf("_showAnimationEnabled")]
		private UIAnimation _showAnimation;
		[SerializeField, BoxGroup("Hide animation"), LabelText("Enabled")]
		private bool _hideAnimationEnabled = true;
		[SerializeField, BoxGroup("Hide animation"), LabelText("Animation"), ShowIf("_hideAnimationEnabled")]
		private UIAnimation _hideAnimation;
		[SerializeField] private UINamedAnimation[] _animations;

		protected async UniTask ShowInternal(CancellationToken ct, float delay) {
			if (!_showAnimationEnabled) return;
			await _showAnimation.Play(gameObject, transform, ct, Application.isPlaying, delay);
		}

		protected async UniTask HideInternal(CancellationToken ct, float delay) {
			if (!_hideAnimationEnabled) return;
			await _hideAnimation.Play(gameObject, transform, ct, Application.isPlaying, delay);
		}

		protected async UniTask PlayInternal(string animationName, CancellationToken ct, float delay) {
			if (animationName.IsNullOrEmpty()) return;
			await PlayInternal(animationName.GetHashCode(), ct, delay);
		}

		protected async UniTask PlayInternal(int animationHash, CancellationToken ct, float delay) {
			var uiAnimation = _animations.FirstOrDefault(anim => anim.Hash == animationHash);
			if (uiAnimation == null) return;
			await uiAnimation.Play(gameObject, transform, ct, false, delay);
		}

		public abstract UniTask Show(CancellationToken ct, float delay = 0);
		public abstract UniTask Hide(CancellationToken ct, float delay = 0);
		public abstract UniTask Play(string animationName, CancellationToken ct, float delay = 0);
		public abstract UniTask Play(int animationHash, CancellationToken ct, float delay = 0);
		
		public override void SaveStartValues() {
			if (_showAnimationEnabled) SaveStartValues(_showAnimation.Parts);
			if (_hideAnimationEnabled) SaveStartValues(_hideAnimation.Parts);
			_animations.ForEach(a => SaveStartValues(a.Parts));
		}
		
		public override void ApplyStartValues() {
			if (_showAnimationEnabled) ApplyStartValues(_showAnimation.Parts);
			if (_hideAnimationEnabled) ApplyStartValues(_hideAnimation.Parts);
			_animations.ForEach(a => ApplyStartValues(a.Parts));
		}
	}

}