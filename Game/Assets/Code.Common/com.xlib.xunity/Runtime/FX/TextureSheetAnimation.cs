using Sirenix.OdinInspector;
using UnityEngine;

namespace XLib.Unity.FX {

	[RequireComponent(typeof(Renderer))]
	public class TextureSheetAnimation : MonoBehaviour {

		[SerializeField] private string _texName = "_MainTex";
		[SerializeField] private Vector2Int _dimensions = new(2, 2);

		[Header("Animation"), SerializeField] private float _fps = 30;
		[SerializeField] private int _startFrame;
		private float _animationTime;
		private int _frame;

		private float _frameTime;
		private Material _material;

		private int _textureId;

		private int NumFrames => _dimensions.x * _dimensions.y;

		private void Awake() {
			_material = this.GetExistingComponent<Renderer>().material;
			Initialize();
		}

		private void Update() {
#if UNITY_EDITOR
			Initialize();
#endif

			_animationTime += Time.deltaTime;

			var skipFrames = Mathf.FloorToInt(_animationTime / _frameTime);
			_animationTime %= _frameTime;

			_frame = (_frame + skipFrames) % NumFrames;

			UpdateFrame(_textureId, _material, _frame);
		}

		private void OnEnable() {
			_animationTime = 0;
			_frame = _startFrame;
			UpdateFrame(_textureId, _material, _frame);
		}

		private void Initialize() {
			_textureId = Shader.PropertyToID(_texName);
			_frameTime = 1.0f / _fps;
			_material.SetTextureScale(_textureId, new Vector2(1.0f / _dimensions.x, 1.0f / _dimensions.y));
		}

		private void UpdateFrame(int texId, Material mat, int frame) {
			var cellX = frame % _dimensions.x;
			var cellY = frame / _dimensions.x;

			mat.SetTextureOffset(texId, new Vector2((float)cellX / _dimensions.x, (float)cellY / _dimensions.y));
		}

#if UNITY_EDITOR
		[Button]
		private void DebugSetFrame(int frame) {
			if (Application.isPlaying) {
				_frame = frame;
				UpdateFrame(_textureId, _material, _frame);
				return;
			}

			var textureId = Shader.PropertyToID(_texName);
			var material = this.GetExistingComponent<Renderer>().sharedMaterial;
			material.SetTextureScale(textureId, new Vector2(1.0f / _dimensions.x, 1.0f / _dimensions.y));

			UpdateFrame(textureId, material, frame);
		}
#endif

	}

}