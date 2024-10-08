using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using XLib.UI.Contracts;
using XLib.Unity.Utils;

namespace XLib.UI.Controls {

	public class WeakImage : Image {
#if UNITY_EDITOR
		[SerializeField, WeakReference] private Sprite _reference;

		public Sprite Reference => _reference;

		protected override void OnValidate()
		{
			base.OnValidate();
			if (Application.IsPlaying(this)) return;
			// hideFlags = HideFlags.DontSaveInBuild;
			if (_reference != null) {
				sprite = null;
				overrideSprite = _reference;
			} else overrideSprite = null;
		}

		[MenuItem("CONTEXT/Image/Convert to WeakImage")]
		public static void LocalizeText(MenuCommand command) {
			var image = (Image)command.context;
			var go = image.gameObject;
			var sprite = image.sprite;
			EditorUtils.ChangeComponentTypeScript<WeakImage>(image);
			go.GetComponent<WeakImage>().SetReference(sprite);
		}

		private void SetReference(Sprite image) {
			_reference = image;
			OnValidate();
		}

		private Vector3[] _corners = new Vector3[4];
		private void OnDrawGizmosSelected() => OnDrawGizmos();
		private void OnDrawGizmos() {
			var tm = transform as RectTransform;
			if (tm == null) return;

			Gizmos.color = Color.magenta * (0.2f + 0.1f * Mathf.Sin(Time.time * 2));
			var size = _corners[2] - _corners[0];
			var center = (_corners[2] + _corners[0])/2;
			Gizmos.DrawCube(center, size);
		}
#endif
	}
}