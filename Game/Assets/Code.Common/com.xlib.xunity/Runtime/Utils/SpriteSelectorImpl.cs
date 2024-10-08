#if UNITY_EDITOR

using System;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using XLib.Core.Reflection;

namespace XLib.Unity.Utils {

	public abstract class SpriteSelector {

		public abstract Sprite Get(object obj);
		public abstract void Set(object obj, Sprite sprite);

		public static SpriteSelector Create<T>(Expression<Func<T, string>> spriteNameProperty) where T : class => new SpriteSelectorImpl<T>(spriteNameProperty);

	}

	internal class SpriteSelectorImpl<T> : SpriteSelector where T : class {

		private readonly Func<T, string> _getValue;

		private readonly Action<T, string> _setValue;
		private Sprite _view;

		public SpriteSelectorImpl(Expression<Func<T, string>> spriteNameProperty) {
			_setValue = spriteNameProperty.ToSetter();
			_getValue = spriteNameProperty.ToGetter();
		}

		public override Sprite Get(object obj) {
			var uiViewName = _getValue((T)obj);

			if (!uiViewName.IsNullOrEmpty() && (_view == null || _view.name != uiViewName)) {
				var textureName = EditorUtils.LoadFirstAsset<Sprite>(uiViewName)?.texture.name;

				if (textureName == null) {
					Debug.LogError($"Cannot load asset '{uiViewName}'");
					return null;
				}

				var atlas = EditorUtils.LoadSingleAsset<Texture>(textureName);
				var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(atlas));
				foreach (var atlasObj in assets) {
					if (!(atlasObj is Sprite sprite)) continue;

					if (sprite.name != uiViewName) continue;

					_view = sprite;
					break;
				}
			}

			return _view;
		}

		public override void Set(object obj, Sprite value) {
			_view = value;
			_setValue((T)obj, _view == null ? string.Empty : _view.name);
		}

	}

}

#endif