using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using System.Collections.Generic;
using System.Linq;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージノードのサブミッション達成状況を、セーブデータと突き合わせて解決するクラス。
    /// </summary>
    public sealed class SubMissionAchievementResolver
    {
        /// <summary>
        ///     SubMissionAchievementResolver を初期化します。
        /// </summary>
        /// <param name="missionPreviewProvider"> サブミッションの評価条件ID解決に使うプロバイダー。 </param>
        /// <param name="stageProgressData"> サブミッションの達成状況を確認するためのセーブデータ。 </param>
        public SubMissionAchievementResolver(
            IMissionPreviewProvider missionPreviewProvider,
            StageProgressData stageProgressData)
        {
            _missionPreviewProvider = missionPreviewProvider;
            _stageProgressData = stageProgressData;
        }

        /// <summary>
        ///     指定ステージノードのサブミッション達成状況を解決します。
        /// </summary>
        /// <param name="node"> 対象のステージノード。 </param>
        /// <returns> サブミッションごとの達成状況。サブミッションが存在しない場合はnull。 </returns>
        public bool[] Resolve(StageNode node)
        {
            if (node == null) { return null; }

            BattleStageDefinition battleDefinition = node.Definition as BattleStageDefinition;
            if (battleDefinition == null || _missionPreviewProvider == null) { return null; }

            _missionPreviewProvider.TryGetPreview(
                battleDefinition.MissionId,
                out _,
                out IReadOnlyList<string> evaluationDescriptions,
                out IReadOnlyList<string> evaluationIds);

            int subMissionCount = evaluationDescriptions?.Count ?? 0;
            if (subMissionCount == 0) { return null; }

            IReadOnlyList<string> achievedEvaluationIds = _stageProgressData?.ClearDatas
                .FirstOrDefault(record => record.StageId == node.Id.Value)
                ?.AchievedEvaluationIds;

            var subMissionCleared = new bool[subMissionCount];
            if (achievedEvaluationIds == null || evaluationIds == null)
            {
                return subMissionCleared;
            }

            for (int i = 0; i < subMissionCount && i < evaluationIds.Count; i++)
            {
                subMissionCleared[i] = achievedEvaluationIds.Contains(evaluationIds[i]);
            }

            return subMissionCleared;
        }

        private readonly IMissionPreviewProvider _missionPreviewProvider;
        private readonly StageProgressData _stageProgressData;
    }
}
