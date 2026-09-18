using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     武器の出現・収納中だけ使用する材質を事前生成し、通常時の材質を保持します。
    /// </summary>
    internal sealed class WeaponDissolveMaterials : IDisposable
    {
        /// <summary>
        ///     指定モデルのメッシュだけを収集し、元材質ごとに演出材質を一度生成します。
        /// </summary>
        /// <param name="model"> 演出対象の武器モデルです。 </param>
        /// <param name="shader"> 明示設定されたディゾルブ用シェーダーです。 </param>
        public WeaponDissolveMaterials(GameObject model, Shader shader)
        {
            List<RendererMaterials> bindings = new();
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is not MeshRenderer && renderer is not SkinnedMeshRenderer)
                {
                    continue;
                }

                Material[] originals = renderer.sharedMaterials;
                Material[] transitions = new Material[originals.Length];
                for (int i = 0; i < originals.Length; i++)
                {
                    Material original = originals[i];
                    if (original == null)
                    {
                        continue;
                    }
                    if (!_generatedMaterials.TryGetValue(original, out Material transition))
                    {
                        transition = CreateTransitionMaterial(original, shader);
                        _generatedMaterials.Add(original, transition);
                    }
                    transitions[i] = transition;
                }
                bindings.Add(new RendererMaterials(renderer, originals, transitions));
            }
            _bindings = bindings.ToArray();
        }

        /// <summary>
        ///     生成済みの演出材質へ切り替えます。
        /// </summary>
        public void Begin()
        {
            if (_isDisposed || _isApplied)
            {
                return;
            }
            foreach (RendererMaterials binding in _bindings)
            {
                if (binding.Renderer != null)
                {
                    binding.Renderer.sharedMaterials = binding.Transitions;
                }
            }
            _isApplied = true;
        }

        /// <summary>
        ///     現在の演出材質へ表示率を適用します。共有の元材質は変更しません。
        /// </summary>
        /// <param name="value"> 非表示0から全表示1までの表示率です。 </param>
        public void SetRatio(float value)
        {
            if (_isDisposed || !_isApplied)
            {
                return;
            }
            foreach (Material material in _generatedMaterials.Values)
            {
                material.SetFloat(RATIO_ID, value);
            }
        }

        /// <summary>
        ///     通常表示に使っていた材質配列をそのまま復元します。
        /// </summary>
        public void Restore()
        {
            if (_isDisposed || !_isApplied)
            {
                return;
            }
            foreach (RendererMaterials binding in _bindings)
            {
                if (binding.Renderer != null)
                {
                    binding.Renderer.sharedMaterials = binding.Originals;
                }
            }
            _isApplied = false;
        }

        /// <summary>
        ///     元材質を復元し、この武器用に生成した材質だけを破棄します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            Restore();
            foreach (Material material in _generatedMaterials.Values)
            {
                UnityEngine.Object.Destroy(material);
            }
            _generatedMaterials.Clear();
            _isDisposed = true;
        }

        private static readonly int RATIO_ID = Shader.PropertyToID("_Ratio");
        private readonly Dictionary<Material, Material> _generatedMaterials = new();
        private readonly RendererMaterials[] _bindings;
        private bool _isApplied;
        private bool _isDisposed;

        /// <summary>
        ///     通常材質の色とテクスチャを引き継いだ、演出専用の実体を生成します。
        /// </summary>
        /// <param name="original"> 通常時の材質です。 </param>
        /// <param name="shader"> 演出専用シェーダーです。 </param>
        /// <returns> 所有者が破棄する演出専用材質です。 </returns>
        private static Material CreateTransitionMaterial(Material original, Shader shader)
        {
            Material material = new(shader)
            {
                name = original.name + " (Weapon Dissolve)",
                hideFlags = HideFlags.DontSave
            };
            CopyTexture(original, material, original.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex", "_Base");
            CopyTexture(original, material, "_BumpMap", "_Normal");
            Color color = original.HasProperty("_BaseColor") ? original.GetColor("_BaseColor")
                : original.HasProperty("_Color") ? original.GetColor("_Color") : Color.white;
            material.SetColor("_Color", color);
            material.SetColor("_EffectColor", color);
            material.SetFloat("_Flash", 0f);
            material.SetFloat(RATIO_ID, 1f);
            if (original.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", original.GetFloat("_Smoothness"));
            }
            else if (original.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Smoothness", original.GetFloat("_Glossiness"));
            }
            if (original.HasProperty("_Metallic"))
            {
                material.SetFloat("_Mettalic", original.GetFloat("_Metallic"));
            }
            // Metallic/Emission/Parallaxのマップ表現はこのShaderに無いため、通常表示は必ず元材質へ戻す。
            return material;
        }

        /// <summary>
        ///     対応するテクスチャと拡縮・オフセットを演出材質へコピーします。
        /// </summary>
        /// <param name="original"> コピー元の材質です。 </param>
        /// <param name="transition"> コピー先の演出材質です。 </param>
        /// <param name="sourceProperty"> 元シェーダーのプロパティ名です。 </param>
        /// <param name="destinationProperty"> 演出シェーダーのプロパティ名です。 </param>
        private static void CopyTexture(Material original, Material transition, string sourceProperty, string destinationProperty)
        {
            if (!original.HasProperty(sourceProperty))
            {
                return;
            }
            transition.SetTexture(destinationProperty, original.GetTexture(sourceProperty));
            transition.SetTextureScale(destinationProperty, original.GetTextureScale(sourceProperty));
            transition.SetTextureOffset(destinationProperty, original.GetTextureOffset(sourceProperty));
        }

        /// <summary>
        ///     Rendererと通常・演出の材質スロット対応を保持します。
        /// </summary>
        private readonly struct RendererMaterials
        {
            /// <summary>
            ///     一つのRendererで使用する材質配列を記録します。
            /// </summary>
            public RendererMaterials(Renderer renderer, Material[] originals, Material[] transitions)
            {
                Renderer = renderer;
                Originals = originals;
                Transitions = transitions;
            }

            /// <summary> 材質を切り替えるRendererです。 </summary>
            public Renderer Renderer { get; }
            /// <summary> 通常時の材質配列です。 </summary>
            public Material[] Originals { get; }
            /// <summary> 演出時の材質配列です。 </summary>
            public Material[] Transitions { get; }
        }
    }
}
