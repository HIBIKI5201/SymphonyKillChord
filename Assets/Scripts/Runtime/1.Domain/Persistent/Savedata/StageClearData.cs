using System;
using System.Collections.Generic;

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

        public int StageId => _stageId;

        public IReadOnlyList<string> AchievedEvaluationIds => _achievedEvaluationIds;

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
        ///     デシリアライズ後のデータを正規化する。
        /// </summary>
        internal void NormalizeAfterDeserialize()
        {
            _achievedEvaluationIds ??= new List<string>();

            HashSet<string> uniqueIds = new(StringComparer.Ordinal);

            for (int i = 0; i < _achievedEvaluationIds.Count; i++)
            {
                string evaluationId = _achievedEvaluationIds[i]?.Trim();

                if (string.IsNullOrWhiteSpace(evaluationId)
                    || !uniqueIds.Add(evaluationId))
                {
                    _achievedEvaluationIds.RemoveAt(i);
                    i--;
                    continue;
                }

                _achievedEvaluationIds[i] =
                    evaluationId;
            }
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

        private int _stageId;
        private List<string> _achievedEvaluationIds;
    }
}
