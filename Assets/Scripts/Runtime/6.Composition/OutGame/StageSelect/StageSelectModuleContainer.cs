using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;

namespace KillChord.Runtime.Composition.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ選択モジュールのうち、外部機能との結合に必要な公開物を保持します。
    /// </summary>
    public sealed class StageSelectModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="stageTree"> 現在のデータ種別から構築されたステージツリーです。 </param>
        /// <param name="selectionService"> バトル出撃対象を設定するサービスです。 </param>
        /// <param name="returnSceneName"> 戦闘終了後の帰還先シーン名です。 </param>
        /// <param name="forceBattleSortie"> 作戦画面で強制出撃の選択と操作制限を適用する処理です。 </param>
        public StageSelectModuleContainer(
            StageTree stageTree,
            BattleSortieSelectionService selectionService,
            string returnSceneName,
            Func<StageId, bool> forceBattleSortie)
        {
            StageTree = stageTree ?? throw new ArgumentNullException(nameof(stageTree));
            SelectionService = selectionService
                ?? throw new ArgumentNullException(nameof(selectionService));
            ReturnSceneName = string.IsNullOrWhiteSpace(returnSceneName)
                ? throw new ArgumentException("帰還先シーン名が未設定です。", nameof(returnSceneName))
                : returnSceneName;
            _forceBattleSortie = forceBattleSortie
                ?? throw new ArgumentNullException(nameof(forceBattleSortie));
        }

        /// <summary> 現在有効なステージツリーです。 </summary>
        public StageTree StageTree { get; }

        /// <summary> バトル出撃対象を設定するサービスです。 </summary>
        public BattleSortieSelectionService SelectionService { get; }

        /// <summary> 戦闘終了後の帰還先シーン名です。 </summary>
        public string ReturnSceneName { get; }

        /// <summary>
        ///     指定ステージを作戦画面で選択し、出撃以外の操作を制限します。
        /// </summary>
        /// <param name="stageId"> 強制選択するバトルステージです。 </param>
        /// <returns> 強制出撃の要求を受け付けた場合はtrueです。 </returns>
        public bool TryForceBattleSortie(StageId stageId)
        {
            return _forceBattleSortie(stageId);
        }

        private readonly Func<StageId, bool> _forceBattleSortie;
    }
}
