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
            if (stageId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stageId), stageId, "ステージIDに0は使用できません。");
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
            if (stageId == 0)
            {
                return false;
            }

            return FindRecord(stageId) != null;
        }

        /// <summary>
        ///     指定された対応表に従って保存済みステージIDを置換し、重複レコードを統合する。
        /// </summary>
        /// <param name="idMap"> 置換前IDと置換後IDの対応表。 </param>
        /// <returns> 保存内容を変更した場合はtrue。 </returns>
        public bool RemapStageIds(IReadOnlyDictionary<int, int> idMap)
        {
            if (idMap == null)
            {
                throw new ArgumentNullException(nameof(idMap));
            }

            if (_clearDatas == null || _clearDatas.Count == 0)
            {
                return false;
            }

            List<StageClearData> migratedRecords = new List<StageClearData>(_clearDatas.Count);
            Dictionary<int, StageClearData> migratedRecordMap = new Dictionary<int, StageClearData>();
            bool isChanged = false;

            // 各記録のステージ ID を新しい ID に置き換える。対応が無い ID はそのまま使う。
            for (int i = 0; i < _clearDatas.Count; i++)
            {
                StageClearData sourceRecord = _clearDatas[i];
                if (sourceRecord == null)
                {
                    isChanged = true;
                    continue;
                }

                int sourceStageId = sourceRecord.StageId;
                int migratedStageId = idMap.TryGetValue(sourceStageId, out int replacementId)
                    ? replacementId
                    : sourceStageId;
                isChanged |= migratedStageId != sourceStageId;

                // ID が 0 になった記録は捨てる。
                if (migratedStageId == 0)
                {
                    isChanged = true;
                    continue;
                }

                // 置き換え後に同じ ID になった記録は1つにまとめる。
                if (migratedRecordMap.TryGetValue(migratedStageId, out StageClearData existingRecord))
                {
                    existingRecord.Merge(sourceRecord);
                    isChanged = true;
                    continue;
                }

                StageClearData migratedRecord = new StageClearData(migratedStageId);
                migratedRecord.Merge(sourceRecord);
                migratedRecordMap.Add(migratedStageId, migratedRecord);
                migratedRecords.Add(migratedRecord);
            }

            // 変更があった場合だけ記録を差し替える。
            if (isChanged)
            {
                _clearDatas = migratedRecords;
            }

            return isChanged;
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

        [SerializeField, Tooltip("ステージごとのクリアデータ。")] private List<StageClearData> _clearDatas = new();
    }
}
