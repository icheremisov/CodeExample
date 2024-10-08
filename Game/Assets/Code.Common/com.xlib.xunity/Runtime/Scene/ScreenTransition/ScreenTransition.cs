using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Debug = UnityEngine.Debug;
using RTH = UnityEngine.Rendering.RTHandle;

namespace XLib.Unity.Scene.ScreenTransition {

    internal class UrpRendererInternal {
        private ScriptableRenderer _renderer;
        private Func<RTH>          _getBackBufferDelegate;
        private Func<RTH>          _getAfterPostColorDelegate;

        public void CacheRenderer(ScriptableRenderer renderer) {
            if (_renderer == renderer) return;

            _renderer = renderer;
            
            const string backBufferMethodName = "PeekBackBuffer";

            if (renderer is UniversalRenderer ur) {
                var cbs = ur.GetType().GetField("m_ColorBufferSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(ur);
                if (cbs == null) return;
                var gbb = cbs.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .First(m => m.Name == backBufferMethodName && m.GetParameters().Length == 0);

                _getBackBufferDelegate = (Func<RTH>)gbb.CreateDelegate(typeof(Func<RTH>), cbs);
            }
            else {
                _getAfterPostColorDelegate = (Func<RTH>)renderer.GetType()
                    .GetProperty("afterPostProcessColorHandle", BindingFlags.NonPublic | BindingFlags.Instance)?
                    .GetGetMethod(true).CreateDelegate(typeof(Func<RTH>), renderer);
            }
        }
        
        public RenderTargetIdentifier GetBackBuffer() {
            Debug.Assert(_getBackBufferDelegate != null);
            return _getBackBufferDelegate.Invoke().nameID;
        }

        public RenderTargetIdentifier GetAfterPostColor() {
            Debug.Assert(_getAfterPostColorDelegate != null);
            return _getAfterPostColorDelegate.Invoke().nameID;
        }
    }

    public enum RenderOrder {
        AfterPostProcessing,
        BeforePostProcessing,
    }

    public class ScreenTransition : ScriptableRendererFeature {
        public RenderOrder _renderOrder = RenderOrder.AfterPostProcessing;

        public bool _disableSourceAfterRender;

        private readonly Dictionary<Camera, ScreenTransitionSource> _stCache = new();

        private UrpRendererInternal _urpRendererInternal;
        private ScreenTransitionRenderPass _pass;

        private RendererType _rendererType;

        public override void Create() {
            _urpRendererInternal = new UrpRendererInternal();

            var renderPassEvent = _renderOrder == RenderOrder.BeforePostProcessing
                                  ? RenderPassEvent.BeforeRenderingPostProcessing
                                  : RenderPassEvent.AfterRenderingPostProcessing;

            _pass = new ScreenTransitionRenderPass(_urpRendererInternal) {
                renderPassEvent = renderPassEvent
            };

            _stCache.Clear();
        }

        private void Setup(ScriptableRenderer renderer, in RenderingData renderingData) {
            var cameraData = renderingData.cameraData;
            var sts        = GetSts(cameraData.camera);

            if (sts == null) return;

            _urpRendererInternal.CacheRenderer(renderer);

            _rendererType = renderer is UniversalRenderer ? RendererType.Universal : RendererType.Renderer2D;

            var passData = new StsPassData {
                RendererType = _rendererType,
                CameraColorTarget = renderer.cameraColorTargetHandle,
                RenderOrder = _renderOrder,
                Source = sts,
            };

            _pass.Setup(passData);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
            Setup(renderer, renderingData);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData  renderingData) {
            var camera = renderingData.cameraData.camera;
            var sts= GetSts(camera);

            if (sts == null || !sts.enabled) return;
            
            renderer.EnqueuePass(_pass);
        }

        private ScreenTransitionSource GetSts(Camera camera) {
            if (!_stCache.ContainsKey(camera)) {
                _stCache.Add(camera, camera.GetComponent<ScreenTransitionSource>());
            }

            return _stCache[camera];
        }
        
    }
}
