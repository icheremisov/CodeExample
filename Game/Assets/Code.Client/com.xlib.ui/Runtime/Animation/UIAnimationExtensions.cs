using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using XLib.UI.Animation.Configs;
using XLib.UI.Animation.Contracts;
using XLib.UI.Animation.Tweens;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XLib.UI.Animation {

	public static class UIAnimationExtensions {
		public delegate void PlayTweenCallback(Tween tween);
		public static PlayTweenCallback PlayTweenEvent;

		private static readonly Dictionary<UIAnimationTweenType, IUIAnimationTween> Tweens = new() {
			{ UIAnimationTweenType.FadeIn,      new FadeInTween()      },
			{ UIAnimationTweenType.FadeOut,     new FadeOutTween()     },
			{ UIAnimationTweenType.Move,        new MoveTween()        },
			{ UIAnimationTweenType.MoveX,       new MoveXTween()       },
			{ UIAnimationTweenType.MoveY,       new MoveYTween()       },
			{ UIAnimationTweenType.MoveZ,       new MoveZTween()       },
			{ UIAnimationTweenType.AnchorMove,  new AnchorMoveTween()  },
			{ UIAnimationTweenType.AnchorMoveX, new AnchorMoveXTween() },
			{ UIAnimationTweenType.AnchorMoveY, new AnchorMoveYTween() },
			{ UIAnimationTweenType.Scale,       new ScaleTween()       },
			{ UIAnimationTweenType.ScaleX,      new ScaleXTween()      },
			{ UIAnimationTweenType.ScaleY,      new ScaleYTween()      },
			{ UIAnimationTweenType.ScaleZ,      new ScaleZTween()      },
		};
		
		private static void PrepareGlobal(this UIAnimationPart animationPart, GameObject gameObject) {
			var globalAnimation = UIGlobalAnimations.Instance != null ? UIGlobalAnimations.Instance.GetAnimation(animationPart.GlobalSettings.AnimationName) : null;
			if (globalAnimation == null) return;
			foreach (var part in globalAnimation.Parts) part.Prepare(gameObject);
		}
		
		private static async UniTask PlayGlobal(this UIAnimationPart animationPart, GameObject gameObject, CancellationToken ct) {
			var globalAnimation = UIGlobalAnimations.Instance != null ? UIGlobalAnimations.Instance.GetAnimation(animationPart.GlobalSettings.AnimationName) : null;
			if (globalAnimation != null) await UniTask.WhenAll(globalAnimation.Parts.Select(part => part.Play(gameObject, ct)));
		}
		
		private static void PrepareAnimator(this UIAnimationPart animationPart, GameObject gameObject) {
			//var animator = gameObject.GetComponent<Animator>();
			//if (animator == null) return;
		}
		
		private static async UniTask PlayAnimator(this UIAnimationPart animationPart, GameObject gameObject, CancellationToken ct) {
			var animator = gameObject.GetComponent<Animator>();
			if (animator == null || animator.runtimeAnimatorController == null) return;

			var animation = animationPart.AnimatorSettings.Animation;
			
#if UNITY_EDITOR
			if (Application.isPlaying) {
#endif
				var deactivateAfterPlay = false;
			
				if (!animator.enabled) {
					animator.enabled = true;
					deactivateAfterPlay = true;
				}
				
				animator.Play(animation);
				await UniTask.Yield();
				await UniTask.WaitWhile(() => {
					var state = animator.GetCurrentAnimatorStateInfo(0);
					return state.length > state.normalizedTime && state.IsName(animation);
				}, cancellationToken: ct);

				if (deactivateAfterPlay) animator.enabled = false;
#if UNITY_EDITOR
			}
			else {
				var clip = animator.runtimeAnimatorController.animationClips.FirstOrDefault(ac => ac.name == animation);
				if (clip == null) return;
				var time = 0f;
				const float deltaTime = 1f / 60f;
				while (time < clip.length) {
					AnimationMode.SampleAnimationClip(gameObject, clip, time);
					await UniEx.DelaySec(deltaTime, ct);
					time += deltaTime;
				}
			}
#endif
		}

		private static void PrepareTween(this UIAnimationPart animationPart, GameObject gameObject) {
			var settings = animationPart.TweenSettings;

			if (!Tweens.TryGetValue(settings.Type, out var tween)) throw new Exception($"Can not found animation tween {settings.Type}");
			
			tween.Prepare(settings, gameObject);
		}
		
		private static Tween CreateTween(this UIAnimationPart animationPart, GameObject gameObject) {
			var settings = animationPart.TweenSettings;
			
			if (!Tweens.TryGetValue(settings.Type, out var tween)) throw new Exception($"Can not found animation tween {settings.Type}");
			
			return settings.UseAnimationCurve ? tween.Create(settings, gameObject).SetEase(settings.AnimationCurve).SetUpdate(true) : tween.Create(settings, gameObject).SetEase(settings.Ease).SetUpdate(true);
		}
		
		private static async UniTask PlayTween(this UIAnimationPart animationPart, GameObject gameObject, CancellationToken ct) {
			var tween = animationPart.CreateTween(gameObject);
			if (tween == null) return;
			PlayTweenEvent?.Invoke(tween);
			await tween.WithCancellation(ct);
		}
		
		private static void PrepareCustom(this UIAnimationPart animationPart) {
			if (animationPart.CustomSettings.CustomAnimation != null)
				animationPart.CustomSettings.CustomAnimation.Prepare();
		}
		
		private static async UniTask PlayCustom(this UIAnimationPart animationPart, CancellationToken ct) {
			if (animationPart.CustomSettings.CustomAnimation != null)
				await animationPart.CustomSettings.CustomAnimation.Play(ct);
		}

		private static void Prepare(this UIAnimationPart animationPart, GameObject gameObject) {
			var go = animationPart.OverrideTarget ? animationPart.Target : gameObject;
			
			if (go == null) return;
			
			switch (animationPart.Type) {
				case UIAnimationType.Global:
					animationPart.PrepareGlobal(go);
					break;
				case UIAnimationType.Animator:
					animationPart.PrepareAnimator(go);
					break;
				case UIAnimationType.Tween:
					animationPart.PrepareTween(go);
					break;
				case UIAnimationType.Custom:
					animationPart.PrepareCustom();
					break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
		
		private static async UniTask Play(this UIAnimationPart animationPart, GameObject gameObject, CancellationToken ct) {
			var go = animationPart.OverrideTarget ? animationPart.Target : gameObject;
			
			if (go == null) return;
			
			if (animationPart.Delay > 0) await UniEx.DelaySec(animationPart.Delay, ct);

			switch (animationPart.Type) {
				case UIAnimationType.Global:
					await animationPart.PlayGlobal(go, ct);
					break;
				case UIAnimationType.Animator:
					await animationPart.PlayAnimator(go, ct);
					break;
				case UIAnimationType.Tween:
					await animationPart.PlayTween(go, ct);
					break;
				case UIAnimationType.Custom:
					await animationPart.PlayCustom(ct);
					break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
		
		public static async UniTask Play(this UIAnimation animation, GameObject gameObject, Transform transform, CancellationToken ct, bool skipOneFrame, float delay) {
			if (animation.Parts.IsNullOrEmpty()) return;

			var deactivateAfterPlay = false;
			
			if (!gameObject.activeSelf) {
				if (animation.ActivateOnPlay) {
					gameObject.SetActive(true);
					deactivateAfterPlay = true;
				}
				else return;
			}

			if (gameObject.activeInHierarchy) {
				foreach (var part in animation.Parts) part.Prepare(gameObject);
				
				if (skipOneFrame) await UniTask.NextFrame(ct);
				
				if (delay > 0) await UniEx.DelaySec(delay, ct);
				if (animation.Delay > 0) await UniEx.DelaySec(animation.Delay, ct);
				if (animation.DelaySiblingMultiply && animation.DelaySibling > 0) {
					var delaySiblingsTime = animation.DelaySibling * 
						(!animation.ReverseDelaySiblingMultiply ? transform.GetSiblingIndex() : transform.parent.childCount-transform.GetSiblingIndex());   
					
					await UniEx.DelaySec(delaySiblingsTime, ct);
				}
				
				await UniTask.WhenAll(animation.Parts.Select(part => part.Play(gameObject, ct)));
			}

			if (deactivateAfterPlay) gameObject.SetActive(false);
		}
		
	}

}