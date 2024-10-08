#if UNITY_EDITOR

using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using XLib.UI.Animation.Configs;

namespace XLib.UI.Animation {

	public partial class UIAnimationGlobalSettings {
		private IEnumerable GetAnimationsList() {
			return Application.isPlaying ? Array.Empty<ValueDropdownItem>() :
				UIGlobalAnimations.Instance.Animations.Select(x => new ValueDropdownItem(x.Name, x.Name));
		}
	}
	
	public partial class UIAnimationTweenSettings {
		private bool IsVector3() => _type is UIAnimationTweenType.Move or UIAnimationTweenType.Scale;
		private bool IsVector2() => _type == UIAnimationTweenType.AnchorMove;
		private bool IsFloat() => _type is
			UIAnimationTweenType.MoveX or UIAnimationTweenType.MoveY or UIAnimationTweenType.MoveZ or
			UIAnimationTweenType.AnchorMoveX or UIAnimationTweenType.AnchorMoveY or
			UIAnimationTweenType.ScaleX or UIAnimationTweenType.ScaleY or UIAnimationTweenType.ScaleZ;
	}
	
	public partial class UIAnimationAnimatorSettings {
		private bool AnimatorCheck() {
			return Selection.activeObject is GameObject go && go.GetComponent<Animator>() != null;
		}
		
		private IEnumerable GetAnimationsList() {
			if (Selection.activeObject is not GameObject go) return Array.Empty<ValueDropdownItem>();
			var animator = go.GetComponent<Animator>();
			if (animator == null || animator.runtimeAnimatorController == null) return Array.Empty<ValueDropdownItem>();
			return animator.runtimeAnimatorController.animationClips.Select(x => new ValueDropdownItem(x.name, x.name));
		}
	}
	
	public partial class UIAnimationCustomSettings {
		
	}
	
	public partial class UIAnimationPart {
		private void OnTypeChange() {
			if (!IsGlobal) _globalSettings = null;
			if (!IsTween) _tweenSettings = null;
			if (!IsAnimator) _animatorSettings = null;
			if (!IsCustom) _customSettings = null;

			switch (_type) {
				case UIAnimationType.Global:
					_globalSettings ??= new UIAnimationGlobalSettings();
					break;
				case UIAnimationType.Animator:
					_animatorSettings ??= new UIAnimationAnimatorSettings();
					break;
				case UIAnimationType.Tween:
					_tweenSettings ??= new UIAnimationTweenSettings();
					break;
				case UIAnimationType.Custom:
					_customSettings ??= new UIAnimationCustomSettings();
					break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
		
		private bool IsGlobal => _type == UIAnimationType.Global;
		private bool IsTween => _type == UIAnimationType.Tween;
		private bool IsAnimator => _type == UIAnimationType.Animator;
		private bool IsCustom => _type == UIAnimationType.Custom;
	}
	
	public partial class UINamedAnimation {
		private void SetHash() => _hash = _name?.GetHashCode() ?? 0;
	}

}

#endif