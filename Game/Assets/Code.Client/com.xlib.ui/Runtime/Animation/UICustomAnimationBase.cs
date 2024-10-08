using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace XLib.UI.Animation {

	public abstract class UICustomAnimationBase : UIAnimationSavableValues {
		public abstract void Prepare();
		public abstract UniTask Play(CancellationToken ct);

		public override void SaveStartValues() => SaveStartValues(gameObject);
		public override void ApplyStartValues() => ApplyStartValues(gameObject);
	}

}