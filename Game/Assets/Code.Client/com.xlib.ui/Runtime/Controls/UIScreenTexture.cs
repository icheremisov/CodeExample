using UnityEngine;
using UnityEngine.UI;
using ScreenCapture = XLib.Unity.FX.ScreenCapture;

namespace XLib.UI.Controls {

	[RequireComponent(typeof(RawImage))]
	public class UIScreenTexture : MonoBehaviour {

		private RawImage _image;
		private Texture2D _texture;

		private void Awake() {
			Init();
		}

		private void OnDisable() {
			if (_image) {
				_image.texture = null;
				_image.gameObject.SetActive(false);
			}
		}

		private void Init() {
			if (_image) return;

			_image = this.GetExistingComponent<RawImage>();
			_image.gameObject.SetActive(false);
		}

		public void CaptureScreen(params Camera[] ignoreCameras) {
			Init();

			ScreenCapture.MakeScreenShot(ref _texture, ignoreCameras);
			_image.texture = _texture;
			_image.gameObject.SetActive(true);
		}

	}

}