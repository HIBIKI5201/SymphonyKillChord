using KillChord.Runtime.Domain.OutGame.StageSelect;

namespace KillChord.Runtime.Application.OutGame.StageSelect
{
    /// <summary>
    ///     ステージの進行（クリア・解放）に関するユースケースを処理するサービス。
    /// </summary>
    public sealed class StageProgressService
    {
        /// <summary>
        ///     StageProgressService を初期化します。
        /// </summary>
        /// <param name="stageTree"> 操作対象のステージツリー。</param>
        public StageProgressService(StageTree stageTree)
        {
            _stageTree = stageTree ?? throw new System.ArgumentNullException(nameof(stageTree));
        }

        /// <summary>
        ///     指定ステージをクリア済みにし、解放できる後続ノードを解放します。
        ///     バトル→シナリオ、またはシナリオ→バトルの遷移でも、
        ///     前ノードのクリアが確認できた場合のみ後続を解放します。
        /// </summary>
        /// <param name="clearedId"> クリアしたステージのID。</param>
        public void CompleteStage(StageId clearedId)
        {
            if (!_stageTree.TryGetNode(clearedId, out var clearedNode)) { return; }

            // ステージをクリア済みにする
            clearedNode.MarkAsCleared();

            // 後続ノードの解放可否を判定して解放する
            var nextIds = _stageTree.GetNextIds(clearedId);
            for (var i = 0; i < nextIds.Count; i++)
            {
                if (!_stageTree.TryGetNode(nextIds[i], out var nextNode)) { continue; }

                if (_stageTree.CanUnlock(nextIds[i]))
                {
                    nextNode.Unlock();
                }
            }
        }

        private readonly StageTree _stageTree;
    }
}
