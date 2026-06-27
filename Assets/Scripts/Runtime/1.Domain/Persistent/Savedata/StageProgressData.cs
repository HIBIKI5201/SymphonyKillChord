using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     全ステージの進行状況を保持するデータ。
    /// </summary>
    [Serializable]
    public class StageProgressData
    {
        /// <summary> クリア済みステージ一覧。 </summary>
        public IReadOnlyList<StageClearData> ClearDatas => _clearDatas;

        /// <summary>
        ///     クリア結果を記録する。
        /// </summary>
        /// <param name="stageId"> ステージのId。 </param>
        /// <param name="achivedEvaluationIds"> サブミッションのId。 </param>
        /// <returns> 保存内容が変わった場合はtrue。 </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool RecordClear(int stageId, IReadOnlyList<string> achivedEvaluationIds)
        {
            if (stageId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stageId), stageId, "ステージIDは1以上である必要があります。");
            }

            StageClearData record = FindRecord(stageId);
            bool isNewRecord = false;

            if (record == null)
            {
                record = new StageClearData(stageId);

                _clearDatas.Add(record);
                isNewRecord = true;
            }

            bool evaluationChanged = record.AddAchievedEvaluationIds(achivedEvaluationIds);
            return isNewRecord || evaluationChanged;
        }

        /// <summary>
        ///     指定したステージがクリア済みか確認する。
        /// </summary>
        public bool IsStageCleared(int stageId)
        {
            if (stageId <= 0)
            {
                return false;
            }

            return FindRecord(stageId) != null;
        }

        /// <summary>
        ///     指定したステージの保存記録を確認する。
        /// </summary>
        /// <param name="stageId"> ステージのId。 </param>
        /// <returns> クリア済みステージ1件分の保存データ。 </returns>
        private StageClearData FindRecord(int stageId)
        {
            for (int i = 0; i < _clearDatas.Count; i++)
            {
                StageClearData record = _clearDatas[i];

                if (record != null && record.StageId == stageId)
                {
                    return record;
                }
            }

            return null;
        }

        [SerializeField] private List<StageClearData> _clearDatas = new();
    }
}
