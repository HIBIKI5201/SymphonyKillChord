using System;
using LitMotion;
using UnityEngine;

namespace KillChord.Runtime.Utility.Rendering
{
    /// <summary>
    ///     LitMotionのモーションをRendererのMaterialPropertyBlockへバインドする拡張です。
    ///     SharedMaterialを維持したまま複数Rendererへ同じ値を反映したい場合に使用します。
    /// </summary>
    public static class LitMotionRendererExtensions
    {
        /// <summary>
        ///     モーションを生成し、複数RendererのMaterialPropertyBlockの同一floatプロパティへ同じ値を反映します。
        ///     MaterialPropertyBlockは1つを使い回すため、Renderer数分アロケートしません。
        /// </summary>
        /// <param name="builder">このビルダーです。</param>
        /// <param name="renderers">対象のRenderer配列です。</param>
        /// <param name="propertyId">Shader.PropertyToIDで取得したプロパティIDです。</param>
        /// <returns>生成されたモーションのハンドルです。</returns>
        public static MotionHandle BindToMaterialPropertyBlockFloat<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder, Renderer[] renderers, int propertyId)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            if (renderers == null)
            {
                throw new ArgumentNullException(nameof(renderers));
            }

            RenderersPropertyBlockBinding binding = new(renderers, propertyId);
            return builder.Bind(binding, static (value, state) => state.SetFloat(value));
        }

        /// <summary>
        ///     複数Rendererとプロパティ IDを保持し、共用MaterialPropertyBlock経由での値反映を仲介します。
        /// </summary>
        private sealed class RenderersPropertyBlockBinding
        {
            private readonly Renderer[] _renderers;
            private readonly int _propertyId;
            private readonly MaterialPropertyBlock _propertyBlock;

            public RenderersPropertyBlockBinding(Renderer[] renderers, int propertyId)
            {
                _renderers = renderers;
                _propertyId = propertyId;
                _propertyBlock = new MaterialPropertyBlock();
            }

            public void SetFloat(float value)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    Renderer renderer = _renderers[i];
                    if (renderer == null)
                    {
                        continue;
                    }

                    renderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetFloat(_propertyId, value);
                    renderer.SetPropertyBlock(_propertyBlock);
                }
            }
        }
    }
}
