using KillChord.Runtime.Domain.OutGame.Screen;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     View へ渡す画面遷移 DTO。
    /// </summary>
    public readonly struct ScreenViewDTO
    {
        /// <summary>
        ///     DTO を初期化します。
        /// </summary>
        public ScreenViewDTO(ScreenId? screenToHideId, ScreenId screenToShowId)
        {
            ScreenToHideId = screenToHideId;
            ScreenToShowId = screenToShowId;
        }

        /// <summary> 非表示対象画面 ID を取得します。 </summary>
        public ScreenId? ScreenToHideId { get; }

        /// <summary> 表示対象画面 ID を取得します。 </summary>
        public ScreenId ScreenToShowId { get; }
    }
}
