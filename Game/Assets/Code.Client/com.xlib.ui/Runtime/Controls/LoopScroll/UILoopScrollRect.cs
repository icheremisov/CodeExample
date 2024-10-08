using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLib.UI.Controls.LoopScroll.ItemModel;
using XLib.UI.Controls.LoopScroll.ItemView;

namespace XLib.UI.Controls.LoopScroll {

	[AddComponentMenu(""), DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
	public abstract class UILoopScrollRect : UIBehaviour,
											 IInitializePotentialDragHandler,
											 IBeginDragHandler,
											 IEndDragHandler,
											 IDragHandler,
											 IScrollHandler,
											 ICanvasElement,
											 ILayoutElement,
											 ILayoutGroup {

		//==========LoopScrollRect==========

		public enum MovementType {

			Unrestricted, // Unrestricted movement -- can scroll forever
			Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
			Clamped // Restricted movement where it's not possible to go past the edges

		}

		public enum ScrollbarVisibility {

			Permanent,
			AutoHide,
			AutoHideAndExpandViewport

		}

		[Tooltip("Total count, negative means INFINITE mode")]
		public int totalCount;

		[Tooltip("Reverse direction for dragging")] public bool reverseDirection;
		[Tooltip("Rubber scale for outside")] public float rubberScale = 1;

		[SerializeField] private RectTransform m_Content;

		[SerializeField] private bool m_Horizontal = true;

		[SerializeField] private bool m_Vertical = true;

		[SerializeField] private MovementType m_MovementType = MovementType.Elastic;

		[SerializeField] private float m_Elasticity = 0.1f; // Only used for MovementType.Elastic

		[SerializeField] private bool m_Inertia = true;

		[SerializeField] private float m_DecelerationRate = 0.135f; // Only used when inertia is enabled

		[SerializeField] private float m_ScrollSensitivity = 1.0f;

		[SerializeField] private RectTransform m_Viewport;

		[SerializeField] private Scrollbar m_HorizontalScrollbar;

		[SerializeField] private Scrollbar m_VerticalScrollbar;

		[SerializeField] private ScrollbarVisibility m_HorizontalScrollbarVisibility;

		[SerializeField] private ScrollbarVisibility m_VerticalScrollbarVisibility;

		[SerializeField] private float m_HorizontalScrollbarSpacing;

		[SerializeField] private float m_VerticalScrollbarSpacing;

		[SerializeField] private ScrollRectEvent m_OnValueChanged = new();

		private readonly Vector3[] m_Corners = new Vector3[4];

		private bool _factoryInitialized;
		//==========LoopScrollRect==========

		private ILoopScrollItemViewFactory _viewFactory;
		private int deletedItemTypeEnd;

		private int deletedItemTypeStart;

		protected LoopScrollRectDirection direction = LoopScrollRectDirection.Horizontal;
		protected int itemTypeEnd;

		protected int itemTypeStart;

		private Bounds m_ContentBounds;
		private int m_ContentConstraintCount;

		private bool m_ContentConstraintCountInit;

		private bool m_ContentSpaceInit;
		private float m_ContentSpacing;
		private Vector2 m_ContentStartPosition = Vector2.zero;

		private bool m_Dragging;
		protected GridLayoutGroup m_GridLayout;
		[NonSerialized] private bool m_HasRebuiltLayout;

		private RectTransform m_HorizontalScrollbarRect;

		private bool m_HSliderExpand;
		private float m_HSliderHeight;

		// The offset from handle position to mouse down position
		private Vector2 m_PointerStartLocalCursor = Vector2.zero;
		private Bounds m_PrevContentBounds;

		private Vector2 m_PrevPosition = Vector2.zero;
		private Bounds m_PrevViewBounds;

		[NonSerialized] private RectTransform m_Rect;

		private DrivenRectTransformTracker m_Tracker;

		private Vector2 m_Velocity;
		private RectTransform m_VerticalScrollbarRect;
		private Bounds m_ViewBounds;

		private RectTransform m_ViewRect;
		private bool m_VSliderExpand;
		private float m_VSliderWidth;

		protected float threshold;

		protected UILoopScrollRect() {
			flexibleWidth = -1;
		}

		public ILoopScrollItemViewFactory ViewFactory {
			private get {
				if (_viewFactory == null && !_factoryInitialized) {
					_viewFactory = GetComponent<ILoopScrollItemViewFactory>();
					_factoryInitialized = true;
					if (_viewFactory == null) UILogger.LogError($"Cannot find {nameof(ILoopScrollItemViewFactory)} for Scroll {this.GetFullPath()}");
				}

				return _viewFactory;
			}
			set {
				_viewFactory = value;
				_factoryInitialized = true;
			}
		}

		public ILoopScrollModelSource DataSource { get; set; } = EmptyModelSource.Instance;

		protected float contentSpacing {
			get {
				if (m_ContentSpaceInit) return m_ContentSpacing;

				m_ContentSpaceInit = true;
				m_ContentSpacing = 0;
				if (content != null) {
					var layout1 = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
					if (layout1 != null) m_ContentSpacing = layout1.spacing;

					m_GridLayout = content.GetComponent<GridLayoutGroup>();
					if (m_GridLayout != null) m_ContentSpacing = GetDimension(m_GridLayout.spacing);
				}

				return m_ContentSpacing;
			}
		}

		protected int contentConstraintCount {
			get {
				if (m_ContentConstraintCountInit) return m_ContentConstraintCount;

				m_ContentConstraintCountInit = true;
				m_ContentConstraintCount = 1;
				if (content != null) {
					var layout2 = content.GetComponent<GridLayoutGroup>();
					if (layout2 != null) {
						if (layout2.constraint == GridLayoutGroup.Constraint.Flexible) UILogger.LogWarning("[LoopScrollRect] Flexible not supported yet");

						m_ContentConstraintCount = layout2.constraintCount;
					}
				}

				return m_ContentConstraintCount;
			}
		}

		// the first line
		protected int StartLine => Mathf.CeilToInt((float)itemTypeStart / contentConstraintCount);

		// how many lines we have for now
		protected int CurrentLines => Mathf.CeilToInt((float)(itemTypeEnd - itemTypeStart) / contentConstraintCount);

		// how many lines we have in total
		protected int TotalLines => Mathf.CeilToInt((float)totalCount / contentConstraintCount);
		public RectTransform content { get => m_Content; set => m_Content = value; }
		public bool horizontal { get => m_Horizontal; set => m_Horizontal = value; }
		public bool vertical { get => m_Vertical; set => m_Vertical = value; }
		public MovementType movementType { get => m_MovementType; set => m_MovementType = value; }
		public float elasticity { get => m_Elasticity; set => m_Elasticity = value; }
		public bool inertia { get => m_Inertia; set => m_Inertia = value; }
		public float decelerationRate { get => m_DecelerationRate; set => m_DecelerationRate = value; }
		public float scrollSensitivity { get => m_ScrollSensitivity; set => m_ScrollSensitivity = value; }

		public RectTransform viewport {
			get => m_Viewport;
			set {
				m_Viewport = value;
				SetDirtyCaching();
			}
		}

		public Scrollbar horizontalScrollbar {
			get => m_HorizontalScrollbar;
			set {
				if (m_HorizontalScrollbar) m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
				m_HorizontalScrollbar = value;
				if (m_HorizontalScrollbar) m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
				SetDirtyCaching();
			}
		}

		public Scrollbar verticalScrollbar {
			get => m_VerticalScrollbar;
			set {
				if (m_VerticalScrollbar) m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
				m_VerticalScrollbar = value;
				if (m_VerticalScrollbar) m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
				SetDirtyCaching();
			}
		}

		public ScrollbarVisibility horizontalScrollbarVisibility {
			get => m_HorizontalScrollbarVisibility;
			set {
				m_HorizontalScrollbarVisibility = value;
				SetDirtyCaching();
			}
		}

		public ScrollbarVisibility verticalScrollbarVisibility {
			get => m_VerticalScrollbarVisibility;
			set {
				m_VerticalScrollbarVisibility = value;
				SetDirtyCaching();
			}
		}

		public float horizontalScrollbarSpacing {
			get => m_HorizontalScrollbarSpacing;
			set {
				m_HorizontalScrollbarSpacing = value;
				SetDirty();
			}
		}

		public float verticalScrollbarSpacing {
			get => m_VerticalScrollbarSpacing;
			set {
				m_VerticalScrollbarSpacing = value;
				SetDirty();
			}
		}

		public ScrollRectEvent onValueChanged { get => m_OnValueChanged; set => m_OnValueChanged = value; }

		protected RectTransform viewRect {
			get {
				if (m_ViewRect == null) m_ViewRect = m_Viewport;
				if (m_ViewRect == null) m_ViewRect = (RectTransform)transform;
				return m_ViewRect;
			}
		}

		public Vector2 velocity { get => m_Velocity; set => m_Velocity = value; }

		private RectTransform rectTransform {
			get {
				if (m_Rect == null) m_Rect = GetComponent<RectTransform>();
				return m_Rect;
			}
		}

		public Vector2 normalizedPosition {
			get => new(horizontalNormalizedPosition, verticalNormalizedPosition);
			set {
				SetNormalizedPosition(value.x, 0);
				SetNormalizedPosition(value.y, 1);
			}
		}

		public float horizontalNormalizedPosition {
			get {
				UpdateBounds();
				//==========LoopScrollRect==========
				if (totalCount > 0 && itemTypeEnd > itemTypeStart) {
					var elementSize = (m_ContentBounds.size.x - contentSpacing * (CurrentLines - 1)) / CurrentLines;
					var totalSize = elementSize * TotalLines + contentSpacing * (TotalLines - 1);
					var offset = m_ContentBounds.min.x - elementSize * StartLine - contentSpacing * StartLine;

					if (totalSize <= m_ViewBounds.size.x) return m_ViewBounds.min.x > offset ? 1 : 0;
					return (m_ViewBounds.min.x - offset) / (totalSize - m_ViewBounds.size.x);
				}

				return 0.5f;
				//==========LoopScrollRect==========
			}
			set => SetNormalizedPosition(value, 0);
		}

		public float verticalNormalizedPosition {
			get {
				UpdateBounds();
				//==========LoopScrollRect==========
				if (totalCount > 0 && itemTypeEnd > itemTypeStart) {
					var elementSize = (m_ContentBounds.size.y - contentSpacing * (CurrentLines - 1)) / CurrentLines;
					var totalSize = elementSize * TotalLines + contentSpacing * (TotalLines - 1);
					var offset = m_ContentBounds.max.y + elementSize * StartLine + contentSpacing * StartLine;

					if (totalSize <= m_ViewBounds.size.y) return offset > m_ViewBounds.max.y ? 1 : 0;
					return (offset - m_ViewBounds.max.y) / (totalSize - m_ViewBounds.size.y);
				}

				return 0.5f;
				//==========LoopScrollRect==========
			}
			set => SetNormalizedPosition(value, 1);
		}

		private bool hScrollingNeeded {
			get {
				if (Application.isPlaying) return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
				return true;
			}
		}

		private bool vScrollingNeeded {
			get {
				if (Application.isPlaying) return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
				return true;
			}
		}

		//==========LoopScrollRect==========
#if UNITY_EDITOR
		protected override void Awake() {
			base.Awake();
			if (Application.isPlaying) {
				float value = reverseDirection ^ (direction == LoopScrollRectDirection.Horizontal) ? 0 : 1;
				Debug.Assert(Mathf.Abs(GetDimension(content.pivot)) == value, this);
				Debug.Assert(Mathf.Abs(GetDimension(content.anchorMin)) == value, this);
				Debug.Assert(Mathf.Abs(GetDimension(content.anchorMax)) == value, this);
			}
		}
#endif

		protected virtual void LateUpdate() {
			if (!m_Content) return;

			EnsureLayoutHasRebuilt();
			UpdateScrollbarVisibility();
			UpdateBounds();
			var deltaTime = Time.unscaledDeltaTime;
			var offset = CalculateOffset(Vector2.zero);
			if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero)) {
				var position = m_Content.anchoredPosition;
				for (var axis = 0; axis < 2; axis++) {
					// Apply spring physics if movement is elastic and content has an offset from the view.
					if (m_MovementType == MovementType.Elastic && offset[axis] != 0) {
						var speed = m_Velocity[axis];
						position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis], ref speed,
							m_Elasticity, Mathf.Infinity, deltaTime);
						m_Velocity[axis] = speed;
					}
					// Else move content according to velocity with deceleration applied.
					else if (m_Inertia) {
						m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
						if (Mathf.Abs(m_Velocity[axis]) < 1) m_Velocity[axis] = 0;
						position[axis] += m_Velocity[axis] * deltaTime;
					}
					// If we have neither elaticity or friction, there shouldn't be any velocity.
					else
						m_Velocity[axis] = 0;
				}

