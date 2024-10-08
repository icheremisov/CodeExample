using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.UI.Screens;

namespace XLib.UI.Animation {

	[RequireComponent(typeof(UIScreen))]
	public sealed partial class UIScreenAnimation : UISceneAnimation {
		[SerializeField, OnValueChanged("OnRootObjectsChange", true)] private GameObject[] _rootObjects;

		private void Awake() {
			SaveStartValues();
		}

		private void OnDisable() {
			ApplyStartValues();
		}

		public IEnumerable<T> GetAnimationComponents<T>() where T : Component {
			return transform.GetComponentsInChildren<T>(true)
				.Concat(_rootObjects?.Where(go => go != null)
				.SelectMany(go => go.GetComponentsInChildren<T>(true)) ?? Array.Empty<T>())
				.Where(go => go != this);
		}

		public override async UniTask Show(CancellationToken ct, float delay = 0) {
			var viewAnimations = GetAnimationComponents<UIViewAnimation>();
			await UniTask.WhenAll(viewAnimations.Select(viewAnimation => viewAnimation.Show(ct, delay)).Append(ShowInternal(ct, delay)));
		}

		public override async UniTask Hide(CancellationToken ct, float delay = 0) {
			var viewAnimations = GetAnimationComponents<UIViewAnimation>();
			await UniTask.WhenAll(viewAnimations.Select(viewAnimation => viewAnimation.Hide(ct, delay)).Append(HideInternal(ct, delay)));
		}

		public override async UniTask Play(string animationName, CancellationToken ct, float delay = 0) {
			if (animationName.IsNullOrEmpty()) return;
			await Play(animationName.GetHashCode(), ct);
		}

		public override async UniTask Play(int animationHash, CancellationToken ct, float delay = 0) {
			var viewAnimations = GetAnimationComponents<UIViewAnimation>();
			await UniTask.WhenAll(viewAnimations.Select(viewAnimation => viewAnimation.Play(animationHash, ct, delay)).Append(PlayInternal(animationHash, ct, delay)));
		}
		
		public override void SaveStartValues() {
			base.SaveStartValues();
			
			var savableValues = GetAnimationComponents<UIAnimationSavableValues>();
			foreach (var savableValue in savableValues) savableValue.SaveStartValues();
		}
		
		public override void ApplyStartValues() {
			base.ApplyStartValues();
			
			var savableValues = GetAnimationComponents<UIAnimationSavableValues>();
			foreach (var savableValue in savableValues) savableValue.ApplyStartValues();
		}
	}

}