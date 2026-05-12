using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     ミッションHUDに表示する情報をまとめたDTOクラス。
    /// </summary>
    public readonly ref struct MissionHudDTO
    {
        /// <summary>
        ///     MissionHudDTO クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="mainMissionText"> メインミッションの説明テキスト。 </param>
        /// <param name="resultText"> ミッションの結果を示すテキスト。 </param>
        /// <param name="evaluationItems"> ミッション評価項目のコレクション。各項目は読み取り専用で渡されます。 </param>
        public MissionHudDTO(
            string mainMissionText,
            string resultText,
            ReadOnlySpan<MissionEvaluationItemDTO> evaluationItems
            )
        {
            MainMissionText = mainMissionText;
            ResultText = resultText;
            EvaluationItems = evaluationItems;
        }

        /// <summary> メインミッションの説明文。 </summary>
        public string MainMissionText { get; }

        /// <summary> ミッションの結果文。 </summary>
        public string ResultText { get; }

        /// <summary> 評価ミッションの項目リスト。 </summary>
        public ReadOnlySpan<MissionEvaluationItemDTO> EvaluationItems { get; }
    }
}
