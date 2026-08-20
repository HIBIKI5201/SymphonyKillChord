using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace DevelopProducts.ToonShader
{
    /// <summary>
    /// キャラ専用シャドウマップ(セルフシャドウ用)を描画するパス。
    /// メインライトのシャドウマップとは独立しており、SilToon側で以下のグローバルを参照して合成する。
    ///   _CharShadowmap      : 深度シャドウマップ(比較サンプラーで読む)
    ///   _CharWorldToShadow  : ワールド→シャドウUV空間([0,1] + 深度)
    ///   _CharShadowParams   : x = 影の強度(0で無効), y = 1/解像度
    /// </summary>
    public sealed class SilToonShadowRenderPass : ScriptableRenderPass
    {
        private const string _passName = "SilToonCharacterShadow";
        private const string _shadowCasterPassName = "ShadowCaster";
        private const int _depthBits = 16;

        private static readonly int _idCharShadowmap = Shader.PropertyToID("_CharShadowmap");
        private static readonly int _idCharWorldToShadow = Shader.PropertyToID("_CharWorldToShadow");
        private static readonly int _idCharShadowParams = Shader.PropertyToID("_CharShadowParams");
        private static readonly int _idShadowBias = Shader.PropertyToID("_ShadowBias");
        private static readonly int _idLightDirection = Shader.PropertyToID("_LightDirection");
        private static readonly int _idCharShadowLightDirection = Shader.PropertyToID("_CharShadowLightDirection");

        private static readonly GlobalKeyword _castingPunctualLightShadow =
            GlobalKeyword.Create("_CASTING_PUNCTUAL_LIGHT_SHADOW");

        private readonly struct DrawCall
        {
            public readonly Renderer Renderer;
            public readonly Material Material;
            public readonly int SubmeshIndex;
            public readonly int PassIndex;

            public DrawCall(Renderer renderer, Material material, int submeshIndex, int passIndex)
            {
                Renderer = renderer;
                Material = material;
                SubmeshIndex = submeshIndex;
                PassIndex = passIndex;
            }
        }

        private sealed class PassData
        {
            public Matrix4x4 ViewMatrix;
            public Matrix4x4 ProjMatrix;
            public Matrix4x4 WorldToShadow;
            public Vector4 ShadowBias;
            public Vector4 LightDirection;
            public Vector4 ShadowParams;
            public readonly List<DrawCall> DrawCalls = new();
        }

        private readonly List<DrawCall> _frameDrawCalls = new();
        private readonly Dictionary<Material, (Shader Shader, int PassIndex)> _shadowPassIndexCache = new();

        private int _resolution = 1024;
        private float _depthBias = 1.0f;
        private float _normalBias = 1.0f;
        private float _boundsPadding = 0.1f;
        private float _shadowStrength = 1.0f;

        private Quaternion _directionEulerOffset = Quaternion.identity;

        /// <summary>
        /// キャラ影の合成を無効化する。強度0を伝えることでSilToon側は影取得を1(影なし)に固定する。
        /// パスが実行されないフレームでは前フレームの行列とシャドウマップが残るため、必ず呼ぶこと。
        /// </summary>
        public static void DisableGlobalShadow()
        {
            Shader.SetGlobalVector(_idCharShadowParams, Vector4.zero);
        }

        public void Setup(int resolution, float depthBias, float normalBias, float boundsPadding, float shadowStrength, in Vector3 eulerOffset)
        {
            _resolution = Mathf.Max(64, resolution);
            _depthBias = depthBias;
            _normalBias = normalBias;
            _boundsPadding = boundsPadding;
            _shadowStrength = shadowStrength;
            _directionEulerOffset = Quaternion.Euler(eulerOffset);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            CollectDrawCalls(_frameDrawCalls);

            if (_frameDrawCalls.Count == 0 || !TryGetCasterBounds(out Bounds boundsWS))
            {
                // 影なしフレーム: 強度0を伝えてシェーダー側の合成を無効化する
                DisableGlobalShadow();
                return;
            }

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            Vector3 lightDirWS = GetShadowLightDirection(_directionEulerOffset, cameraData.camera.transform.position, boundsWS.center);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(_passName, out PassData passData))
            {
                passData.DrawCalls.Clear();
                passData.DrawCalls.AddRange(_frameDrawCalls);
                SetupShadowMatrices(passData, lightDirWS, boundsWS);

                var descriptor = new RenderTextureDescriptor(_resolution, _resolution,
                    GraphicsFormat.None, GraphicsFormatUtility.GetDepthStencilFormat(_depthBits, 0))
                {
                    shadowSamplingMode = ShadowSamplingMode.CompareDepths,
                };
                TextureHandle shadowmap = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph, descriptor, "_CharShadowmap", true, FilterMode.Bilinear);

                builder.SetRenderAttachmentDepth(shadowmap, AccessFlags.Write);
                // グローバル定数の書き込みとシェーダーからの参照はRenderGraphが追跡できないため明示する
                builder.AllowGlobalStateModification(true);
                builder.AllowPassCulling(false);
                builder.SetGlobalTextureAfterPass(shadowmap, _idCharShadowmap);

                builder.SetRenderFunc<PassData>(static (data, context) => ExecutePass(data, context.cmd));
            }
        }

        private static void ExecutePass(PassData data, RasterCommandBuffer cmd)
        {
            // 直前のAdditionalLightsShadowでONにされた状態が残っている可能性があるため明示的にOFF
            cmd.SetKeyword(_castingPunctualLightShadow, false);

            // ShadowCasterPass.hlsl (ApplyShadowBias) が参照するグローバル。
            // セットしないとUnityのカスケード用の値が漏れて使われる
            cmd.SetGlobalVector(_idShadowBias, data.ShadowBias);
            cmd.SetGlobalVector(_idLightDirection, data.LightDirection);

            cmd.SetViewProjectionMatrices(data.ViewMatrix, data.ProjMatrix);

            foreach (DrawCall drawCall in data.DrawCalls)
            {
                cmd.DrawRenderer(drawCall.Renderer, drawCall.Material, drawCall.SubmeshIndex, drawCall.PassIndex);
            }

            cmd.SetGlobalMatrix(_idCharWorldToShadow, data.WorldToShadow);
            cmd.SetGlobalVector(_idCharShadowParams, data.ShadowParams);
            cmd.SetGlobalVector(_idCharShadowLightDirection, data.LightDirection);
        }

        private void SetupShadowMatrices(PassData data, in Vector3 lightDirWS, in Bounds boundsWS)
        {
            float radius = boundsWS.extents.magnitude + _boundsPadding;
            float texelSize = radius * 2f / _resolution;

            Vector3 upWS = Mathf.Abs(Vector3.Dot(lightDirWS, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
            Quaternion lightRotation = Quaternion.LookRotation(lightDirWS, upWS);

            // キャラ移動で影の輪郭がチラつかないよう、ライト空間で中心をテクセル格子にスナップする
            Vector3 centerLS = Quaternion.Inverse(lightRotation) * boundsWS.center;
            centerLS.x = Mathf.Round(centerLS.x / texelSize) * texelSize;
            centerLS.y = Mathf.Round(centerLS.y / texelSize) * texelSize;
            Vector3 centerWS = lightRotation * centerLS;

            Matrix4x4 view = Matrix4x4.TRS(centerWS - lightDirWS * radius, lightRotation, Vector3.one).inverse;
            // Unityのビュー行列は-Zが前方(worldToCameraMatrix互換)のためZ行を反転する
            view.SetRow(2, -view.GetRow(2));

            // SetViewProjectionMatricesがバインド時にプラットフォーム変換(Reversed-Z等)を行うため素の行列を渡す
            Matrix4x4 proj = Matrix4x4.Ortho(-radius, radius, -radius, radius, 0f, radius * 2f);

            data.ViewMatrix = view;
            data.ProjMatrix = proj;
            data.WorldToShadow = GetShadowTransform(proj, view);

            // ApplyShadowBiasは「表面からライトへ向かう」方向を期待する
            data.LightDirection = -lightDirWS;
            // URPのShadowUtils.GetShadowBiasに合わせ、テクセルサイズ比例の負バイアス
            data.ShadowBias = new Vector4(-_depthBias * texelSize, -_normalBias * texelSize, 0f, 0f);
            data.ShadowParams = new Vector4(_shadowStrength, 1f / _resolution, 0f, 0f);
        }

        /// <summary>
        /// サンプリング用のワールド→シャドウ空間行列を作る。
        /// URPのShadowUtils.GetShadowTransformと同一(internalのため複製)。
        /// </summary>
        private static Matrix4x4 GetShadowTransform(Matrix4x4 proj, in Matrix4x4 view)
        {
            // 描画側はSetViewProjectionMatricesが自動でZ反転するが、
            // サンプリング側の行列は手動で反転する必要がある
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }

            Matrix4x4 worldToShadow = proj * view;

            var textureScaleAndBias = Matrix4x4.identity;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;

            return textureScaleAndBias * worldToShadow;
        }

        /// <summary>
        /// メインライトの実方向ではなく、メインカメラのヨーに追従する固定仰角の擬似ライト方向を使う。
        /// カメラがどこを向いてもセルフシャドウの見え方が安定する。
        /// </summary>
        private static Vector3 GetShadowLightDirection(in Quaternion offset, in Vector3 cameraPosition, in Vector3 characterCenter)
        {
            float cameraYaw = Quaternion.LookRotation(characterCenter - cameraPosition).eulerAngles.y;
            return Quaternion.Euler(0f, cameraYaw, 0f) * offset * Vector3.forward;
        }

        private static bool TryGetCasterBounds(out Bounds boundsWS)
        {
            boundsWS = default;

            bool found = false;
            foreach (SilToonShadowCaster caster in SilToonShadowCaster.Instances)
            {
                if (!caster.TryGetBounds(out Bounds casterBounds)) continue;

                if (found)
                {
                    boundsWS.Encapsulate(casterBounds);
                }
                else
                {
                    boundsWS = casterBounds;
                    found = true;
                }
            }

            return found;
        }

        private void CollectDrawCalls(List<DrawCall> drawCalls)
        {
            drawCalls.Clear();

            foreach (SilToonShadowCaster caster in SilToonShadowCaster.Instances)
            {
                Renderer[] renderers = caster.Renderers;
                if (renderers == null) continue;

                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;

                    Material[] materials = renderer.sharedMaterials;
                    for (int submeshIndex = 0; submeshIndex < materials.Length; submeshIndex++)
                    {
                        Material material = materials[submeshIndex];
                        if (material == null) continue;

                        int passIndex = GetShadowCasterPassIndex(material);
                        if (passIndex < 0) continue;

                        drawCalls.Add(new DrawCall(renderer, material, submeshIndex, passIndex));
                    }
                }
            }
        }

        private int GetShadowCasterPassIndex(Material material)
        {
            if (_shadowPassIndexCache.TryGetValue(material, out var entry) && entry.Shader == material.shader)
            {
                return entry.PassIndex;
            }

            int passIndex = material.FindPass(_shadowCasterPassName);
            _shadowPassIndexCache[material] = (material.shader, passIndex);
            return passIndex;
        }
    }
}
