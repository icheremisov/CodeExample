using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.DOTweenEditor;
using DG.Tweening;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace XLib.UI.Animation {

	[CustomEditor(typeof(UISceneAnimation))]
	public class UISceneAnimationInspector : OdinEditor  {
		private bool _playing;
		private CancellationTokenSource _cancellationTokenSource;
		
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			var script = (UISceneAnimation)target;
			
			EditorGUI.BeginDisabledGroup(_playing);
			GUILayout.BeginHorizontal();
			
			if (GUILayout.Button("► Play Show")) Play(script.Show).Forget();
			if (GUILayout.Button("► Play Hide")) Play(script.Hide).Forget();
			
			GUILayout.EndHorizontal();
			foreach (var animation in script.Animations) {
				if (GUILayout.Button($"► Play {animation.Name}"))
					Play((ct, delay) => script.Play(animation.Hash, ct, delay)).Forget();
			}
			EditorGUI.EndDisabledGroup();
			
			EditorGUI.BeginDisabledGroup(!_playing);
			if (GUILayout.Button("■ Stop")) {
				if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested) {
					_cancellationTokenSource = null;
					_playing = false;
				}
				else _cancellationTokenSource?.Cancel();
			}
			EditorGUI.EndDisabledGroup();
		}
		
		private async UniTaskVoid Play(Func<CancellationToken, float, UniTask> callback) {
			var animation = (UISceneAnimation)target;
			
			

			if (!EditorApplication.isPlaying) {
				animation.SaveStartValues();
				DOTweenEditorPreview.Start();
				UIAnimationExtensions.PlayTweenEvent += OnPlayTween;
				
				if (!AnimationMode.InAnimationMode()) AnimationMode.StartAnimationMode();
				AnimationMode.BeginSampling();
			}
			
			_cancellationTokenSource = new CancellationTokenSource();
			_playing = true;

			await callback(_cancellationTokenSource.Token, 0);
			
			_playing = false;
			_cancellationTokenSource = null;
			
			if (!EditorApplication.isPlaying) {
				UIAnimationExtensions.PlayTweenEvent -= OnPlayTween;
				DOTweenEditorPreview.Stop();
				animation.ApplyStartValues();
				
				AnimationMode.EndSampling();
				if (AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode();
			}
		}

		private static void OnPlayTween(Tween tween) =>
			DOTweenEditorPreview.PrepareTweenForPreview(tween, true, false);
	}

	[CustomEditor(typeof(UIScreenAnimation))]
	public class UIScreenAnimationInspector : UISceneAnimationInspector {
		private IEnumerable<UIViewAnimation> _viewAnimations;
		
		protected override void OnEnable() {
			base.OnEnable();
			_viewAnimations = ((UIScreenAnimation)target).GetAnimationComponents<UIViewAnimation>();
		}
		
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Child animations");
			foreach (var viewAnimation in _viewAnimations) {
				if (GUILayout.Button($"{viewAnimation.GetFullPath()}")) Selection.activeObject = viewAnimation;
			}
		}
	}
	
	[CustomEditor(typeof(UIViewAnimation))]
	public class UIViewAnimationInspector : UISceneAnimationInspector {}

}