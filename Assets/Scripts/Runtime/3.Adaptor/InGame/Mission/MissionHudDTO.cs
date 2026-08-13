using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     ミッションHUDに表示する情報をまとめたDTO構造体。
    /// </summary>
    public readonly ref struct MissionHudDTO
    {
        /// <summary>
        ///     MissionHudDTO 構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="mainMissionText"> メインミッションの説明テキスト。 </param>
        /// <param name="evaluationItems"> ミッション評価項目のコレクション。 </param>
        public MissionHudDTO(
            string mainMissionText,
            ReadOnlySpan<MissionEvaluationItemDTO> evaluationItems
            )
        {
            MainMissionText = mainMissionText;
            EvaluationItems = evaluationItems;
        }

        /// <summary> メインミッションの説明文を取得します。 </summary>
        public string MainMissionText { get; }

        /// <summary> 評価ミッションの項目リストを取得します。 </summary>
        public ReadOnlySpan<MissionEvaluationItemDTO> EvaluationItems { get; }
    }
}
