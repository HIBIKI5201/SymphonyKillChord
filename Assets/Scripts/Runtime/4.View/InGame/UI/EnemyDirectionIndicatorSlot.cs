using LitMotion;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     1つの3Dマーカーとフェード状態を保持する。
    /// </summary>
    internal sealed class EnemyDirectionIndicatorSlot
    {
        /// <summary>
        ///     3DマーカーとRendererを受け取る。
        /// </summary>
        /// <param name="gameObject"> 表示対象のGameObject。 </param>
        /// <param name="renderers"> 透明度を反映するRenderer一覧。 </param>
        public EnemyDirectionIndicatorSlot(
            GameObject gameObject,
            Renderer[] renderers)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            if (renderers == null)
            {
                throw new ArgumentNullException(nameof(renderers));
            }

            GameObject = gameObject;
            Transform = gameObject.transform;
            _renderers = renderers;
            _materialStates = new RendererMaterialState[renderers.Length][];

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    throw new ArgumentException("Renderer一覧にnullが含まれています。", nameof(renderers));
                }

                Material[] materials = renderer.sharedMaterials;
                int materialCount = materials.Length;
                _materialStates[i] = new RendererMaterialState[materialCount];

                for (int materialIndex = 0; materialIndex < materialCount; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (!TryGetColorPropertyId(material, out int colorPropertyId))
                    {
                        throw new ArgumentException(
                            "透明度を制御できるメインカラーがないMaterialが含まれています。",
                            nameof(renderers));
                    }

                    _materialStates[i][materialIndex] = new RendererMaterialState(
                        colorPropertyId,
                        material.GetColor(colorPropertyId));
                }
            }
        }

        /// <summary> 表示対象のGameObject。 </summary>
        public GameObject GameObject { get; }

        /// <summary> 表示対象のTransform。 </summary>
        public Transform Transform { get; }

        /// <summary> 希望表示状態。 </summary>
        public bool IsVisible => _isVisible;

        /// <summary> 最後に確定または読み取った透明度。 </summary>
        public float CurrentAlpha => _currentAlpha;

        /// <summary> フェード完了処理を識別する世代番号。 </summary>
        public int MotionVersion => _motionVersion;

        /// <summary>
        ///     希望表示状態を記録し、Motion世代を進める。
        /// </summary>
        /// <param name="isVisible"> 表示する場合はtrue。 </param>
        /// <returns> 更新後のMotion世代番号。 </returns>
        public int RecordVisibility(bool isVisible)
        {
            _isVisible = isVisible;
            _motionVersion++;
            return _motionVersion;
        }

        /// <summary>
        ///     再生中のフェードMotionを記録する。
        /// </summary>
        /// <param name="motionHandle"> 記録するMotion。 </param>
        public void RecordMotion(MotionHandle motionHandle)
        {
            _motionHandle = motionHandle;
        }

        /// <summary>
        ///     再生中のフェードMotionを取り消す。
        /// </summary>
        public void CancelMotion()
        {
            _motionHandle.TryCancel();
        }

        /// <summary>
        ///     全Rendererへ透明度を即時反映する。
        /// </summary>
        /// <param name="alpha"> 反映する0以上1以下の透明度。 </param>
        public void ApplyAlpha(float alpha)
        {
            if (float.IsNaN(alpha) || float.IsInfinity(alpha))
            {
                throw new ArgumentOutOfRangeException(nameof(alpha));
            }

            alpha = Mathf.Clamp01(alpha);
            _currentAlpha = alpha;
            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer renderer = _renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                RendererMaterialState[] materialStates = _materialStates[i];
                for (int materialIndex = 0; materialIndex < materialStates.Length; materialIndex++)
                {
                    RendererMaterialState state = materialStates[materialIndex];
                    renderer.GetPropertyBlock(state.PropertyBlock, materialIndex);
                    Color color = state.BaseColor;
                    color.a *= alpha;
                    state.PropertyBlock.SetColor(state.ColorPropertyId, color);
                    renderer.SetPropertyBlock(state.PropertyBlock, materialIndex);
                }
            }
        }

        /// <summary>
        ///     Materialの透明度制御に使用するメインカラーを取得する。
        /// </summary>
        /// <param name="material"> 検査するMaterial。 </param>
        /// <param name="colorPropertyId"> メインカラーのShaderプロパティID。 </param>
        /// <returns> 対応するカラープロパティがある場合はtrue。 </returns>
        internal static bool TryGetColorPropertyId(Material material, out int colorPropertyId)
        {
            colorPropertyId = 0;
            Shader shader = material != null ? material.shader : null;
            if (shader == null)
            {
                return false;
            }

            int propertyCount = shader.GetPropertyCount();
            for (int propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                if (shader.GetPropertyType(propertyIndex) != ShaderPropertyType.Color
                    || (shader.GetPropertyFlags(propertyIndex) & ShaderPropertyFlags.MainColor) == 0)
                {
                    continue;
                }

                colorPropertyId = shader.GetPropertyNameId(propertyIndex);
                return material.HasProperty(colorPropertyId);
            }

            return false;
        }

        private readonly Renderer[] _renderers;
        private readonly RendererMaterialState[][] _materialStates;
        private MotionHandle _motionHandle;
        private float _currentAlpha;
        private int _motionVersion;
        private bool _isVisible;

        /// <summary>
        ///     Renderer内の1つのMaterialに対するフェード情報を保持する。
        /// </summary>
        private sealed class RendererMaterialState
        {
            /// <summary>
            ///     カラープロパティと初期色を受け取る。
            /// </summary>
            /// <param name="colorPropertyId"> カラーのShaderプロパティID。 </param>
            /// <param name="baseColor"> フェード前の色。 </param>
            public RendererMaterialState(int colorPropertyId, in Color baseColor)
            {
                ColorPropertyId = colorPropertyId;
                BaseColor = baseColor;
                PropertyBlock = new MaterialPropertyBlock();
            }

            /// <summary> カラーのShaderプロパティID。 </summary>
            public int ColorPropertyId { get; }

            /// <summary> フェード前の色。 </summary>
            public Color BaseColor { get; }

            /// <summary> Material単位で適用するプロパティ。 </summary>
            public MaterialPropertyBlock PropertyBlock { get; }
        }
    }
}
