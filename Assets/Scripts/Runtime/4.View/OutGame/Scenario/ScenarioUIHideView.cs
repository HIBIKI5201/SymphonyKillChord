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

            _activeStatesBeforeHide = new bool[_hideTargets.Length];
            for (int i = 0; i < _hideTargets.Length; i++)
            {
                GameObject go = _hideTargets[i].gameObject;
                _activeStatesBeforeHide[i] = go.activeSelf;
                go.SetActive(false);
            }

            _isHidden = true;
        }

        /// <summary>
        ///     UIを表示します。
        /// </summary>
        public void ShowUI()
        {
            if (!_isHidden) return;

            for (int i = 0; i < _hideTargets.Length; i++)
            {
                if (_activeStatesBeforeHide != null && i < _activeStatesBeforeHide.Length)
                {
                    _hideTargets[i].gameObject.SetActive(_activeStatesBeforeHide[i]);
                }
            }

            _isHidden = false;
        }

        [SerializeField, Tooltip("非表示にするUIのObject")]
        private RectTransform[] _hideTargets;

        private bool _isHidden = false;
        private bool[] _activeStatesBeforeHide;
    }
}
