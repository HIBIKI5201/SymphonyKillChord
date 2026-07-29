using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションのプレビュー表示テキストを解決するインターフェース。
    /// </summary>
    public interface IMissionPreviewProvider
    {
        /// <summary>
        ///     指定ミッションIDのプレビュー表示テキストを取得しようとします。
        /// </summary>
        /// <param name="missionId"> 対象のミッションIDです。 </param>
        /// <param name="mainMissionText"> ミッションHUDに表示される説明文です。 </param>
        /// <param name="evaluationDescriptions"> 評価条件の説明文一覧です。 </param>
        /// <returns> IDに対応するミッション定義が存在する場合はtrueです。 </returns>
        bool TryGetPreview(
            MissionId missionId,
            out string mainMissionText,
            out IReadOnlyList<string> evaluationDescriptions);
    }
}
