using DG.Tweening;
using UnityEngine;
using XLib.UI.Animation.Contracts;

namespace XLib.UI.Animation.Tweens {

	public class FadeInTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			var canvasGroup = gameObject.GetComponent<CanvasGroup>();
			if (canvasGroup == null) return;
			canvasGroup.alpha = 0;
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			var canvasGroup = gameObject.GetComponent<CanvasGroup>();
			return canvasGroup == null ? null : canvasGroup.DOFade(1f, settings.Duration);
		}
	}
	
	public class FadeOutTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			var canvasGroup = gameObject.GetComponent<CanvasGroup>();
			if (canvasGroup == null) return;
			canvasGroup.alpha = 1;
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			var canvasGroup = gameObject.GetComponent<CanvasGroup>();
			return canvasGroup == null ? null : canvasGroup.DOFade(0f, settings.Duration);
		}
	}
	
	public class MoveTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			gameObject.transform.localPosition = settings.Vector3From;
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			return gameObject.transform.DOLocalMove(settings.Vector3To, settings.Duration);
		}
	}
	
	public class MoveXTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			gameObject.transform.localPosition = gameObject.transform.localPosition.To0YZ(settings.FloatFrom);
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			return gameObject.transform.DOLocalMoveX(settings.FloatTo, settings.Duration);
		}
	}
	
	public class MoveYTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			gameObject.transform.localPosition = gameObject.transform.localPosition.ToX0Z(settings.FloatFrom);
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			return gameObject.transform.DOLocalMoveY(settings.FloatTo, settings.Duration);
		}
	}
	
	public class MoveZTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			gameObject.transform.localPosition = gameObject.transform.localPosition.ToXY0(settings.FloatFrom);
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			return gameObject.transform.DOLocalMoveZ(settings.FloatTo, settings.Duration);
		}
	}
	
	public class AnchorMoveTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			var rectTransform = (RectTransform)gameObject.transform;
			if (rectTransform == null) return;
			rectTransform.anchoredPosition = settings.Vector2From;
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			var rectTransform = (RectTransform)gameObject.transform;
			return rectTransform == null ? null : rectTransform.DOAnchorPos(settings.Vector2To, settings.Duration);
		}
	}
	
	public class AnchorMoveXTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			var rectTransform = (RectTransform)gameObject.transform;
			if (rectTransform == null) return;
			rectTransform.anchoredPosition = rectTransform.anchoredPosition.To0Y(settings.FloatFrom);
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			var rectTransform = (RectTransform)gameObject.transform;
			return rectTransform == null ? null : rectTransform.DOAnchorPosX(settings.FloatTo, settings.Duration);
		}
	}
	
	public class AnchorMoveYTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			var rectTransform = (RectTransform)gameObject.transform;
			if (rectTransform == null) return;
			rectTransform.anchoredPosition = rectTransform.anchoredPosition.ToX0(settings.FloatFrom);
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			var rectTransform = (RectTransform)gameObject.transform;
			return rectTransform == null ? null : rectTransform.DOAnchorPosY(settings.FloatTo, settings.Duration);
		}
	}
	
	public class ScaleTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			gameObject.transform.localScale = settings.Vector3From;
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			return gameObject.transform.DOScale(settings.Vector3To, settings.Duration);
		}
	}

	public class ScaleXTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			gameObject.transform.localScale = gameObject.transform.localScale.To0YZ(settings.FloatFrom);
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			return gameObject.transform.DOScaleX(settings.FloatTo, settings.Duration);
		}
	}
	
	public class ScaleYTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			gameObject.transform.localScale = gameObject.transform.localScale.ToX0Z(settings.FloatFrom);
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			return gameObject.transform.DOScaleY(settings.FloatTo, settings.Duration);
		}
	}
	
	public class ScaleZTween : IUIAnimationTween {
		public void Prepare(UIAnimationTweenSettings settings, GameObject gameObject) {
			gameObject.transform.localScale = gameObject.transform.localScale.ToXY0(settings.FloatFrom);
		}

		public Tween Create(UIAnimationTweenSettings settings, GameObject gameObject) {
			return gameObject.transform.DOScaleZ(settings.FloatTo, settings.Duration);
		}
	}
}