using System.Collections.Generic;
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

            _hiddenTargets = BuildValidatedTargets();
            _activeStatesBeforeHide = new bool[_hiddenTargets.Count];
            for (int i = 0; i < _hiddenTargets.Count; i++)
            {
                GameObject go = _hiddenTargets[i].gameObject;
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

            for (int i = 0; i < _hiddenTargets.Count; i++)
            {
                if (_activeStatesBeforeHide != null && i < _activeStatesBeforeHide.Length)
                {
                    RectTransform target = _hiddenTargets[i];
                    if (target != null)
                    {
                        target.gameObject.SetActive(_activeStatesBeforeHide[i]);
                    }
                }
            }

            _isHidden = false;
            _activeStatesBeforeHide = null;
            _hiddenTargets.Clear();
        }

        /// <summary>
        /// 再生開始前に非表示状態を復元する。
        /// </summary>
        public void RestoreForPlayback()
        {
            ShowUI();
        }

        /// <summary>
        /// null、重複、親子二重登録を除外した非表示対象を構築する。
        /// </summary>
        private List<RectTransform> BuildValidatedTargets()
        {
            var targets = new List<RectTransform>(_hideTargets?.Length ?? 0);
            var unique = new HashSet<RectTransform>();
            if (_hideTargets == null)
            {
                return targets;
            }

            for (int i = 0; i < _hideTargets.Length; i++)
            {
                RectTransform target = _hideTargets[i];
                if (target == null)
                {
                    Debug.LogWarning(
                        $"[{nameof(ScenarioUIHideView)}] nullの非表示対象を除外します。Index={i}",
                        this);
                    continue;
                }

                if (!unique.Add(target))
                {
                    Debug.LogWarning(
                        $"[{nameof(ScenarioUIHideView)}] 重複した非表示対象を除外します。Target={target.name}",
                        this);
                    continue;
                }

                targets.Add(target);
            }

            for (int i = targets.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < targets.Count; j++)
                {
                    if (i == j || !targets[i].IsChildOf(targets[j]))
                    {
                        continue;
                    }

                    Debug.LogWarning(
                        $"[{nameof(ScenarioUIHideView)}] 親子が二重登録されているため子を除外します。Target={targets[i].name}",
                        this);
                    targets.RemoveAt(i);
                    break;
                }
            }

            return targets;
        }

        [SerializeField, Tooltip("非表示にするUIのObject")]
        private RectTransform[] _hideTargets;

        private bool _isHidden = false;
        private bool[] _activeStatesBeforeHide;
        private List<RectTransform> _hiddenTargets = new();
    }
}
