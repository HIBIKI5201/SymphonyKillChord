using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     ステージ開始演出中にインゲームHUDの表示を切り替えるViewです。
    /// </summary>
    public sealed class InGameHudVisibilityView : MonoBehaviour
    {
        /// <summary> HUDを非表示にします。 </summary>
        public void Hide() => SetVisible(false);

        /// <summary> HUDを表示します。 </summary>
        public void Show() => SetVisible(true);

        [SerializeField, Tooltip("表示を切り替えるインゲームのCanvas。")]
        private Canvas[] _hudCanvases;

        /// <summary>
        ///     必須のCanvas参照が設定されているかを検証します。
        /// </summary>
        private void Awake()
        {
            if (_hudCanvases == null || _hudCanvases.Length == 0)
            {
                Debug.LogWarning($"{nameof(InGameHudVisibilityView)}: 表示を切り替えるCanvasが設定されていません。", this);
            }
        }

        /// <summary>
        ///     設定されたCanvasの表示状態をまとめて切り替えます。
        /// </summary>
        /// <param name="isVisible"> 表示する場合はtrue。 </param>
        private void SetVisible(bool isVisible)
        {
            if (_hudCanvases == null) { return; }

            for (int i = 0; i < _hudCanvases.Length; i++)
            {
                if (_hudCanvases[i] == null) { continue; }

                _hudCanvases[i].enabled = isVisible;
            }
        }
    }
}