using System.Collections.Generic;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     クリア済みステージ ID の永続化データを読み取るリポジトリインターフェース。
    ///     セーブデータ実装が完成したら InfraStructure 層に実装する。
    /// </summary>
    public interface IStageClearRepository
    {
        /// <summary>
        ///     クリア済みステージ ID のリストを取得する。
        /// </summary>
        /// <returns> クリア済みステージ ID のリスト。 </returns>
        IReadOnlyList<StageId> GetClearedStageIds();
    }
}
