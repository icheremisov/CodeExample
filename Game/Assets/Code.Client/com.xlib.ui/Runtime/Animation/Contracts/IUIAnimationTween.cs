using DG.Tweening;
using UnityEngine;

namespace XLib.UI.Animation.Contracts {

	public interface IUIAnimationTween {
		void Prepare(UIAnimationTweenSettings settings, GameObject gameObject);
		Tween Create(UIAnimationTweenSettings settings, GameObject gameObject);
	}

}