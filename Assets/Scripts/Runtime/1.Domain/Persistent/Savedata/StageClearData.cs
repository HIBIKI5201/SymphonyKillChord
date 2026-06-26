using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     クリア済みステージ1件分の保存データ。
    /// </summary>
    [Serializable]
    public class StageClearData
    {
        public StageClearData(int stageId)
        {
            if (stageId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stageId),
                    stageId,
                    "ステージIDは1以上である必要があります。");
            }

            _stageId = stageId;
            _achievedEvaluationIds = new List<string>();
        }

        /// <summary> クリア済みステージID。 </summary>
        public int StageId => _stageId;

        /// <summary> 一度でも達成したサブミッション一覧。 </summary>
        public IReadOnlyList<string> AchievedEvaluationIds => _achievedEvaluationIds;

        /// <summary>
        ///     達成したサブミッションIDを追加する。
        /// </summary>
        /// <param name="evaluationIds"> 達成したサブミッションIDのリスト。 </param>
        /// <returns></returns>
        internal bool AddAchievedEvaluationIds(IReadOnlyList<string> evaluationIds)
        {
            if (evaluationIds == null || evaluationIds.Count == 0)
            {
                return false;
            }

            bool isChanged = false;

            for (int i = 0; i < evaluationIds.Count; i++)
            {
                string evaluationId = evaluationIds[i]?.Trim();

                if (string.IsNullOrWhiteSpace(evaluationId)
                    || _achievedEvaluationIds.Contains(evaluationId))
                {
                    continue;
                }

                _achievedEvaluationIds.Add(evaluationId);
                isChanged = true;
            }

            return isChanged;
        }

        /// <summary>
        ///     同じステージの重複記録を統合する。
        /// </summary>
        internal void Merge(StageClearData other)
        {
            if (other == null)
            {
                return;
            }

            AddAchievedEvaluationIds(other._achievedEvaluationIds);
        }

        [SerializeField] private int _stageId;
        [SerializeField] private List<string> _achievedEvaluationIds;
    }
}
