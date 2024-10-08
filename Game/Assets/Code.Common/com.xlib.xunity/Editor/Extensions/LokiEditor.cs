using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XLib.Unity.Extensions {

	public class LokiEditor<T> {
		protected abstract class LokiAttribute : OdinAttributeProcessor<T> { }

		protected abstract class LokiAttributesEnumerate : OdinAttributeProcessor<IEnumerable<T>> { }

		protected abstract class LokiProperties : OdinPropertyProcessor<T> { }

		protected abstract class LokiValueDrawer : OdinValueDrawer<T> { }

		protected abstract class LokiAttributeDrawer<TAttribute> : OdinAttributeDrawer<TAttribute, T> where TAttribute : Attribute { }

		protected abstract class SimpleEditor : OdinEditor {

			// public override Texture2D RenderStaticPreview(string assetPath, Object[] objects, int width, int height) {
			// 	var sprite = RenderSpriteFrom(target is T t ? t : default);
			// 	return sprite != null ? OdinUtils.RenderStaticPreview(sprite, Color.white, width, height) : null;
			// }

			protected virtual Sprite RenderSpriteFrom(T speaker) {
				return null;
			}
		}
	}

}