using KillChord.Runtime.Domain.OutGame.Screen;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     View へ渡す画面遷移 DTO。
    /// </summary>
    public readonly ref struct ScreenViewDTO
    {
        /// <summary>
        ///     DTO を初期化します。
        /// </summary>
        public ScreenViewDTO(ScreenId? screenToHideId, ScreenId screenToShowId, string targetSceneName = null)
        {
            ScreenToHideId = screenToHideId;
            ScreenToShowId = screenToShowId;
            TargetSceneName = targetSceneName;
        }

        /// <summary> 非表示対象画面 ID を取得します。 </summary>
        public ScreenId? ScreenToHideId { get; }

        /// <summary> 表示対象画面 ID を取得します。 </summary>
        public ScreenId ScreenToShowId { get; }

        /// <summary> 遷移先シーン名を取得します。 </summary>
        public string TargetSceneName { get; }
    }
}
