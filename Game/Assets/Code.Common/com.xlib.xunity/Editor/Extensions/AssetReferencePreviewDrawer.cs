using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLib.Unity.Attributes;

namespace XLib.Unity.Extensions {

	public class AssetReferencePreviewDrawer : OdinAttributeDrawer<AssetReferencePreviewAttribute> {
		private bool _visible = false;

		protected override void Initialize() {
			_visible = Attribute.Expanded;
			base.Initialize();
		}

		protected override void DrawPropertyLayout(GUIContent label) {
			GUILayout.BeginHorizontal();
			if (Attribute.Inline) DrawPreview();
			else {
				if (SirenixEditorGUI.IconButton(_visible ? EditorIcons.TriangleDown : EditorIcons.TriangleRight)) 
					_visible = !_visible;
			}

			// if (label != null && !label.text.IsNullOrEmpty())
			// 	GUILayout.Label(label);

			CallNextDrawer(label);
			GUILayout.EndHorizontal();
			if (!Attribute.Inline && _visible) 
				DrawPreview();
		}

		private void DrawPreview() {
			var assetReference = Property.GetValue<AssetReference>();
			var width = Attribute.Inline ? Attribute.Size : EditorGUIUtility.currentViewWidth-30;

			if (assetReference.editorAsset is Sprite sprite)
				GUILayout.Box(sprite.texture, GUILayout.Height(GetHeight(width, sprite.texture.width, sprite.texture.height)), GUILayout.Width(width));

			if (assetReference.editorAsset is Texture texture)
				GUILayout.Box(texture, GUILayout.Height(GetHeight(width, texture.width, texture.height)), GUILayout.Width(width));

			if (assetReference.editorAsset is GameObject gameObject) {
				var texture2D = AssetPreview.GetAssetPreview(gameObject);
				GUILayout.Box(texture2D, GUILayout.Height(GetHeight(width, texture2D.width, texture2D.height)), GUILayout.Width(width));
			}
		}

		private float GetHeight(float width, int textureWidth, int textureHeight) {
			if(textureWidth > width) {
				var ratio = (float)textureHeight / textureWidth;
				return Mathf.Min(width * ratio, Attribute.Size);
			}
			return Mathf.Min(textureHeight, Attribute.Size);
		}

		protected override bool CanDrawAttributeProperty(InspectorProperty property) {
			return base.CanDrawAttributeProperty(property);
		}
	}

}