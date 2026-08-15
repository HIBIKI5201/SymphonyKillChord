using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DevelopProducts.ToonShader
{
    public sealed class SilToonShadowFeature : ScriptableRendererFeature
    {
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            CameraType cameraType = renderingData.cameraData.cameraType;
            if (cameraType != CameraType.Game && cameraType != CameraType.SceneView
                || SilToonShadowCaster.Instances.Count == 0)
            {
                // パスを積まないフレームは前フレームのグローバルが残るため、影取得を1に固定させる
                SilToonShadowRenderPass.DisableGlobalShadow();
                return;
            }

            _silToonShadowRenderPass.Setup(_resolution, _depthBias, _normalBias, _boundsPadding, _shadowStrength, _eulerOffset);
            renderer.EnqueuePass(_silToonShadowRenderPass);
        }

        public override void Create()
        {
            // 設定変更でFeatureが作り直された直後は、まだこのフレームのパスが走っていないため無効値から始める
            SilToonShadowRenderPass.DisableGlobalShadow();

            _silToonShadowRenderPass = new SilToonShadowRenderPass();
            _silToonShadowRenderPass.renderPassEvent = _renderPassEvent;
        }

        // 不透明描画より前(シャドウ描画直後)に実行しないとキャラ本体の描画で参照できない
        [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingShadows;

        [SerializeField, Range(256, 4096)] private int _resolution = 1024;

        [Header("Bias (テクセルサイズ比例)")]
        [SerializeField, Range(0f, 10f)] private float _depthBias = 1.0f;
        [SerializeField, Range(0f, 10f)] private float _normalBias = 1.0f;

        [Header("Bounds")]
        [SerializeField] private float _boundsPadding = 0.1f;

        [Header("Shadow")]
        [SerializeField, Range(0f, 1f)] private float _shadowStrength = 1.0f;

        [Header("EulerOffset")]
        [SerializeField] private Vector3 _eulerOffset = Vector3.zero;

        private SilToonShadowRenderPass _silToonShadowRenderPass;
    }
}
