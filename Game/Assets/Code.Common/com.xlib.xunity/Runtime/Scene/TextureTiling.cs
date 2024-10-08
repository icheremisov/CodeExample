using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Unity.Scene {

	[RequireComponent(typeof(Renderer))]
	public class TextureTiling : MonoBehaviour {

		private static readonly int MainTex = Shader.PropertyToID("_MainTex");
		[SerializeField] private Vector2 _textureSize = new(1, 1);

		private void Awake() {
			ApplyToMaterial();
		}

		[Button]
		private void ApplyToMaterial() {
			var r = this.GetExistingComponent<Renderer>();

			var mat = Application.isPlaying ? r.material : r.sharedMaterial;

			if (mat != null) {
				var boundsSize = r.bounds.size;

				var scale = new Vector2(boundsSize.x / _textureSize.x, boundsSize.y / _textureSize.y);
				mat.SetTextureScale(MainTex, scale);
			}
		}

	}

}