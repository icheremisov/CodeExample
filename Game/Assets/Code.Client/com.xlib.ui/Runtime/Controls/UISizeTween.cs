using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Controls {

	public class UISizeTween : MonoBehaviour {
		[SerializeField] private float _animTimeSec = 0.2f;

		[Header("Width")]
		[SerializeField] private bool _width = false;
		[SerializeField, ShowIf(nameof(_width))] private int _widthShort = 0;
		[SerializeField, ShowIf(nameof(_width))] private int _widthLong = 100;

		[Header("Height")]
		[SerializeField] private bool _height = false;
		[SerializeField, ShowIf(nameof(_height))] private int _heightShort = 0;
		[SerializeField, ShowIf(nameof(_height))] private int _heightLong = 100;

		private LayoutGroup _parentLayout;

		private void OnEnable() {
			InitLayout();
		}

		private void OnDisable() {
			transform.DOKill(true);
		}

		private void InitLayout() {
			if (!_parentLayout) _parentLayout = this.GetComponentInParent<LayoutGroup>(true);
		}

		[Button]
		public void SetShort(bool animated) {
			transform.DOKill();

			var target = new Vector2(_widthShort, _heightShort);
			if (animated && Application.isPlaying && gameObject.activeInHierarchy)
				Play(target);
			else
				Set(target);
		}

		[Button]
		public void SetLong(bool animated) {
			transform.DOKill();

			var target = new Vector2(_widthLong, _heightLong);
			if (animated && Application.isPlaying)
				Play(target);
			else
				Set(target);
		}

		private void Play(Vector2 target) {
			InitLayout();

			var tm = (RectTransform)transform;
			if (_width && _height)
				tm.DOSizeDelta(target, _animTimeSec).OnUpdate(UpdateLayout).OnComplete(() => Set(target));
			else if (_width)
				tm.DOSizeDelta(target.ToX0(tm.sizeDelta.y), _animTimeSec).OnUpdate(UpdateLayout).OnComplete(() => Set(target));
			else if (_height) 
				tm.DOSizeDelta(target.To0Y(tm.sizeDelta.x), _animTimeSec).OnUpdate(UpdateLayout).OnComplete(() => Set(target));
		}
		
		private void Set(Vector2 target) {
			InitLayout();

			var tm = (RectTransform)transform;
			if (_width && _height)
				tm.sizeDelta = target;
			else if (_width)
				tm.sizeDelta = target.ToX0(tm.sizeDelta.y);
			else if (_height) 
				tm.sizeDelta = target.To0Y(tm.sizeDelta.x);

			UpdateLayout();
		}

		private void UpdateLayout() {
			if (_parentLayout != null) LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_parentLayout.transform);
		}
	}

}