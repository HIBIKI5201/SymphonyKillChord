using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.ToonShader
{
    /// <summary>
    /// キャラ専用シャドウマップ(_CharShadowmap)に描き込むRendererを登録するコンポーネント。
    /// キャラクターのルートにアタッチする。
    /// メインライトのシャドウマップへのキャストは通常どおり行う(案B)ため、
    /// Rendererの Cast Shadows 設定は変更しないこと。
    /// </summary>
    [ExecuteAlways]
    public sealed class SilToonShadowCaster : MonoBehaviour
    {
        private static readonly List<SilToonShadowCaster> _instances = new();

        /// <summary>現在有効な全キャスター。SilToonShadowRenderPassが毎フレーム参照する。</summary>
        public static IReadOnlyList<SilToonShadowCaster> Instances => _instances;

        [SerializeField] private Renderer[] _renderers;

        public Renderer[] Renderers => _renderers;

        /// <summary>
        /// 有効なRendererのワールドBoundsの合併を取得する。
        /// </summary>
        public bool TryGetBounds(out Bounds boundsWS)
        {
            boundsWS = default;
            if (_renderers == null) return false;

            bool found = false;
            foreach (Renderer renderer in _renderers)
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;

                if (found)
                {
                    boundsWS.Encapsulate(renderer.bounds);
                }
                else
                {
                    boundsWS = renderer.bounds;
                    found = true;
                }
            }

            return found;
        }

        [ContextMenu("Collect Renderers")]
        public void CollectRenderers()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void OnEnable()
        {
            _instances.Add(this);
        }

        private void OnDisable()
        {
            _instances.Remove(this);
        }

        private void Reset()
        {
            CollectRenderers();
        }
    }
}
