using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace
public static class UIExtensions {
	private const string ColorTag = "<color=#{0}>{1}</color>";
	private static readonly Vector3[] Corners = new Vector3[4];

	public static RectTransform GetRectTransform(this GameObject obj) => (RectTransform)obj.transform;

	public static RectTransform GetRectTransform(this Component obj) => (RectTransform)obj.transform;

	public static Canvas GetTopmostCanvas(this GameObject obj) => GetTopmostCanvas(obj.transform);

	public static Canvas GetTopmostCanvas(this Component obj) {
		var tm = obj.transform;
		var result = tm.GetComponent<Canvas>();

		while (tm.parent != null) {
			tm = tm.parent;

			if (!tm) continue;
			
			var newCanvas = tm.GetComponent<Canvas>();
			if (newCanvas) result = newCanvas;
		}

		return result;
	}

	public static void RecursiveUpdateLayout(this GameObject obj) {
		obj.transform.RecursiveUpdateLayout();
	}

	public static void RecursiveUpdateLayout(this Transform tm) {
		for (int i = 0, c = tm.childCount; i < c; ++i) tm.GetChild(i).RecursiveUpdateLayout();

		if (tm is RectTransform transform) LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
	}

	public static bool NeedVerticalScroll(this ScrollRect scrollRect) => scrollRect.viewport.rect.height < scrollRect.content.rect.height;

	public static bool NeedHorizontalScroll(this ScrollRect scrollRect) => scrollRect.viewport.rect.width < scrollRect.content.rect.width;

	public static Rect GetWorldRect(this RectTransform tm) {
		// 0 - bottom left, 1 - top left, 2 - top right, 3 - bottom right
		// 1--2
		// |  |
		// 0--3

		tm.GetWorldCorners(Corners);
		return new Rect(Corners[0].x, Corners[0].y, Mathf.Abs(Corners[3].x - Corners[0].x), Mathf.Abs(Corners[1].y - Corners[0].y));
	}

	public static Rect GetWorldRectXZ(this RectTransform tm) {
		// 0 - bottom left, 1 - top left, 2 - top right, 3 - bottom right
		// 1--2
		// |  |
		// 0--3

		tm.GetWorldCorners(Corners);
		return new Rect(Corners[0].x, Corners[0].z, Mathf.Abs(Corners[3].x - Corners[0].x), Mathf.Abs(Corners[1].z - Corners[0].z));
	}

	public static Rect GetOverlayRectByWorld(this RectTransform tm, Camera camera) {
		// 0 - bottom left, 1 - top left, 2 - top right, 3 - bottom right
		// 1--2
		// |  |
		// 0--3

		tm.GetWorldCorners(Corners);

		for (var i = 0; i < Corners.Length; i++) Corners[i] = RectTransformUtility.WorldToScreenPoint(camera, Corners[i]);

		return new Rect(Corners[0].x, Corners[0].y, Mathf.Abs(Corners[3].x - Corners[0].x), Mathf.Abs(Corners[1].y - Corners[0].y));
	}

	public static Rect GetLocalRect(this RectTransform tm) {
		// 0 - bottom left, 1 - top left, 2 - top right, 3 - bottom right
		// 1--2
		// |  |
		// 0--3

		tm.GetLocalCorners(Corners);
		return new Rect(Corners[0].x, Corners[0].y, Mathf.Abs(Corners[3].x - Corners[0].x), Mathf.Abs(Corners[1].y - Corners[0].y));
	}

