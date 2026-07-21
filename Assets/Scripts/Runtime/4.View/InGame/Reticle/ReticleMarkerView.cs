using UnityEngine;

namespace KillChord.Runtime.View.InGame.Reticle
{
    /// <summary>
    ///     1体分のレティクルマーカーを表す View コンポーネント。
    ///     マーカー用 Prefab のルートにアタッチして使用する。
    ///     スクリーン座標の反映と表示/非表示の切り替えのみを担い、ロジックは持たない。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReticleMarkerView : MonoBehaviour
    {
        /// <summary>
        ///     マーカーのスクリーン座標を反映する。
        ///     Screen Space - Overlay Canvas では transform.position にスクリーン座標を直接設定する。
        /// </summary>
        /// <param name="screenPosition"> 反映するスクリーン座標。 </param>
        public void SetPosition(Vector2 screenPosition)
        {
            transform.position = screenPosition;
        }

        /// <summary>
        ///     マーカーを表示する。
        /// </summary>
        public void Show()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        ///     マーカーを非表示にしてプールへ退避できる状態にする。
        /// </summary>
        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
