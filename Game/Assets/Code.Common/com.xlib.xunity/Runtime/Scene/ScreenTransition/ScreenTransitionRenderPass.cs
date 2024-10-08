using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace XLib.Unity.Scene.ScreenTransition {

    internal enum RendererType {
        Universal,
        Renderer2D
    }

    internal struct StsPassData {
        public RendererType           RendererType;
        public RenderTargetIdentifier CameraColorTarget;
        public ScreenTransitionSource Source;
        public RenderOrder            RenderOrder;
    }

    public class ScreenTransitionRenderPass : ScriptableRenderPass {
        private const string ProfilerTag = "Screen Transition Source";

        private readonly UrpRendererInternal _urpRendererInternal;

        private StsPassData  _currentPassData;

        internal ScreenTransitionRenderPass(UrpRendererInternal urpRendererInternal) {
            _urpRendererInternal = urpRendererInternal;
        }

        private RenderTargetIdentifier GetAfterPostColor() {
            return _urpRendererInternal.GetAfterPostColor();
        }

        internal void Setup(StsPassData passData) {
            _currentPassData = passData;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            var cmd = CommandBufferPool.Get(ProfilerTag);
            RenderTargetIdentifier source;
            if (_currentPassData.RendererType == RendererType.Universal) {
                source = _urpRendererInternal.GetBackBuffer();
            }
            else {
                var useAfterPostTex = renderingData.cameraData.postProcessEnabled;
                useAfterPostTex &= _currentPassData.RenderOrder == RenderOrder.AfterPostProcessing;
                source = useAfterPostTex ? GetAfterPostColor() : _currentPassData.CameraColorTarget;
            }

            cmd.Blit(source, _currentPassData.Source.TransitionScreen);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