	/// <summary>
	///     return point in local-space of tm
	/// </summary>
	public static Vector2 GetLocalPoint(this RectTransform tm, Vector2 worldUIPoint, Camera uiCamera = null) {
		if (!uiCamera) uiCamera = tm.GetCanvasCamera();
		var screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, worldUIPoint);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(tm, screenPos, uiCamera, out var localPos);
		return localPos;
	}

	/// <summary>
	///     return rect in local-space of tm
	/// </summary>
	public static Rect ToLocalSpace(this RectTransform tm, Rect worldUIRect, Camera uiCamera = null) {
		if (!uiCamera) uiCamera = tm.GetCanvasCamera();
		var lt = tm.GetLocalPoint(worldUIRect.position, uiCamera);
		var rb = tm.GetLocalPoint(worldUIRect.position + worldUIRect.size, uiCamera);
		return new Rect(lt, rb - lt);
	}

	/// <summary>
	///     return rect of other transform in local-space
	/// </summary>
	public static Rect ToLocalSpace(this RectTransform tm, RectTransform otherTM, Camera uiCamera) => tm.ToLocalSpace(otherTM.GetWorldRect(), uiCamera);

	/// <summary>
	///     try align scroll rect to show item on screen
	/// </summary>
	public static void ScrollToView(this ScrollRect pScroll, Component child, bool centered = false, float duration = 0f) {
		if (pScroll == null || child == null) return;

		ScrollToView(pScroll, (RectTransform)child.transform, centered, duration);
	}

	/// <summary>
	///     try align scroll rect to show item on screen
	/// </summary>
	public static void ScrollToView(this ScrollRect pScroll, GameObject child, bool centered = false, float duration = 0f) {
		if (pScroll == null || child == null) return;

		ScrollToView(pScroll, (RectTransform)child.transform, centered, duration);
	}
	
	/// <summary>
	///		try enable scrolls in object based on content size
	/// </summary>
	public static void AdjustScrollBasedOnContent(this Transform tm) 
	{
		tm.RecursiveUpdateLayout();
		var scrollRects = tm.GetComponentsInChildren<ScrollRect>(true);
		foreach (var rect in scrollRects)
			EnableScrollRectContent(rect);
	}
	
	/// <summary>
	///		try enable scroll in object based on content size
	/// </summary>
	public static void AdjustScrollBasedOnContent(this ScrollRect pScroll) {
		pScroll.transform.RecursiveUpdateLayout();
		EnableScrollRectContent(pScroll);
	}
	
	/// <summary>
	///		try set scroll value
	/// </summary>
	public static void SetScrollValue(this ScrollRect pScroll, float value = 0f) {
		SetScrollRectNormalizedPosition(pScroll, value);
		SetScrollBarValue(pScroll, value);
	}
	
	/// <summary>
	///		try set scroll value
	/// </summary>
	public static void SetScrollValue(this Transform tm, float value = 0f) {
		var scrollRects = tm.GetComponentsInChildren<ScrollRect>(true);
		foreach (var rect in scrollRects) {
			SetScrollRectNormalizedPosition(rect, value);
			SetScrollBarValue(rect, value);
		}
	}

	/// <summary>
	///		try set scroll rect value
	/// </summary>
	public static void SetScrollRectNormalizedPosition(this ScrollRect pScroll, float value = 0f) {
		if (pScroll.vertical) pScroll.verticalNormalizedPosition = value;
		if (pScroll.horizontal) pScroll.horizontalNormalizedPosition = value;
	}
	
	/// <summary>
	///		try set scroll bar value
	/// </summary>
	public static void SetScrollBarValue(this ScrollRect pScroll, float value = 0f) {
		if (pScroll.verticalScrollbar) pScroll.verticalScrollbar.value = value;
		if (pScroll.horizontalScrollbar) pScroll.horizontalScrollbar.value = value;
	}

	/// <summary>
	///     try move align scroll rect to show item on screen
	/// </summary>
	public static void ScrollToView(this ScrollRect pScroll, RectTransform child, bool centered = false, float duration = 0f) {
		if (pScroll == null || child == null) return;

		ScrollToViewInternal(pScroll, child.GetWorldRect(), centered, duration);
	}

	public static void ScrollToView(this ScrollRect pScroll, Rect childWorldRect, bool centered = false, float duration = 0f) {
		if (pScroll == null) return;

		ScrollToViewInternal(pScroll, childWorldRect, centered, duration);
	}

	private static void EnableScrollRectContent(ScrollRect pScroll) {
		var contentRect = pScroll.content.rect;
		var viewportRect = pScroll.viewport.rect;

		if (pScroll.vertical) {
			pScroll.enabled = contentRect.height > viewportRect.height;
			if (pScroll.verticalScrollbar != null) pScroll.verticalScrollbar.SetActive(pScroll.enabled);
		}

		if (pScroll.horizontal) {
			pScroll.enabled = contentRect.width > viewportRect.width;
			if (pScroll.horizontalScrollbar != null) pScroll.horizontalScrollbar.SetActive(pScroll.enabled);
		}
	}

	private static void ScrollToViewInternal(ScrollRect pScroll, Rect childRect, bool centered, float duration) {
		if (pScroll == null) return;

		pScroll.DOKill();

		var scrollTm = (RectTransform)pScroll.transform;
		scrollTm.RecursiveUpdateLayout();

		var viewportRect = (pScroll.viewport != null ? pScroll.viewport : (RectTransform)pScroll.transform).GetWorldRect(); 
		var contentRect = pScroll.content.GetWorldRect();

		if (pScroll.horizontal && pScroll.vertical) {
			//TODO implement when elements of this type will appear 
			throw new NotImplementedException("Implement when elements of this type will appear");
		}

		if (pScroll.horizontal) {
			if (!centered && childRect.xMin >= viewportRect.xMin && childRect.xMax <= viewportRect.xMax) return;
			if (contentRect.width <= viewportRect.width) return; 
			
			if (centered) {
				childRect = new Rect(childRect.center.x - viewportRect.width * 0.5f, childRect.y, viewportRect.width, childRect.height);				
			}

			var newPosition = childRect.xMin;
			var wasChanged = false;

			if (childRect.xMin <= viewportRect.xMin) {
				newPosition = childRect.xMin;
				wasChanged = true;
			}
			else if (childRect.xMax >= viewportRect.xMax) {
				newPosition = childRect.xMax - viewportRect.width;
				wasChanged = true;
			}

			if (!wasChanged) return;

			var normalizedPosition = (newPosition - contentRect.xMin) / (contentRect.width - viewportRect.width); 
			normalizedPosition = Mathf.Clamp01(normalizedPosition);

			if (duration <= 0)
				pScroll.horizontalNormalizedPosition = normalizedPosition;
			else
				pScroll.DOHorizontalNormalizedPos(normalizedPosition, duration).SetEase(Ease.Linear).SetUpdate(false);
		}
		else if (pScroll.vertical) {
			if (!centered && childRect.yMin >= viewportRect.yMin && childRect.yMax <= viewportRect.yMax) return;
			if (contentRect.height <= viewportRect.height) return; 
			
			if (centered) {
				childRect = new Rect(childRect.x, childRect.center.y - viewportRect.height * 0.5f, childRect.width, viewportRect.height);				
			}

			var newPosition = childRect.yMin;
			var wasChanged = false;

			if (childRect.yMin <= viewportRect.yMin) {
				newPosition = childRect.yMin;
				wasChanged = true;
			}
			else if (childRect.yMax >= viewportRect.yMax) {
				newPosition = childRect.yMax - viewportRect.height;
				wasChanged = true;
			}

			if (!wasChanged) return;

			var normalizedPosition = (newPosition - contentRect.yMin) / (contentRect.height - viewportRect.height); 
			normalizedPosition = Mathf.Clamp01(normalizedPosition);

			if (duration <= 0)
				pScroll.verticalNormalizedPosition = normalizedPosition;
			else
				pScroll.DOVerticalNormalizedPos(normalizedPosition, duration).SetEase(Ease.Linear).SetUpdate(false);
		}
	}

	/// <summary>
	///     convert world position from one camera space to another
	/// </summary>
	public static Vector3 ConvertWorldPosition(Vector3 worldPos, Camera srcCamera, Camera targetCamera) {
		var screenPos = srcCamera.WorldToScreenPoint(worldPos);
		return targetCamera.ScreenToWorldPoint(screenPos);
	}

	/// <summary>
	///     convert world rect from one camera space to another
	/// </summary>
	public static Rect ConvertWorldRect(Rect rect, Camera srcCamera, Camera targetCamera) {
		var min = ConvertWorldPosition(rect.position, srcCamera, targetCamera);
		var max = ConvertWorldPosition(rect.position + rect.size, srcCamera, targetCamera);

		return new Rect(min, max - min);
	}

	/// <summary>
	///     convert screen rect to camera space on XY plane
	/// </summary>
	public static Rect ScreenToWorldRectXY(Rect rect, Camera targetCamera) {
		var min = targetCamera.ScreenToWorldPoint(rect.position).ToXY();
		var max = targetCamera.ScreenToWorldPoint(rect.position + rect.size).ToXY();

		return new Rect(min, max - min);
	}

	/// <summary>
	///     set sprite if sprite == null => disable image obj
	/// </summary>
	public static void SetSprite(this Image image, Sprite sprite) {
		image.sprite = sprite;
		image.gameObject.SetActive(sprite != null);
	}

	/// <summary>
	///     set image alpha color
	/// </summary>
	public static void SetAlpha(this Image image, float a) {
		var color = image.color;
		color.a = a;
		image.color = color;
	}

	/// <summary>
	///  get UI camera of topmost canvas
	/// </summary>
	public static Camera GetCanvasCamera(this RectTransform tm) => tm.GetTopmostCanvas().worldCamera;

	/// <summary>
	///  get UI camera of topmost canvas
	/// </summary>
	public static Camera GetCanvasCamera(this Component tm) => tm.GetTopmostCanvas().worldCamera;
}