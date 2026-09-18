using KillChord.Runtime.Domain.OutGame.Screen;
using System;
using TMPro;
using UnityEngine;

namespace KillChord.Demo
{
    /// <summary>
    ///     体験版のホームタイマーと全体タイマーを表示します。
    /// </summary>
    public sealed class DemoTimerView : MonoBehaviour
    {
        /// <summary>
        ///     表示対象の体験版セッションを設定します。
        /// </summary>
        /// <param name="session"> 表示するセッションです。 </param>
        public void Initialize(IDemoSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            Refresh(false);
        }

        /// <summary>
        ///     左下を基準に、現在画面のUIと操作案内を避けてタイマーを移動します。
        /// </summary>
        /// <param name="screenId"> 現在のOutGame画面IDです。未確定の場合はnullです。 </param>
        /// <param name="isOutGameActive"> OutGame内にいる場合はtrueです。 </param>
        /// <param name="isResultActive"> 戦闘終了演出またはリザルト表示中の場合はtrueです。 </param>
        /// <param name="isScenarioActive"> 選択されたシナリオシーンがロード済みの場合はtrueです。 </param>
        public void UpdatePosition(
            ScreenId? screenId,
            bool isOutGameActive,
            bool isResultActive,
            bool isScenarioActive)
        {
            if (_timerPanel == null)
            {
                return;
            }

            _isSkillBuildActive = !isResultActive && !isScenarioActive && isOutGameActive
                && screenId == ScreenId.SkillBuild;
            float bottom = isResultActive
                ? _resultBottom
                : isScenarioActive
                    ? _scenarioBottom
                    : isOutGameActive
                        ? GetOutGameBottom(screenId)
                        : _battleBottom;
            Vector2 position = _timerPanel.anchoredPosition;
            position.x = _isSkillBuildActive ? _skillBuildLeft : _defaultLeft;
            position.y = bottom;
            _timerPanel.anchoredPosition = position;
        }

        /// <summary> 現在のOutGame画面に設定された下端座標を取得します。 </summary>
        /// <param name="screenId"> 現在画面IDです。 </param>
        /// <returns> タイマーの下端座標です。 </returns>
        private float GetOutGameBottom(ScreenId? screenId)
        {
            return screenId switch
            {
                ScreenId.StageSelect => _stageSelectBottom,
                ScreenId.SkillTree => _skillTreeBottom,
                ScreenId.SkillBuild => _skillBuildBottom,
                ScreenId.Setting => _settingBottom,
                ScreenId.BattlePreparation => _battlePreparationBottom,
                _ => _homeBottom,
            };
        }

        /// <summary>
        ///     現在のタイマー状態を画面へ反映します。
        /// </summary>
        /// <param name="isHomeActive"> ホームにいる場合はtrueです。 </param>
        public void Refresh(bool isHomeActive)
        {
            bool isVisible = _session != null && _session.IsStarted;
            if (_canvas != null)
            {
                _canvas.enabled = isVisible;
            }

            if (!isVisible)
            {
                return;
            }

            if (_timerPanel != null)
            {
                // 改造画面の下部は2段分の高さがないため、両タイマーを同じ文字サイズで横に並べる。
                bool isHorizontal = _isSkillBuildActive && isHomeActive;
                _timerPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, isHorizontal ? 480f : 240f);
                _timerPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, isHomeActive && !isHorizontal ? 116f : 66f);
                SetTextLayout(_overallTimerText, 0f, isHorizontal ? 0.5f : 1f, -8f);
                SetTextLayout(_homeTimerText, isHorizontal ? 0.5f : 0f, 1f, isHorizontal ? -8f : -58f);
            }

            if (_overallTimerText != null)
            {
                _overallTimerText.text =
                    $"全体  {FormatTime(_session.OverallRemainingSeconds)}";
            }

            if (_homeTimerText != null)
            {
                _homeTimerText.gameObject.SetActive(isHomeActive);
                _homeTimerText.text =
                    $"ホーム  {FormatTime(_session.HomeRemainingSeconds)}";
            }
        }

        /// <summary> タイマーを左右の列または元の上下2段へ配置します。 </summary>
        /// <param name="text"> 配置するテキストです。 </param>
        /// <param name="leftAnchor"> 左側のアンカーです。 </param>
        /// <param name="rightAnchor"> 右側のアンカーです。 </param>
        /// <param name="topOffset"> 上端からの位置です。 </param>
        private static void SetTextLayout(TMP_Text text, float leftAnchor, float rightAnchor, float topOffset)
        {
            if (text == null)
            {
                return;
            }

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(leftAnchor, 1f);
            rect.anchorMax = new Vector2(rightAnchor, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, topOffset);
            rect.sizeDelta = new Vector2(-32f, 50f);
        }

        private static string FormatTime(float remainingSeconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }

        [SerializeField, Tooltip("タイマー表示用Canvasです。")]
        private Canvas _canvas;

        [SerializeField, Tooltip("タイマー表示の背景パネルです。")]
        private RectTransform _timerPanel;

        [SerializeField, Tooltip("全体タイマーの残り時間表示です。")]
        private TMP_Text _overallTimerText;

        [SerializeField, Tooltip("ホームタイマーの残り時間表示です。")]
        private TMP_Text _homeTimerText;

        [SerializeField, Tooltip("ホーム画面でのタイマー下端座標です。")]
        private float _homeBottom = 80f;

        [SerializeField, Tooltip("作戦画面でのタイマー下端座標です。")]
        private float _stageSelectBottom = 80f;

        [SerializeField, Tooltip("研究画面でのタイマー下端座標です。")]
        private float _skillTreeBottom = 448f;

        [SerializeField, Tooltip("改造画面でのタイマー下端座標です。")]
        private float _skillBuildBottom = 8f;

        [SerializeField, Tooltip("改造画面以外のタイマー左端座標です。")]
        private float _defaultLeft = 24f;

        [SerializeField, Tooltip("改造画面の操作案内を避けるタイマー左端座標です。")]
        private float _skillBuildLeft = 320f;

        [SerializeField, Tooltip("設定画面でのタイマー下端座標です。")]
        private float _settingBottom = 160f;

        [SerializeField, Tooltip("戦闘準備画面でのタイマー下端座標です。")]
        private float _battlePreparationBottom = 80f;

        [SerializeField, Tooltip("戦闘中のタイマー下端座標です。")]
        private float _battleBottom = 24f;

        [SerializeField, Tooltip("リザルト画面でのタイマー下端座標です。")]
        private float _resultBottom = 400f;

        [SerializeField, Tooltip("シナリオ画面でのタイマー下端座標です。")]
        private float _scenarioBottom = 400f;

        private IDemoSession _session;
        private bool _isSkillBuildActive;
    }
}
