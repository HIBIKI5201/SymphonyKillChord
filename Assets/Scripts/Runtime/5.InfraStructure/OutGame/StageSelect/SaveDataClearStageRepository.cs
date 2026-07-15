using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.OutGame.StageSelect
{
    /// <summary>
    ///     セーブデータからクリアしたステージの ID を取得するリポジトリ
    /// </summary>
    public sealed class SaveDataClearStageRepository : IStageClearRepository
    {
        /// <summary>
        ///     読み込み済みのステージ進行データを設定します。
        /// </summary>
        /// <param name="stageProgressData"> ステージ進行データです。 </param>
        public SaveDataClearStageRepository(StageProgressData stageProgressData)
        {
            _stageProgressData = stageProgressData
                ?? throw new ArgumentNullException(nameof(stageProgressData));
        }

        /// <inheritdoc/>
        public IReadOnlyList<StageId> GetClearedStageIds()
        {
            IReadOnlyList<StageClearData> clearData = _stageProgressData.ClearDatas;
            List<StageId> stageIds = new(clearData.Count);

            for (int i = 0; i < clearData.Count; i++)
            {
                StageClearData stageClearData = clearData[i];
                if (stageClearData == null || stageClearData.StageId == 0)
                {
                    continue;
                }

                stageIds.Add(new StageId(stageClearData.StageId));
            }

            return stageIds;
        }

        private readonly StageProgressData _stageProgressData;
    }
}
