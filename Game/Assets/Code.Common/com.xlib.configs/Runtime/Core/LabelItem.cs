using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Configs.Core {

	[HideMonoScript]
	public class LabelItem : GameItemComponent {
		[SerializeField] private Color _color;
		public Color Color => _color;

		public override string ToString() => AssetName;

	}

	public static class TagItemUtils {

		public static bool HasAnyLabel(this IReadOnlyCollection<LabelItem> thisTags, IEnumerable<LabelItem> otherTags) => thisTags?.Any(otherTags.Contains) ?? false;
		public static IReadOnlyCollection<LabelItem> GetIntersectLabels(this IReadOnlyCollection<LabelItem> thisTags, IReadOnlyCollection<LabelItem> labels) => thisTags.Where(labels.Contains).ToArray();
		public static bool HasLabel(this IReadOnlyCollection<LabelItem> thisTags, LabelItem tag) => thisTags?.Contains(tag) ?? false;

	}

}