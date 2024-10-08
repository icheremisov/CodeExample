using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.UI.Animation {

	public enum UIAnimationType {
		Global,
		Animator,
		Tween,
		Custom
	}

	public enum UIAnimationTweenType {
		FadeIn      = 0,
		FadeOut     = 1,
		Move        = 10,
		MoveX       = 11,
		MoveY       = 12,
		MoveZ       = 13,
		AnchorMove  = 20,
		AnchorMoveX = 21,
		AnchorMoveY = 22,
		Scale       = 30,
		ScaleX      = 31,
		ScaleY      = 32,
		ScaleZ      = 33,
	}
	
	[Serializable]
	public partial class UIAnimationGlobalSettings {
		[SerializeField, ValueDropdown("GetAnimationsList")] private string _animationName;
		
		public string AnimationName => _animationName;
	}
	
	[Serializable]
	public partial class UIAnimationTweenSettings {
		[SerializeField] private UIAnimationTweenType _type;
		[SerializeField] private float _duration = 0.5f;
		[SerializeField, HideIf("_useAnimationCurve")] private Ease _ease = Ease.OutQuad;
		[SerializeField, LabelText("Use custom easy curve")] private bool _useAnimationCurve;
		[SerializeField, ShowIf("_useAnimationCurve")] private AnimationCurve _animationCurve = AnimationCurve.Linear(0f,0f,1f,1f);
		[SerializeField, LabelText("From"), ShowIf("IsVector3")] private Vector3 _vector3From;
		[SerializeField, LabelText("To"), ShowIf("IsVector3")] private Vector3 _vector3To;
		[SerializeField, LabelText("From"), ShowIf("IsVector2")] private Vector2 _vector2From;
		[SerializeField, LabelText("To"), ShowIf("IsVector2")] private Vector2 _vector2To;
		[SerializeField, LabelText("From"), ShowIf("IsFloat")] private float _floatFrom;
		[SerializeField, LabelText("To"), ShowIf("IsFloat")] private float _floatTo;
		
		public UIAnimationTweenType Type => _type;
		public float Duration => _duration;
		public Ease Ease => _ease;
		public bool UseAnimationCurve => _useAnimationCurve;
		public AnimationCurve AnimationCurve => _animationCurve;
		public Vector3 Vector3From => _vector3From;
		public Vector3 Vector3To => _vector3To;
		public Vector3 Vector2From => _vector2From;
		public Vector3 Vector2To => _vector2To;
		public float FloatFrom => _floatFrom;
		public float FloatTo => _floatTo;
	}
	
	[Serializable]
	public partial class UIAnimationAnimatorSettings {
		[ValidateInput("AnimatorCheck", "Required Animator component"), ValueDropdown("GetAnimationsList")]
		[SerializeField] private string _animation;

		public string Animation => _animation;
	}

	[Serializable]
	public partial class UIAnimationCustomSettings {
		[SerializeField] private UICustomAnimationBase _customAnimation;
		
		public UICustomAnimationBase CustomAnimation => _customAnimation;
	}
	
	[Serializable]
	public partial class UIAnimationPart {
		[SerializeField, OnValueChanged("OnTypeChange")] private UIAnimationType _type = UIAnimationType.Tween;
		[SerializeField] private bool _overrideTarget;
		[SerializeField, ShowIf("_overrideTarget"), Required, ChildGameObjectsOnly] private GameObject _target;
		[SerializeField] private float _delay;
		
		[SerializeReference, HideLabel, InlineProperty, HideReferenceObjectPicker, ShowIf("IsGlobal")]
		private UIAnimationGlobalSettings _globalSettings;
		[SerializeReference, HideLabel, InlineProperty, HideReferenceObjectPicker, ShowIf("IsTween")]
		private UIAnimationTweenSettings _tweenSettings = new();
		[SerializeReference, HideLabel, InlineProperty, HideReferenceObjectPicker, ShowIf("IsAnimator")]
		private UIAnimationAnimatorSettings _animatorSettings;
		[SerializeReference, HideLabel, InlineProperty, HideReferenceObjectPicker, ShowIf("IsCustom")]
		private UIAnimationCustomSettings _customSettings;
		
		public UIAnimationType Type => _type;
		public bool OverrideTarget => _overrideTarget;
		public GameObject Target => _target;
		public float Delay => _delay;
		public UIAnimationGlobalSettings GlobalSettings => _globalSettings;
		public UIAnimationTweenSettings TweenSettings => _tweenSettings;
		public UIAnimationAnimatorSettings AnimatorSettings => _animatorSettings;
		public UIAnimationCustomSettings CustomSettings => _customSettings;
	}

	[Serializable]
	public partial class UIAnimation {
		[SerializeField] private bool _activateOnPlay;
		[SerializeField] private float _delay;
		[SerializeField] private bool _delaySiblingMultiply;
		[SerializeField, ShowIf("_delaySiblingMultiply")] private bool _reverseDelaySiblingMultiply;
		[SerializeField, ShowIf("_delaySiblingMultiply")] private float _delaySibling;
		[SerializeField] private UIAnimationPart[] _parts;
		
		public bool ActivateOnPlay => _activateOnPlay;
		public float Delay => _delay;
		public bool DelaySiblingMultiply => _delaySiblingMultiply;
		public bool ReverseDelaySiblingMultiply => _reverseDelaySiblingMultiply;
		public float DelaySibling => _delaySibling;
		public UIAnimationPart[] Parts => _parts;
	}
	
	[Serializable]
	public partial class UINamedAnimation : UIAnimation {
		[SerializeField, OnValueChanged("SetHash"), PropertyOrder(-2)] private string _name;
		[SerializeField, ReadOnly, PropertyOrder(-1)] private int _hash;

		public string Name => _name;
		public int Hash => _hash;
	}
}