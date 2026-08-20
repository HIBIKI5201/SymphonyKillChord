using KillChord.Runtime.Domain.OutGame.Screen;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     画面遷移結果を表します。
    /// </summary>
    public readonly struct ScreenTransitionResult
    {
        /// <summary>
        ///     画面遷移結果を初期化します。
        /// </summary>
        public ScreenTransitionResult(
            ScreenId? screenToHideId,
            ScreenId screenToShowId,
            bool clearHistory)
        {
            ScreenToHideId = screenToHideId;
            ScreenToShowId = screenToShowId;
            ClearHistory = clearHistory;
        }

        /// <summary> 非表示対象画面 ID を取得します。 </summary>
        public ScreenId? ScreenToHideId { get; }

        /// <summary> 表示対象画面 ID を取得します。 </summary>
        public ScreenId ScreenToShowId { get; }

        /// <summary> 履歴クリアを伴うかどうかを取得します。 </summary>
        public bool ClearHistory { get; }
    }
}
