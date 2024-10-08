using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLib.UI.Contracts;

namespace XLib.UI.Tooltips {

	public abstract class UITooltipBase : MonoBehaviour, ILayoutIgnorer {
		public enum TooltipPosition {
			Top,
			Left,
			Right,
			Bottom,
			Fixed,
			LeftOrRight,
			Center,
			NoMove
		}

		[EnumToggleButtons, SerializeField] protected TooltipPosition _position;

		[SerializeField] protected Vector2 _offset;

		[SerializeField] protected bool _useClamp = true;

		[SerializeField] protected bool _autoClose = true;

		[SerializeField, ShowIf("@this._position >= TooltipPosition.Top && this._position <= TooltipPosition.Right")]
		protected float _pivotOffset;

		private IChangeEvent _changeEvent;

		protected TooltipClickData _clickData;

		protected virtual void UpdateViewBasedOnPosition(TooltipPosition actualPosition) {
			var rt = transform as RectTransform;
			var pivot = rt.pivot;
			switch (actualPosition) {
				case TooltipPosition.Top:
					pivot.y = _pivotOffset;
					break;

				case TooltipPosition.Left:
					pivot.x = 1f - _pivotOffset;
					break;

				case TooltipPosition.Right:
					pivot.x = _pivotOffset;
					break;
			}

			rt.pivot = pivot;
		}

		protected virtual void UpdateTooltipPosition() {
			if (_position == TooltipPosition.NoMove) return;

			transform.RecursiveUpdateLayout();

			var tooltipTr = transform as RectTransform;
			var tooltipParentTr = tooltipTr.parent as RectTransform;
			var bounds = tooltipParentTr.rect;

			var transformedRect = _clickData.GetTransformedRect(tooltipParentTr);

			Vector2 anchorPos;
			var offset = _offset;
			switch (_position) {
				case TooltipPosition.Top:
				default:
					anchorPos = new Vector2(transformedRect.x + transformedRect.width * 0.5f, transformedRect.yMax);
					break;

				case TooltipPosition.Bottom:
					anchorPos = new Vector2(transformedRect.x + transformedRect.width * 0.5f, transformedRect.y);
					break;

				case TooltipPosition.Left:
					anchorPos = new Vector2(transformedRect.x, transformedRect.y + transformedRect.height * 0.5f);
					break;

				case TooltipPosition.Right:
					anchorPos = new Vector2(transformedRect.xMax, transformedRect.y + transformedRect.height * 0.5f);
					break;

				case TooltipPosition.LeftOrRight:
					if (transformedRect.x + transformedRect.width * 0.5f - bounds.x > bounds.width / 2f) {
						UpdateViewBasedOnPosition(TooltipPosition.Left);
						offset = new Vector2(-_offset.x, _offset.y);
						goto case TooltipPosition.Left;
					}

					UpdateViewBasedOnPosition(TooltipPosition.Right);
					goto case TooltipPosition.Right;

				case TooltipPosition.Fixed:
					anchorPos = tooltipTr.anchoredPosition;
					break;

				case TooltipPosition.Center:
					anchorPos = new Vector2(transformedRect.x + transformedRect.width * 0.5f, transformedRect.y + transformedRect.height * 0.5f);
					break;
			}

			anchorPos += offset;

			if (_position != TooltipPosition.Fixed && _useClamp) {
				var tooltipSize = tooltipTr.rect;
				ClampInterval(ref anchorPos.x, tooltipSize.xMin, tooltipSize.xMax, bounds.xMin, bounds.xMax);
				ClampInterval(ref anchorPos.y, tooltipSize.yMin, tooltipSize.yMax, bounds.yMin, bounds.yMax);
			}

			tooltipTr.localPosition = new Vector3(anchorPos.x, anchorPos.y, tooltipTr.localPosition.z);
		}

		private static void ClampInterval(ref float pos, float offsetMin, float offsetMax, float min, float max) {
			float delta;
			if ((delta = pos + offsetMax - max) > 0)
				pos -= delta;
			else if ((delta = min - pos - offsetMin) > 0) pos += delta;
		}

		//localPoint - local pos in tooltip parent rect
		protected void Show(TooltipClickData clickData, IChangeEvent change = null) {
			_clickData = clickData;
			transform.SetAsLastSibling();
			gameObject.SetActive(true);
			EventSystem.current?.SetSelectedGameObject(gameObject);
			if (_changeEvent != null) _changeEvent.OnChange -= Refresh;
			_changeEvent = change;
			if (_changeEvent != null) _changeEvent.OnChange += Refresh;
			UpdateTooltipPosition();
		}

		protected virtual void Refresh() { }

		public virtual void Close() {
			if (_changeEvent != null) {
				_changeEvent.OnChange -= Refresh;
				_changeEvent = null;
			}

			gameObject.SetActive(false);
		}

		private void Update() {
			if (!_autoClose) return;

			if (_clickData.Target == null || !_clickData.Target.activeInHierarchy) {
				Close();
				return;
			}

			var currentEventSystem = EventSystem.current;
			var obj = currentEventSystem == null ? null : currentEventSystem.currentSelectedGameObject;
			if (obj == null || !obj.transform.IsChildOf(transform)) Close();
		}

		public bool ignoreLayout => true;
	}

}