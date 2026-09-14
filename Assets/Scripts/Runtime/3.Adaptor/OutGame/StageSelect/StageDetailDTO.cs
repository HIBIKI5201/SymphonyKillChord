using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ詳細画面向けの DTO。
    /// </summary>
    public readonly ref struct StageDetailDTO
    {
        /// <summary>
        ///     StageDetailDTO を初期化します。
        /// </summary>
        /// <param name="stageName"> ステージ名。</param>
        /// <param name="flavorText"> フレーバーテキスト。</param>
        /// <param name="firstClearRewards"> 初回クリア時にのみ付与される報酬の一覧。</param>
        /// <param name="clearRewards"> クリアするたびに付与される成功報酬の一覧。</param>
        /// <param name="mainMissionText">
        ///     メインミッションのテキスト。
        ///     シナリオパートの場合は null。
        /// </param>
        public StageDetailDTO(
            string stageName,
            string flavorText,
            IReadOnlyList<StageRewardViewData> firstClearRewards,
            IReadOnlyList<StageRewardViewData> clearRewards,
            string mainMissionText,
            string[] subMissionTexts,
            bool[] subMissionCleared)
        {
            StageName = stageName;
            FlavorText = flavorText;
            FirstClearRewards = firstClearRewards;
            ClearRewards = clearRewards;
            MainMissionText = mainMissionText;
            SubMissionTexts = subMissionTexts;
            SubMissionCleared = subMissionCleared;
        }

        /// <summary> ステージ名。 </summary>
        public string StageName { get; }
        /// <summary> フレーバーテキスト。 </summary>
        public string FlavorText { get; }
        /// <summary> 初回クリア時にのみ付与される報酬の一覧。 </summary>
        public IReadOnlyList<StageRewardViewData> FirstClearRewards { get; }
        /// <summary> クリアするたびに付与される成功報酬の一覧。 </summary>
        public IReadOnlyList<StageRewardViewData> ClearRewards { get; }
        /// <summary>
        ///     メインミッションのテキスト。
        ///     シナリオパートの場合は null。
        /// </summary>
        public string MainMissionText { get; }
        /// <summary>
        ///     サブミッションのテキスト。
        ///     シナリオパートの場合は null。
        /// </summary>
        public string[] SubMissionTexts { get; }
        /// <summary>
        ///     サブミッションごとの達成状況。SubMissionTextsと同じ並び順。
        ///     シナリオパートの場合は null。
        /// </summary>
        public bool[] SubMissionCleared { get; }

        /// <summary> バトルパートかどうか。 </summary>
        public bool IsBattle => MainMissionText != null;
    }
}
