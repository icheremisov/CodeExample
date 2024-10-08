using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Unity.Scene.ScreenTransition;

namespace XLib.UI.TransitionScreen {

	public class TransitionView : MonoBehaviour {
		[SerializeField, Required] private GameObject _viewRoot;
		[SerializeField, Required] private GameObject _content;
		[SerializeField, Required] private ScreenTransitionImage _transitionImage;
		[SerializeField, Required] private float _transitionDuration = 0.5f;
		
		public bool IsVisible => _viewRoot != null && _viewRoot.activeSelf && _content.activeSelf;

		public void Prepare() {
			_viewRoot.SetActive(true);
			_content.SetActive(false);
		}
		
		public async UniTask Show(bool force, float delay = 0f) {
			if (IsVisible) return;
			
			_content.SetActive(true);
			
			if (force) {
				_viewRoot.SetActive(true);
			}
			else {
				_viewRoot.SetActive(true);
				_transitionImage.Dissolve = 0f;

				if (delay > 0) await UniEx.DelaySec(delay);
				await DOTween.To(() => _transitionImage.Dissolve,
					(v) => _transitionImage.Dissolve = v, 1f, _transitionDuration)
					.SetUpdate(true);
			}
		}
		
		public async UniTask Hide(bool force, float delay = 0f) {
			if (!_viewRoot.activeSelf) return;
			
			if (force) {
				_viewRoot.SetActive(false);
			}
			else {
				_transitionImage.Dissolve = 1f;
				
				if (delay > 0) await UniEx.DelaySec(delay);
				await DOTween.To(() => _transitionImage.Dissolve,
						(v) => _transitionImage.Dissolve = v, 0f, _transitionDuration)
					.SetUpdate(true)
					.OnComplete(() => {
						_viewRoot.SetActive(false);
					});
			}
		}
		
	}

}