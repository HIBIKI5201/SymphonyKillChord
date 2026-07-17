using KillChord.Runtime.Domain.OutGame.StageSelect;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.StageSelect
{
    /// <summary>
    ///     作戦画面を開く際の処理を担うユースケース。
    ///     セーブデータと現在のツリー状態を比較し、
    ///     新規クリア済みステージを検出してツリーへ反映します。
    /// </summary>
    public class StageSelectOpenUseCase
    {
        /// <summary>
        ///     StageSelectOpenUseCase を初期化します。
        /// </summary>
        /// <param name="stageTree"> 操作対象のステージツリー。</param>
        /// <param name="stageProgressService"> ステージ進行状況を管理するサービス。 </param>
        /// <param name="stageClearRepository"> ステージクリア情報を管理するリポジトリ。 </param>
        public StageSelectOpenUseCase(
            StageTree stageTree,
            StageProgressService stageProgressService,
            IStageClearRepository stageClearRepository)
        {
            _stageTree = stageTree;
            _progressService = stageProgressService;
            _clearRepository = stageClearRepository;
        }

        /// <summary>
        ///     セーブデータからクリア済みステージを取得し、ツリーに反映します。
        ///     Presenter 生成前に呼び出すことを想定しています。
        ///     既知のクリア済み状態をアニメーション無しで即時反映する。
        /// </summary>
        public void ApplySavedClearedStages()
        {
            var clearedIds = _clearRepository.GetClearedStageIds();
            for(var  i = 0; i < clearedIds.Count; i++)
            {
                if (!_stageTree.TryGetNode(clearedIds[i], out var node)) { continue; }

                if(node.Status == StageStatus.Cleared) { continue; }

                _progressService.CompleteStage(clearedIds[i]);
            }
        }

        /// <summary>
        ///     セーブデータと現在のツリー状態を比較し、
        ///     新規クリア済みステージを検出して返します。
        /// </summary>
        /// <returns> 新規クリア済みステージの ID リスト。 </returns>
        public IReadOnlyList<StageId> GetNewlyClearedIds()
        {
            var clearedIds = _clearRepository.GetClearedStageIds();
            var newlyClearedIds = new List<StageId>();

            for(var i = 0; i < clearedIds.Count; i++)
            {
                if(!_stageTree.TryGetNode(clearedIds[i], out var node)) { continue; }

                // ツリー上でまだ Cleared になっていない = 新規クリア
                if (node.Status == StageStatus.Cleared) { continue; }

                newlyClearedIds.Add(clearedIds[i]);
                _progressService.CompleteStage(clearedIds[i]);
            }

            return newlyClearedIds;
        }

        private readonly StageTree _stageTree;
        private readonly StageProgressService _progressService;
        private readonly IStageClearRepository _clearRepository;
    }
}
