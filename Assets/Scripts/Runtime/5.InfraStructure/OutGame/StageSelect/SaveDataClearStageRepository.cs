using KillChord.Runtime.Domain.OutGame.StageSelect;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.OutGame.StageSelect
{
    /// <summary>
    ///     セーブデータからクリアしたステージの ID を取得するリポジトリ
    /// </summary>
    public sealed class SaveDataClearStageRepository : IStageClearRepository
    {
        /// <inheritdoc/>
        public IReadOnlyList<StageId> GetClearedStageIds()
        {
            // TODO: セーブデータからクリアしたステージのIDを取得する処理を実装する
            return new List<StageId>();
        }
    }
}
