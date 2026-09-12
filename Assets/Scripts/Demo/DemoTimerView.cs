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
            Refresh();
        }

        /// <summary>
        ///     現在のタイマー状態を画面へ反映します。
        /// </summary>
        public void Refresh()
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

            if (_overallTimerText != null)
            {
                _overallTimerText.text =
                    $"全体  {FormatTime(_session.OverallRemainingSeconds)}";
            }

            if (_homeTimerText != null)
            {
                _homeTimerText.text =
                    $"ホーム  {FormatTime(_session.HomeRemainingSeconds)}";
            }
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

        [SerializeField, Tooltip("全体タイマーの残り時間表示です。")]
        private TMP_Text _overallTimerText;

        [SerializeField, Tooltip("ホームタイマーの残り時間表示です。")]
        private TMP_Text _homeTimerText;

        private IDemoSession _session;
    }
}
