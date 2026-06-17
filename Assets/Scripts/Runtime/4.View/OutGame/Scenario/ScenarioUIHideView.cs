using UnityEngine;

namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    ///    シナリオUIの表示・非表示を制御するクラス。
    /// </summary>
    public class ScenarioUIHideView : MonoBehaviour
    {
        /// <summary> UIが非表示かどうかを示します。 </summary>
        public bool IsHidden => _isHidden;

        /// <summary>
        ///     UIを非表示にします。
        /// </summary>
        public void HideUI()
        {
            if (_isHidden) return;

            foreach (var target in _hideTargets)
            {
                target.gameObject.SetActive(false);
            }

            _isHidden = true;
        }

        /// <summary>
        ///     UIを表示します。
        /// </summary>
        public void ShowUI()
        {
            if (!_isHidden) return;

            foreach (var target in _hideTargets)
            {
                target.gameObject.SetActive(true);
            }

            _isHidden = false;
        }

        [SerializeField, Tooltip("非表示にするUIのObject")]
        private RectTransform[] _hideTargets;

        private bool _isHidden = false;
    }
}
