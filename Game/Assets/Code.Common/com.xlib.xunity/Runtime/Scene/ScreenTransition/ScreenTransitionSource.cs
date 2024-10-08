using UnityEngine;

namespace XLib.Unity.Scene.ScreenTransition {

    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("UI/Screen Transition Source")]
    public class ScreenTransitionSource : MonoBehaviour {
        private Camera _camera;
        private int _useCount;

        public RenderTexture TransitionScreen { get; set; }

        internal Camera Cam => _camera ? _camera : _camera = GetComponent<Camera>();


#if UNITY_EDITOR

        protected virtual void OnEnable() {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) Start();
        }
        
#endif

        public void Use() {
            _useCount++;
            if (!enabled) enabled = _useCount > 0;
        }
        
        public void Unuse() {
            _useCount--;
            if (enabled) enabled = _useCount > 0;
        }

        private void Awake() {
            if (Application.isPlaying) enabled = _useCount > 0;
        }

        protected virtual void Start() {
            CreateNewTransitionScreen(Vector2Int.RoundToInt(Cam.pixelRect.size));
        }

        private void OnDestroy() {
            // RT are not released automatically
            if (TransitionScreen) TransitionScreen.Release();
        }

        protected virtual void CreateNewTransitionScreen(Vector2Int camPixelSize) {
            if (TransitionScreen) TransitionScreen.Release();
            
            TransitionScreen = new RenderTexture(camPixelSize.x, camPixelSize.y, 0) {
                antiAliasing = 1,
                useMipMap = false,
                name = $"{gameObject.name} Transition Screen Source",
                filterMode = FilterMode.Bilinear
            };

            TransitionScreen.Create();
        }

        protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination) {
            Graphics.Blit(source, TransitionScreen);
            Graphics.Blit(source, destination);
        }
        
    }
}
