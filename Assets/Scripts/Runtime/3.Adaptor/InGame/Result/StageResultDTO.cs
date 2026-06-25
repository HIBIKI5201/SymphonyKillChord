using System;

namespace KillChord.Runtime.Adaptor.InGame.Result
{
    public readonly ref struct StageResultDTO
    {
        /// <summary>
        ///     リザルトDTOを初期化します。
        /// </summary>
        public StageResultDTO(
            StageResultType resultType,
            string stageNameText,
            string mainMissionText,
            string mainMissionStateText,
            ReadOnlySpan<StageResultMissionItemDTO> subMissionItems,
            string battleTimeText,
            string maxComboText,
            string rankText,
            string tipsText)
        {
            ResultType = resultType;
            StageNameText = stageNameText ?? string.Empty;
            MainMissionText = mainMissionText ?? string.Empty;
            MainMissionStateText = mainMissionStateText ?? string.Empty;
            SubMissionItems = subMissionItems;
            BattleTimeText = battleTimeText ?? string.Empty;
            MaxComboText = maxComboText ?? string.Empty;
            RankText = rankText ?? string.Empty;
            TipsText = tipsText ?? string.Empty;
        }

        public StageResultType ResultType { get; }

        public string StageNameText { get; }

        public string MainMissionText { get; }

        public string MainMissionStateText { get; }

        public ReadOnlySpan<StageResultMissionItemDTO> SubMissionItems { get; }

        public string BattleTimeText { get; }

        public string MaxComboText { get; }

        public string RankText { get; }

        public string TipsText { get; }
    }
}
