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