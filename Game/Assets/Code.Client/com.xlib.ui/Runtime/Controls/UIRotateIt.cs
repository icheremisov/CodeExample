using DG.Tweening;
using UnityEngine;

namespace XLib.UI.Controls {

	public class UIRotateIt : MonoBehaviour {

		[SerializeField] private float _degPerSec = 180;

		private void OnEnable() {
			transform.DOLocalRotate(new Vector3(0, 0, _degPerSec), 1.0f)
				.SetUpdate(true)
				.SetEase(Ease.Linear)
				.SetRelative()
				.SetLoops(-1, LoopType.Incremental);
		}

		private void OnDisable() {
			transform.DOKill();
		}

	}

}