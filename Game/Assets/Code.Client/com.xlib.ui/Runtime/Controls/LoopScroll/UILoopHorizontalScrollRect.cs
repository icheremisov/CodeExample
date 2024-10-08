using UnityEngine;
using UnityEngine.UI;

namespace XLib.UI.Controls.LoopScroll {

	[AddComponentMenu("UI/Loop Horizontal Scroll Rect", 50), DisallowMultipleComponent]
	public class UILoopHorizontalScrollRect : UILoopScrollRect {

		protected override void Awake() {
			direction = LoopScrollRectDirection.Horizontal;
			base.Awake();

			var layout = content.GetComponent<GridLayoutGroup>();
			if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedRowCount)
				UILogger.LogError("[LoopHorizontalScrollRect] unsupported GridLayoutGroup constraint");
		}

		protected override float GetSize(RectTransform item) {
			var size = contentSpacing;
			if (m_GridLayout != null)
				size += m_GridLayout.cellSize.x;
			else
				size += LayoutUtility.GetPreferredWidth(item);

			return size;
		}

		protected override float GetDimension(Vector2 vector) => -vector.x;

		protected override Vector2 GetVector(float value) => new(-value, 0);

		protected override bool UpdateItems(Bounds viewBounds, Bounds contentBounds) {
			var changed = false;

			// special case: handling move several page in one frame
			if (viewBounds.max.x < contentBounds.min.x && itemTypeEnd > itemTypeStart) {
				var currentSize = contentBounds.size.x;
				var elementSize = (currentSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
				ReturnToTempPool(false, itemTypeEnd - itemTypeStart);
				itemTypeEnd = itemTypeStart;

				var offsetCount = Mathf.FloorToInt((contentBounds.min.x - viewBounds.max.x) / (elementSize + contentSpacing));
				if (totalCount >= 0 && itemTypeStart - offsetCount * contentConstraintCount < 0) offsetCount = Mathf.FloorToInt((float)itemTypeStart / contentConstraintCount);

				itemTypeStart -= offsetCount * contentConstraintCount;
				if (totalCount >= 0) itemTypeStart = Mathf.Max(itemTypeStart, 0);

				itemTypeEnd = itemTypeStart;

				var offset = offsetCount * (elementSize + contentSpacing);
				content.anchoredPosition -= new Vector2(offset + (reverseDirection ? currentSize : 0), 0);
				contentBounds.center -= new Vector3(offset + currentSize / 2, 0, 0);
				contentBounds.size = Vector3.zero;

				changed = true;
			}

			if (viewBounds.min.x > contentBounds.max.x && itemTypeEnd > itemTypeStart) {
				var maxItemTypeStart = -1;
				if (totalCount >= 0) {
					maxItemTypeStart = Mathf.Max(0, totalCount - (itemTypeEnd - itemTypeStart));
					maxItemTypeStart = maxItemTypeStart / contentConstraintCount * contentConstraintCount;
				}

				var currentSize = contentBounds.size.x;
				var elementSize = (currentSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
				ReturnToTempPool(true, itemTypeEnd - itemTypeStart);
				// TODO: fix with contentConstraint?
				itemTypeStart = itemTypeEnd;

				var offsetCount = Mathf.FloorToInt((viewBounds.min.x - contentBounds.max.x) / (elementSize + contentSpacing));
				if (maxItemTypeStart >= 0 && itemTypeStart + offsetCount * contentConstraintCount > maxItemTypeStart)
					offsetCount = Mathf.FloorToInt((float)(maxItemTypeStart - itemTypeStart) / contentConstraintCount);

				itemTypeStart += offsetCount * contentConstraintCount;
				if (totalCount >= 0) itemTypeStart = Mathf.Max(itemTypeStart, 0);

				itemTypeEnd = itemTypeStart;

				var offset = offsetCount * (elementSize + contentSpacing);
				content.anchoredPosition += new Vector2(offset + (reverseDirection ? 0 : currentSize), 0);
				contentBounds.center += new Vector3(offset + currentSize / 2, 0, 0);
				contentBounds.size = Vector3.zero;

				changed = true;
			}

			if (viewBounds.max.x < contentBounds.max.x - threshold) {
				float size = DeleteItemAtEnd(), totalSize = size;
				while (size > 0 && viewBounds.max.x < contentBounds.max.x - threshold - totalSize) {
					size = DeleteItemAtEnd();
					totalSize += size;
				}

				if (totalSize > 0) changed = true;
			}

			if (viewBounds.min.x > contentBounds.min.x + threshold) {
				float size = DeleteItemAtStart(), totalSize = size;
				while (size > 0 && viewBounds.min.x > contentBounds.min.x + threshold + totalSize) {
					size = DeleteItemAtStart();
					totalSize += size;
				}

				if (totalSize > 0) changed = true;
			}

			if (viewBounds.max.x > contentBounds.max.x) {
				float size = NewItemAtEnd(), totalSize = size;
				while (size > 0 && viewBounds.max.x > contentBounds.max.x + totalSize) {
					size = NewItemAtEnd();
					totalSize += size;
				}

				if (totalSize > 0) changed = true;
			}

			if (viewBounds.min.x < contentBounds.min.x) {
				float size = NewItemAtStart(), totalSize = size;
				while (size > 0 && viewBounds.min.x < contentBounds.min.x - totalSize) {
					size = NewItemAtStart();
					totalSize += size;
				}

				if (totalSize > 0) changed = true;
			}

			if (changed) ClearTempPool();

			return changed;
		}

	}

}