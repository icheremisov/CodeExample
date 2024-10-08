using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using XLib.UI.Controls;
using XLib.Unity.Cameras;
using XLib.Unity.Scene.ScreenTransition;

namespace Client.Core.Common.UI.LoadingScreen {

	public class LoadingView : MonoBehaviour {
		[SerializeField, Required] private CameraLayer _cameraLayer;
		[SerializeField, Required] private GameObject _viewRoot;
		[SerializeField, Required] private GameObject _content;
		[SerializeField, Required] private UIProgressBar _slider;
		[SerializeField, Required] private TMP_Text _lbVersion;
		[SerializeField, Required] private GameObject _loadingTextPanel;
		[SerializeField, Required] private ScreenTransitionImage _transitionImage;
		[SerializeField, Required] private float _transitionDuration = 0.5f;
		
		public bool IsVisible => _viewRoot != null && _viewRoot.activeSelf;
		public bool IsBarVisible => _content != null && _content.activeSelf;
		
		public async UniTask Show(bool force) {
			if (_viewRoot.activeSelf) return;
			
			if (force) {
				_cameraLayer.SetFullScreen(true);
				_viewRoot.SetActive(true);
			}
			else {
				_viewRoot.SetActive(true);
				_cameraLayer.SetFullScreen(false);
				_transitionImage.Dissolve = 0f;

				await DOTween.To(() => _transitionImage.Dissolve,
					(v) => _transitionImage.Dissolve = v, 1f, _transitionDuration)
					.SetUpdate(true)
					.OnComplete(() => {
						_cameraLayer.SetFullScreen(true);
					});
			}
		}
		
		public async UniTask Hide(bool force) {
			if (!_viewRoot.activeSelf) return;
			
			if (force) {
				_viewRoot.SetActive(false);
				_cameraLayer.SetFullScreen(true);
			}
			else {
				_cameraLayer.SetFullScreen(false);
				
				_transitionImage.Dissolve = 1f;
				
				await DOTween.To(() => _transitionImage.Dissolve,
						(v) => _transitionImage.Dissolve = v, 0f, _transitionDuration)
					.SetUpdate(true)
					.OnComplete(() => {
						_viewRoot.SetActive(false);
						_cameraLayer.SetFullScreen(true);
					});
			}
		}
		
		public void SetContentVisible(bool v) => _content.SetActive(v);

		public void SetProgress(float v) => _slider.SetProgress(v, duration: 1.0f);

		public void SetVersion(string v) => _lbVersion.text = v.Replace("\n", "; ");
	}

}