				if (m_Velocity != Vector2.zero) {
					if (m_MovementType == MovementType.Clamped) {
						offset = CalculateOffset(position - m_Content.anchoredPosition);
						position += offset;
					}

					SetContentAnchoredPosition(position);
				}
			}

			if (m_Dragging && m_Inertia) {
				Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
				m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
			}

			if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition) {
				UpdateScrollbars(offset);
				m_OnValueChanged.Invoke(normalizedPosition);
				UpdatePrevData();
			}
		}

		protected override void OnEnable() {
			base.OnEnable();

			if (m_HorizontalScrollbar) m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
			if (m_VerticalScrollbar) m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
		}

		protected override void OnDisable() {
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

			if (m_HorizontalScrollbar) m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
			if (m_VerticalScrollbar) m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

			m_HasRebuiltLayout = false;
			m_Tracker.Clear();
			m_Velocity = Vector2.zero;
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
			base.OnDisable();
		}

		protected override void OnRectTransformDimensionsChange() {
			SetDirty();
		}

#if UNITY_EDITOR
		protected override void OnValidate() {
			SetDirtyCaching();
		}
#endif

		public virtual void OnBeginDrag(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left) return;

			if (!IsActive()) return;

			UpdateBounds();

			m_PointerStartLocalCursor = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
			m_ContentStartPosition = m_Content.anchoredPosition;
			m_Dragging = true;
		}
		//==========LoopScrollRect==========

		public virtual void Rebuild(CanvasUpdate executing) {
			if (executing == CanvasUpdate.Prelayout) UpdateCachedData();

			if (executing == CanvasUpdate.PostLayout) {
				UpdateBounds();
				UpdateScrollbars(Vector2.zero);
				UpdatePrevData();

				m_HasRebuiltLayout = true;
			}
		}

		public virtual void LayoutComplete() { }

		public virtual void GraphicUpdateComplete() { }

		public virtual void OnDrag(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left) return;

			if (!IsActive()) return;

			Vector2 localCursor;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor)) return;

			UpdateBounds();

			var pointerDelta = localCursor - m_PointerStartLocalCursor;
			var position = m_ContentStartPosition + pointerDelta;

			// Offset to get content into place in the view.
			var offset = CalculateOffset(position - m_Content.anchoredPosition);
			position += offset;
			if (m_MovementType == MovementType.Elastic) {
				//==========LoopScrollRect==========
				if (offset.x != 0) position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x) * rubberScale;
				if (offset.y != 0) position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y) * rubberScale;
				//==========LoopScrollRect==========
			}

			SetContentAnchoredPosition(position);
		}

		public virtual void OnEndDrag(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left) return;

			m_Dragging = false;
		}

		public virtual void OnInitializePotentialDrag(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left) return;

			m_Velocity = Vector2.zero;
		}

		public virtual void CalculateLayoutInputHorizontal() { }

		public virtual void CalculateLayoutInputVertical() { }

		public virtual float minWidth => -1;
		public virtual float preferredWidth => -1;
		public virtual float flexibleWidth { get; }

		public virtual float minHeight => -1;
		public virtual float preferredHeight => -1;
		public virtual float flexibleHeight => -1;

		public virtual int layoutPriority => -1;

		public virtual void SetLayoutHorizontal() {
			m_Tracker.Clear();

			if (m_HSliderExpand || m_VSliderExpand) {
				m_Tracker.Add(this, viewRect,
					DrivenTransformProperties.Anchors |
					DrivenTransformProperties.SizeDelta |
					DrivenTransformProperties.AnchoredPosition);

				// Make view full size to see if content fits.
				viewRect.anchorMin = Vector2.zero;
				viewRect.anchorMax = Vector2.one;
				viewRect.sizeDelta = Vector2.zero;
				viewRect.anchoredPosition = Vector2.zero;

				// Recalculate content layout with this size to see if it fits when there are no scrollbars.
				LayoutRebuilder.ForceRebuildLayoutImmediate(content);
				m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
				m_ContentBounds = GetBounds();
			}

			// If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
			if (m_VSliderExpand && vScrollingNeeded) {
				viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);

				// Recalculate content layout with this size to see if it fits vertically
				// when there is a vertical scrollbar (which may reflowed the content to make it taller).
				LayoutRebuilder.ForceRebuildLayoutImmediate(content);
				m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
				m_ContentBounds = GetBounds();
			}

			// If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
			if (m_HSliderExpand && hScrollingNeeded) {
				viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
				m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
				m_ContentBounds = GetBounds();
			}

			// If the vertical slider didn't kick in the first time, and the horizontal one did,
			// we need to check again if the vertical slider now needs to kick in.
			// If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
			if (m_VSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
				viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);
		}

		public virtual void SetLayoutVertical() {
			UpdateScrollbarLayout();
			m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
			m_ContentBounds = GetBounds();
		}

		public virtual void OnScroll(PointerEventData data) {
			if (!IsActive()) return;

			EnsureLayoutHasRebuilt();
			UpdateBounds();

			var delta = data.scrollDelta;
			// Down is positive for scroll events, while in UI system up is positive.
			delta.y *= -1;
			if (vertical && !horizontal) {
				if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) delta.y = delta.x;
				delta.x = 0;
			}

			if (horizontal && !vertical) {
				if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x)) delta.x = delta.y;
				delta.y = 0;
			}

			var position = m_Content.anchoredPosition;
			position += delta * m_ScrollSensitivity;
			if (m_MovementType == MovementType.Clamped) position += CalculateOffset(position - m_Content.anchoredPosition);

			SetContentAnchoredPosition(position);
			UpdateBounds();
		}

		protected abstract float GetSize(RectTransform item);
		protected abstract float GetDimension(Vector2 vector);
		protected abstract Vector2 GetVector(float value);

		protected virtual bool UpdateItems(Bounds viewBounds, Bounds contentBounds) => false;

		public void ClearCells() {
			if (Application.isPlaying) {
				itemTypeStart = 0;
				itemTypeEnd = 0;
				totalCount = 0;
				for (var i = content.childCount - 1; i >= 0; i--) ViewFactory.ReturnObject(content.GetChild(i));
			}
		}

		public void ScrollToCell(int index, float speed) {
			if (totalCount >= 0 && (index < 0 || index >= totalCount)) {
				UILogger.LogError($"invalid index {index}");
				return;
			}

			StopAllCoroutines();
			if (speed <= 0) {
				RefillCells(index);
				return;
			}

			this.StartThrowingCoroutine(ScrollToCellCoroutine(index, speed));
		}

		private IEnumerator ScrollToCellCoroutine(int index, float speed) {
			var needMoving = true;
			while (needMoving) {
				yield return null;
				if (!m_Dragging) {
					float move = 0;
					if (index < itemTypeStart)
						move = -Time.deltaTime * speed;
					else if (index >= itemTypeEnd)
						move = Time.deltaTime * speed;
					else {
						m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
						var m_ItemBounds = GetBounds4Item(index);
						var offset = 0.0f;
						if (direction == LoopScrollRectDirection.Vertical)
							offset = reverseDirection ? m_ViewBounds.min.y - m_ItemBounds.min.y : m_ViewBounds.max.y - m_ItemBounds.max.y;
						else
							offset = reverseDirection ? m_ItemBounds.max.x - m_ViewBounds.max.x : m_ItemBounds.min.x - m_ViewBounds.min.x;
						// check if we cannot move on
						if (totalCount >= 0) {
							if (offset > 0 && itemTypeEnd == totalCount && !reverseDirection) {
								m_ItemBounds = GetBounds4Item(totalCount - 1);
								// reach bottom
								if ((direction == LoopScrollRectDirection.Vertical && m_ItemBounds.min.y > m_ViewBounds.min.y) ||
									(direction == LoopScrollRectDirection.Horizontal && m_ItemBounds.max.x < m_ViewBounds.max.x)) {
									needMoving = false;
									break;
								}
							}
							else if (offset < 0 && itemTypeStart == 0 && reverseDirection) {
								m_ItemBounds = GetBounds4Item(0);
								if ((direction == LoopScrollRectDirection.Vertical && m_ItemBounds.max.y < m_ViewBounds.max.y) ||
									(direction == LoopScrollRectDirection.Horizontal && m_ItemBounds.min.x > m_ViewBounds.min.x)) {
									needMoving = false;
									break;
								}
							}
						}

						var maxMove = Time.deltaTime * speed;
						if (Mathf.Abs(offset) < maxMove) {
							needMoving = false;
							move = offset;
						}
						else
							move = Mathf.Sign(offset) * maxMove;
					}

					if (move != 0) {
						var offset = GetVector(move);
						content.anchoredPosition += offset;
						m_PrevPosition += offset;
						m_ContentStartPosition += offset;
						UpdateBounds(true);
					}
				}
			}

			StopMovement();
			UpdatePrevData();
		}

		public void RefreshCells() {
			if (Application.isPlaying && isActiveAndEnabled) {
				itemTypeEnd = itemTypeStart;
				// recycle items if we can
				for (var i = 0; i < content.childCount; i++) {
					if (itemTypeEnd < totalCount) {
						DataSource.FillView(content.GetChild(i), itemTypeEnd);
						itemTypeEnd++;
					}
					else {
						ViewFactory.ReturnObject(content.GetChild(i));
						i--;
					}
				}
			}
		}

		public void RefillCellsFromEnd(int offset = 0, bool alignStart = false) {
			if (!Application.isPlaying || ViewFactory == null) return;

			StopMovement();
			itemTypeEnd = reverseDirection ? offset : totalCount - offset;
			itemTypeStart = itemTypeEnd;

			if (totalCount >= 0 && itemTypeStart % contentConstraintCount != 0) itemTypeStart = itemTypeStart / contentConstraintCount * contentConstraintCount;

			ReturnToTempPool(!reverseDirection, m_Content.childCount);

			float sizeToFill = Mathf.Abs(GetDimension(viewRect.rect.size)), sizeFilled = 0;

			while (sizeToFill > sizeFilled) {
				var size = reverseDirection ? NewItemAtEnd() : NewItemAtStart();
				if (size <= 0) break;
				sizeFilled += size;
			}

			// refill from start in case not full yet
			while (sizeToFill > sizeFilled) {
				var size = reverseDirection ? NewItemAtStart() : NewItemAtEnd();
				if (size <= 0) break;
				sizeFilled += size;
			}

			var pos = m_Content.anchoredPosition;
			var dist = alignStart ? 0 : Mathf.Max(0, sizeFilled - sizeToFill);
			if (reverseDirection) dist = -dist;
			if (direction == LoopScrollRectDirection.Vertical)
				pos.y = dist;
			else
				pos.x = -dist;
			m_Content.anchoredPosition = pos;
			m_ContentStartPosition = pos;

			ClearTempPool();
			UpdateScrollbars(Vector2.zero);
		}

		public void RefillCells(int offset = 0, bool fillViewRect = false) {
			if (!Application.isPlaying || ViewFactory == null) return;

			StopMovement();
			itemTypeStart = reverseDirection ? totalCount - offset : offset;
			if (totalCount >= 0 && itemTypeStart % contentConstraintCount != 0) itemTypeStart = itemTypeStart / contentConstraintCount * contentConstraintCount;

			itemTypeEnd = itemTypeStart;

			// Don't `Canvas.ForceUpdateCanvases();` here, or it will new/delete cells to change itemTypeStart/End
			ReturnToTempPool(reverseDirection, m_Content.childCount);

			float sizeToFill = Mathf.Abs(GetDimension(viewRect.rect.size)), sizeFilled = 0;
			// m_ViewBounds may be not ready when RefillCells on Start

			float itemSize = 0;

			while (sizeToFill > sizeFilled) {
				var size = reverseDirection ? NewItemAtStart() : NewItemAtEnd();
				if (size <= 0) break;
				itemSize = size;
				sizeFilled += size;
			}

			// refill from start in case not full yet
			while (sizeToFill > sizeFilled) {
				var size = reverseDirection ? NewItemAtEnd() : NewItemAtStart();
				if (size <= 0) break;
				sizeFilled += size;
			}

			if (fillViewRect && itemSize > 0 && sizeFilled < sizeToFill) {
				var itemsToAddCount =
					(int)((sizeToFill - sizeFilled) / itemSize); //calculate how many items can be added above the offset, so it still is visible in the view
				var newOffset = offset - itemsToAddCount;
				if (newOffset < 0) newOffset = 0;
				if (newOffset != offset) RefillCells(newOffset); //refill again, with the new offset value, and now with fillViewRect disabled.
			}

			var pos = m_Content.anchoredPosition;
			if (direction == LoopScrollRectDirection.Vertical)
				pos.y = 0;
			else
				pos.x = 0;
			m_Content.anchoredPosition = pos;
			m_ContentStartPosition = pos;

			ClearTempPool();
			UpdateScrollbars(Vector2.zero);
		}

		protected float NewItemAtStart() {
			if (totalCount >= 0 && itemTypeStart - contentConstraintCount < 0) return 0;

			float size = 0;
			for (var i = 0; i < contentConstraintCount; i++) {
				itemTypeStart--;
				var newItem = GetFromTempPool(itemTypeStart);
				newItem.SetSiblingIndex(deletedItemTypeStart);
				size = Mathf.Max(GetSize(newItem), size);
			}

			threshold = Mathf.Max(threshold, size * 1.5f);

			if (!reverseDirection) {
				var offset = GetVector(size);
				content.anchoredPosition += offset;
				m_PrevPosition += offset;
				m_ContentStartPosition += offset;
			}

			return size;
		}

		protected float DeleteItemAtStart() {
			// special case: when moving or dragging, we cannot simply delete start when we've reached the end
			if ((m_Dragging || m_Velocity != Vector2.zero) && totalCount >= 0 && itemTypeEnd >= totalCount - contentConstraintCount) return 0;

			var availableChilds = content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
			Debug.Assert(availableChilds >= 0);
			if (availableChilds == 0) return 0;

			float size = 0;
			for (var i = 0; i < contentConstraintCount; i++) {
				var oldItem = content.GetChild(deletedItemTypeStart) as RectTransform;
				size = Mathf.Max(GetSize(oldItem), size);
				ReturnToTempPool(true);
				availableChilds--;
				itemTypeStart++;

				if (availableChilds == 0) break;
			}

			if (!reverseDirection) {
				var offset = GetVector(size);
				content.anchoredPosition -= offset;
				m_PrevPosition -= offset;
				m_ContentStartPosition -= offset;
			}

			return size;
		}

		protected float NewItemAtEnd() {
			if (totalCount >= 0 && itemTypeEnd >= totalCount) return 0;

			float size = 0;
			// issue 4: fill lines to end first
			var availableChilds = content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
			var count = contentConstraintCount - availableChilds % contentConstraintCount;
			for (var i = 0; i < count; i++) {
				var newItem = GetFromTempPool(itemTypeEnd);
				newItem.SetSiblingIndex(content.childCount - deletedItemTypeEnd - 1);
				size = Mathf.Max(GetSize(newItem), size);
				itemTypeEnd++;
				if (totalCount >= 0 && itemTypeEnd >= totalCount) break;
			}

			threshold = Mathf.Max(threshold, size * 1.5f);

			if (reverseDirection) {
				var offset = GetVector(size);
				content.anchoredPosition -= offset;
				m_PrevPosition -= offset;
				m_ContentStartPosition -= offset;
			}

			return size;
		}

		protected float DeleteItemAtEnd() {
			if ((m_Dragging || m_Velocity != Vector2.zero) && totalCount >= 0 && itemTypeStart < contentConstraintCount) return 0;

			var availableChilds = content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
			Debug.Assert(availableChilds >= 0);
			if (availableChilds == 0) return 0;

			float size = 0;
			for (var i = 0; i < contentConstraintCount; i++) {
				var oldItem = content.GetChild(content.childCount - deletedItemTypeEnd - 1) as RectTransform;
				size = Mathf.Max(GetSize(oldItem), size);
				ReturnToTempPool(false);
				availableChilds--;
				itemTypeEnd--;
				if (itemTypeEnd % contentConstraintCount == 0 || availableChilds == 0) break; //just delete the whole row
			}

			if (reverseDirection) {
				var offset = GetVector(size);
				content.anchoredPosition += offset;
				m_PrevPosition += offset;
				m_ContentStartPosition += offset;
			}

			return size;
		}

		protected RectTransform GetFromTempPool(int itemIdx) {
			RectTransform nextItem = null;
			if (deletedItemTypeStart > 0) {
				deletedItemTypeStart--;
				nextItem = content.GetChild(0) as RectTransform;
				nextItem.SetSiblingIndex(itemIdx - itemTypeStart + deletedItemTypeStart);
			}
			else if (deletedItemTypeEnd > 0) {
				deletedItemTypeEnd--;
				nextItem = content.GetChild(content.childCount - 1) as RectTransform;
				nextItem.SetSiblingIndex(itemIdx - itemTypeStart + deletedItemTypeStart);
			}
			else {
				nextItem = ViewFactory.GetObject().transform as RectTransform;
				nextItem.transform.SetParent(content, false);
				nextItem.transform.SetLocalScale(1);
				nextItem.gameObject.SetActive(true);
			}

			DataSource.FillView(nextItem, itemIdx);
			return nextItem;
		}

		protected void ReturnToTempPool(bool fromStart, int count = 1) {
			if (fromStart)
				deletedItemTypeStart += count;
			else
				deletedItemTypeEnd += count;
		}

		protected void ClearTempPool() {
			while (deletedItemTypeStart > 0) {
				deletedItemTypeStart--;
				ViewFactory.ReturnObject(content.GetChild(0));
			}

			while (deletedItemTypeEnd > 0) {
				deletedItemTypeEnd--;
				ViewFactory.ReturnObject(content.GetChild(content.childCount - 1));
			}
		}

		private void UpdateCachedData() {
			var transform = this.transform;
			m_HorizontalScrollbarRect = m_HorizontalScrollbar == null ? null : m_HorizontalScrollbar.transform as RectTransform;
			m_VerticalScrollbarRect = m_VerticalScrollbar == null ? null : m_VerticalScrollbar.transform as RectTransform;

			// These are true if either the elements are children, or they don't exist at all.
			var viewIsChild = viewRect.parent == transform;
			var hScrollbarIsChild = !m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform;
			var vScrollbarIsChild = !m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform;
			var allAreChildren = viewIsChild && hScrollbarIsChild && vScrollbarIsChild;

			m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect && horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
			m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect && verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
			m_HSliderHeight = m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height;
			m_VSliderWidth = m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width;
		}

		public override bool IsActive() => base.IsActive() && m_Content != null;

		private void EnsureLayoutHasRebuilt() {
			if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout()) Canvas.ForceUpdateCanvases();
		}

		public virtual void StopMovement() {
			m_Velocity = Vector2.zero;
		}

		protected virtual void SetContentAnchoredPosition(Vector2 position) {
			if (!m_Horizontal) position.x = m_Content.anchoredPosition.x;
			if (!m_Vertical) position.y = m_Content.anchoredPosition.y;

			if ((position - m_Content.anchoredPosition).sqrMagnitude > 0.001f) {
				m_Content.anchoredPosition = position;
				UpdateBounds(true);
			}
		}

		private void UpdatePrevData() {
			if (m_Content == null)
				m_PrevPosition = Vector2.zero;
			else
				m_PrevPosition = m_Content.anchoredPosition;
			m_PrevViewBounds = m_ViewBounds;
			m_PrevContentBounds = m_ContentBounds;
		}

		private void UpdateScrollbars(Vector2 offset) {
			if (m_HorizontalScrollbar) {
				//==========LoopScrollRect==========
				if (m_ContentBounds.size.x > 0 && totalCount > 0) {
					var elementSize = (m_ContentBounds.size.x - contentSpacing * (CurrentLines - 1)) / CurrentLines;
					var totalSize = elementSize * TotalLines + contentSpacing * (TotalLines - 1);
					m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / totalSize);
				}
				//==========LoopScrollRect==========
				else
					m_HorizontalScrollbar.size = 1;

				m_HorizontalScrollbar.value = horizontalNormalizedPosition;
			}

			if (m_VerticalScrollbar) {
				//==========LoopScrollRect==========
				if (m_ContentBounds.size.y > 0 && totalCount > 0) {
					var elementSize = (m_ContentBounds.size.y - contentSpacing * (CurrentLines - 1)) / CurrentLines;
					var totalSize = elementSize * TotalLines + contentSpacing * (TotalLines - 1);
					m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / totalSize);
				}
				//==========LoopScrollRect==========
				else
					m_VerticalScrollbar.size = 1;

				m_VerticalScrollbar.value = verticalNormalizedPosition;
			}
		}

		private void SetHorizontalNormalizedPosition(float value) {
			SetNormalizedPosition(value, 0);
		}

		private void SetVerticalNormalizedPosition(float value) {
			SetNormalizedPosition(value, 1);
		}

		private void SetNormalizedPosition(float value, int axis) {
			//==========LoopScrollRect==========
			if (totalCount <= 0 || itemTypeEnd <= itemTypeStart) return;
			//==========LoopScrollRect==========

			EnsureLayoutHasRebuilt();
			UpdateBounds();

			//==========LoopScrollRect==========
			var localPosition = m_Content.localPosition;
			var newLocalPosition = localPosition[axis];
			if (axis == 0) {
				var elementSize = (m_ContentBounds.size.x - contentSpacing * (CurrentLines - 1)) / CurrentLines;
				var totalSize = elementSize * TotalLines + contentSpacing * (TotalLines - 1);
				var offset = m_ContentBounds.min.x - elementSize * StartLine - contentSpacing * StartLine;

				newLocalPosition += m_ViewBounds.min.x - value * (totalSize - m_ViewBounds.size[axis]) - offset;
			}
			else if (axis == 1) {
				var elementSize = (m_ContentBounds.size.y - contentSpacing * (CurrentLines - 1)) / CurrentLines;
				var totalSize = elementSize * TotalLines + contentSpacing * (TotalLines - 1);
				var offset = m_ContentBounds.max.y + elementSize * StartLine + contentSpacing * StartLine;

				newLocalPosition -= offset - value * (totalSize - m_ViewBounds.size.y) - m_ViewBounds.max.y;
			}
			//==========LoopScrollRect==========

			if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f) {
				localPosition[axis] = newLocalPosition;
				m_Content.localPosition = localPosition;
				m_Velocity[axis] = 0;
				UpdateBounds(true);
			}
		}

		private static float RubberDelta(float overStretching, float viewSize) =>
			(1 - 1 / (Mathf.Abs(overStretching) * 0.55f / viewSize + 1)) * viewSize * Mathf.Sign(overStretching);

		private void UpdateScrollbarVisibility() {
			if (m_VerticalScrollbar && m_VerticalScrollbarVisibility != ScrollbarVisibility.Permanent &&
				m_VerticalScrollbar.gameObject.activeSelf != vScrollingNeeded)
				m_VerticalScrollbar.gameObject.SetActive(vScrollingNeeded);

			if (m_HorizontalScrollbar && m_HorizontalScrollbarVisibility != ScrollbarVisibility.Permanent &&
				m_HorizontalScrollbar.gameObject.activeSelf != hScrollingNeeded)
				m_HorizontalScrollbar.gameObject.SetActive(hScrollingNeeded);
		}

		private void UpdateScrollbarLayout() {
			if (m_VSliderExpand && m_HorizontalScrollbar) {
				m_Tracker.Add(this, m_HorizontalScrollbarRect,
					DrivenTransformProperties.AnchorMinX |
					DrivenTransformProperties.AnchorMaxX |
					DrivenTransformProperties.SizeDeltaX |
					DrivenTransformProperties.AnchoredPositionX);
				m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
				m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
				m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
				if (vScrollingNeeded)
					m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
				else
					m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
			}

			if (m_HSliderExpand && m_VerticalScrollbar) {
				m_Tracker.Add(this, m_VerticalScrollbarRect,
					DrivenTransformProperties.AnchorMinY |
					DrivenTransformProperties.AnchorMaxY |
					DrivenTransformProperties.SizeDeltaY |
					DrivenTransformProperties.AnchoredPositionY);
				m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0);
				m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1);
				m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
				if (hScrollingNeeded)
					m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
				else
					m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
			}
		}

		private void UpdateBounds(bool updateItems = false) {
			m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
			m_ContentBounds = GetBounds();

			if (m_Content == null) return;

			// ============LoopScrollRect============
			// Don't do this in Rebuild
			if (Application.isPlaying && updateItems && UpdateItems(m_ViewBounds, m_ContentBounds)) {
				Canvas.ForceUpdateCanvases();
				m_ContentBounds = GetBounds();
			}
			// ============LoopScrollRect============

			// Make sure content bounds are at least as large as view by adding padding if not.
			// One might think at first that if the content is smaller than the view, scrolling should be allowed.
			// However, that's not how scroll views normally work.
			// Scrolling is *only* possible when content is *larger* than view.
			// We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
			// E.g. if pivot is at top, bounds are expanded downwards.
			// This also works nicely when ContentSizeFitter is used on the content.
			var contentSize = m_ContentBounds.size;
			var contentPos = m_ContentBounds.center;
			var excess = m_ViewBounds.size - contentSize;
			if (excess.x > 0) {
				contentPos.x -= excess.x * (m_Content.pivot.x - 0.5f);
				contentSize.x = m_ViewBounds.size.x;
			}

			if (excess.y > 0) {
				contentPos.y -= excess.y * (m_Content.pivot.y - 0.5f);
				contentSize.y = m_ViewBounds.size.y;
			}

			m_ContentBounds.size = contentSize;
			m_ContentBounds.center = contentPos;
		}

		private Bounds GetBounds() {
			if (m_Content == null) return new Bounds();

			var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			var toLocal = viewRect.worldToLocalMatrix;
			m_Content.GetWorldCorners(m_Corners);
			for (var j = 0; j < 4; j++) {
				var v = toLocal.MultiplyPoint3x4(m_Corners[j]);
				vMin = Vector3.Min(v, vMin);
				vMax = Vector3.Max(v, vMax);
			}

			var bounds = new Bounds(vMin, Vector3.zero);
			bounds.Encapsulate(vMax);
			return bounds;
		}

		private Bounds GetBounds4Item(int index) {
			if (m_Content == null) return new Bounds();

			var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			var toLocal = viewRect.worldToLocalMatrix;
			var offset = index - itemTypeStart;
			if (offset < 0 || offset >= m_Content.childCount) return new Bounds();
			var rt = m_Content.GetChild(offset) as RectTransform;
			if (rt == null) return new Bounds();
			rt.GetWorldCorners(m_Corners);
			for (var j = 0; j < 4; j++) {
				var v = toLocal.MultiplyPoint3x4(m_Corners[j]);
				vMin = Vector3.Min(v, vMin);
				vMax = Vector3.Max(v, vMax);
			}

			var bounds = new Bounds(vMin, Vector3.zero);
			bounds.Encapsulate(vMax);
			return bounds;
		}

		private Vector2 CalculateOffset(Vector2 delta) {
			var offset = Vector2.zero;
			if (m_MovementType == MovementType.Unrestricted) return offset;
			if (m_MovementType == MovementType.Clamped) {
				if (totalCount < 0) return offset;
				if (GetDimension(delta) < 0 && itemTypeStart > 0) return offset;
				if (GetDimension(delta) > 0 && itemTypeEnd < totalCount) return offset;
			}

			Vector2 min = m_ContentBounds.min;
			Vector2 max = m_ContentBounds.max;

			if (m_Horizontal) {
				min.x += delta.x;
				max.x += delta.x;
				if (min.x > m_ViewBounds.min.x)
					offset.x = m_ViewBounds.min.x - min.x;
				else if (max.x < m_ViewBounds.max.x) offset.x = m_ViewBounds.max.x - max.x;
			}

			if (m_Vertical) {
				min.y += delta.y;
				max.y += delta.y;
				if (max.y < m_ViewBounds.max.y)
					offset.y = m_ViewBounds.max.y - max.y;
				else if (min.y > m_ViewBounds.min.y) offset.y = m_ViewBounds.min.y - min.y;
			}

			return offset;
		}

		protected void SetDirty() {
			if (!IsActive()) return;

			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}

		protected void SetDirtyCaching() {
			if (!IsActive()) return;

			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}

		private class EmptyModelSource : ILoopScrollModelSource {

			public static readonly EmptyModelSource Instance = new();

			public void FillView(Transform viewTransform, int index) { }

		}

		protected enum LoopScrollRectDirection {

			Vertical,
			Horizontal

		}

		[Serializable]
		public class ScrollRectEvent : UnityEvent<Vector2> { }

	}

}