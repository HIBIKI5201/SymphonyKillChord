using System;

namespace KillChord.Runtime.Adaptor.InGame.Result
{
    /// <summary>
    ///    ステージのリザルトを表すDTO。
    /// </summary>
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
            float battleTimeSeconds,
            int maxCombo,
            string rankText,
            string tipsText)
        {
            ResultType = resultType;
            StageNameText = stageNameText ?? string.Empty;
            MainMissionText = mainMissionText ?? string.Empty;
            MainMissionStateText = mainMissionStateText ?? string.Empty;
            SubMissionItems = subMissionItems;
            BattleTimeSeconds = battleTimeSeconds;
            MaxCombo = maxCombo;
            RankText = rankText ?? string.Empty;
            TipsText = tipsText ?? string.Empty;
        }

        /// <summary> リザルトの種類を取得します。 </summary>
        public StageResultType ResultType { get; }

        /// <summary> ステージ名のテキストを取得します。 </summary>
        public string StageNameText { get; }

        /// <summary> メインミッションのテキストを取得します。 </summary>
        public string MainMissionText { get; }

        /// <summary> メインミッションの状態のテキストを取得します。 </summary>
        public string MainMissionStateText { get; }

        /// <summary> サブミッションのアイテムを取得します。 </summary>
        public ReadOnlySpan<StageResultMissionItemDTO> SubMissionItems { get; }

        /// <summary> 戦闘時間（秒）を取得します。 </summary>
        public float BattleTimeSeconds { get; }

        /// <summary> 最大コンボ数を取得します。 </summary>
        public int MaxCombo { get; }

        /// <summary> ランクのテキストを取得します。 </summary>
        public string RankText { get; }

        /// <summary> ヒントのテキストを取得します。 </summary>
        public string TipsText { get; }
    }
}
