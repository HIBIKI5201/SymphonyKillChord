using KillChord.Runtime.Domain.OutGame.StageSelect;
using System.Threading;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ選択画面の操作を処理するコントローラー。
    ///     ノード選択イベントを受け取り、詳細表示へ橋渡しします。
    /// </summary>
    public sealed class StageSelectController
    {
        /// <summary>
        ///     StageSelectController を初期化します。
        /// </summary>
        /// <param name="stageTree"> 参照するステージツリー。</param>
        /// <param name="detailPresenter"> 詳細表示に使用するプレゼンター。</param>
        /// <param name="detailScreenView"> 詳細画面の View。</param>
        public StageSelectController(
            StageTree stageTree,
            StageDetailPresenter detailPresenter,
            IStageDetailScreenShowable detailScreenView)
        {
            _stageTree = stageTree ?? throw new System.ArgumentNullException(nameof(stageTree));
            _detailPresenter = detailPresenter ?? throw new System.ArgumentNullException(nameof(detailPresenter));
            _detailScreenView = detailScreenView ?? throw new System.ArgumentNullException(nameof(detailScreenView));
        }

        /// <summary>
        ///     ステージノードが選択されたときの処理。
        ///     ID に対応するノードのデータを詳細 Presenter へ渡し、詳細画面を表示します。
        /// </summary>
        /// <param name="stageIdValue"> 選択されたステージ ID の整数値。</param>
        /// <param name="token"> キャンセルトークン。</param>
        public void OnStageNodeSelected(int stageIdValue, CancellationToken token)
        {
            var stageId = new StageId(stageIdValue);

            if (!_stageTree.TryGetNode(stageId, out var node))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(StageSelectController)}] StageId '{stageIdValue}' に対応するノードが見つかりませんでした。");
#endif
                return;
            }

            // 現在選択中のノード ID を保持する
            _selectedStageId = stageId;

            _detailPresenter.Push(node);
            _detailScreenView.Show(token);
        }

        /// <summary>
        ///     現在選択中のステージをクリアします。
        ///     選択中のノードがない場合は何もしません。
        /// </summary>
        public bool TryClearSelectedStage(out StageId clearedId)
        {
            clearedId = _selectedStageId;

            if (_selectedStageId.Value == null)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(StageSelectController)}] 選択中のノードがありません。");
#endif
                return false;
            }

            return true;
        }

        private readonly StageTree _stageTree;
        private readonly StageDetailPresenter _detailPresenter;
        private readonly IStageDetailScreenShowable _detailScreenView;
        private StageId _selectedStageId;
    }
}