using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Unity.Core;

namespace Client.Enviroment {

	/// <summary>
	/// persistent animation for battle camera 
	/// </summary>
	[RequireComponent(typeof(DOTweenAnimation))]
	public class CameraTweenPersistentAnimation : MonoBehaviour, ICameraPersistentAnimation {
		private DOTweenAnimation _animation;

		private bool _cameraSet;

		private void Awake() {
			_animation = this.GetExistingComponent<DOTweenAnimation>();

			if (GameLoader.Mode == GameLoadingMode.GameFromStart && !_cameraSet) {
				_animation.autoPlay = false;
				if (!_cameraSet) _animation.DOKill();
			}
		}

		public void SetCamera(ICameraPersistentAnimation.CameraType type, Camera cam) {
			if (type == ICameraPersistentAnimation.CameraType.Main) {
				_cameraSet = true;
				_animation.SetAnimationTarget(cam);
				_animation.CreateTween(true, true);
			}
		}
		
#if UNITY_EDITOR
		private const string MainCameraName = "Main Battle Camera";

		[Button]
		public void SetupEditorPreview() {
			var cam = FindObjectsOfType<Camera>(true)
				.FirstOrDefault(x => x.name == MainCameraName);
			if (cam == null) {
				Debug.LogError($"Cannot find camera with name '{MainCameraName}'");
				return;
			}

			var anim = this.GetExistingComponent<DOTweenAnimation>();
			
			anim.target = cam;
			anim.targetGO = cam.gameObject;
			anim.tweenTargetIsTargetGO = true;
			anim.targetType = DOTweenAnimation.TypeToDOTargetType(cam.GetType());
			UnityEditor.EditorUtility.SetDirty(anim);
		}
#endif
	}

}