using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XLib.Unity.Scene.ScreenTransition {
    
    [AddComponentMenu("UI/Screen Transition Image")]
    public class ScreenTransitionImage : Image {
        [SerializeField]
        private ScreenTransitionSource _source;
        [Range(0, 1)]
        public float _dissolve;

        public float Dissolve {
            get => _dissolve;
            set {
                _dissolve = value;
                UpdateTransitionMaterial(false);
            }
        }

        private static readonly int DissolvePropId   = Shader.PropertyToID("_Dissolve");
        private static readonly int SourceTexPropId    = Shader.PropertyToID("_SourceTex");

        private Material _materialForRenderingCached;
        private bool _shouldRun;
        private float _oldDissolve;

        protected override void Start() {
            base.Start();

            Dissolve = 1f;
            _oldDissolve = _dissolve;
            
            if (material) {
                //Have to use string comparison as Addressable break object comparision :(
                if (Application.isPlaying && material.shader.name != "UI/Transition") {
                    Debug.LogWarning("Screen Transition Image requires a material using the \"UI/Transition\" shader");
                }
                else if (_source) {
                    material.SetTexture(SourceTexPropId, _source.TransitionScreen);
                }
            }

    #if UNITY_5_6_OR_NEWER
            if (canvas)
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
    #endif
        }

        public void SetSource(ScreenTransitionSource s) {
            if (isActiveAndEnabled && _source != null) _source.Unuse();
            _source = s;
            UpdateTransitionMaterial(true);
            if (isActiveAndEnabled && _source != null) _source.Use();
        }

        private bool Validate() {
            if (!IsActive() || !material) return false;

            return !_source ? false : _source.TransitionScreen;
        }

        public void UpdateTransitionMaterial(bool withTexture) {
            _materialForRenderingCached = materialForRendering;

            if (_source && withTexture) _materialForRenderingCached.SetTexture(SourceTexPropId, _source.TransitionScreen);
            SyncMaterialProperty(DissolvePropId, ref _dissolve, ref _oldDissolve);
        }
        
        private void LateUpdate() {
            if (!_shouldRun) return;
            if (!_source) return;

            UpdateTransitionMaterial(true);
        }

        private void Update() {
            _shouldRun = Validate();
            
            if (!_shouldRun) return;

            if (DissolvePropId == 0) return;

            UpdateTransitionMaterial(false);
        }
        
        private void SyncMaterialProperty(int propId, ref float value, ref float oldValue) {
            var matValue = materialForRendering.GetFloat(propId);
            
            if (Mathf.Abs(matValue - value) > 1e-4) {
                if (Mathf.Abs(value - oldValue) > 1e-4) {
                    if (_materialForRenderingCached)
                        _materialForRenderingCached.SetFloat(propId, value);

                    material.SetFloat(propId, value);
                    SetMaterialDirty();
                }
                else value = matValue;
            }

            oldValue = value;
        }

        protected override void OnEnable() {
            base.OnEnable();
            
            if (_source != null) _source.Use();
            
#if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode) Start();
#endif
        }

        protected override void OnDisable() {
            if (_source != null) _source.Unuse();
            base.OnDisable();
        }
        
#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();
            color = Color.white;

            material = FindDefaultMaterial();
        }

        private static Material FindDefaultMaterial() {
            var guid = AssetDatabase.FindAssets("ui_vfx_transition t:Material l:UITransition");

            if (guid.Length == 0)
                Debug.LogError("Can't find ui_vfx_transition Material");

            var path = AssetDatabase.GUIDToAssetPath(guid[0]);

            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        protected override void OnValidate() {
            base.OnValidate();
            Update();
        }
#endif
        
    }
}
