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
            _stageTree = stageTree;
            _detailPresenter = detailPresenter;
            _detailScreenView = detailScreenView;
        }

        /// <summary>
        ///     ステージノードが選択されたときの処理。
        ///     ID に対応するノードのデータを詳細 Presenter へ渡し、詳細画面を表示します。
        /// </summary>
        /// <param name="stageIdValue"> 選択されたステージ ID の文字列値。</param>
        /// <param name="token"> キャンセルトークン。</param>
        public void OnStageNodeSelected(string stageIdValue, CancellationToken token)
        {
            var stageId = new StageId(stageIdValue);

            if (!_stageTree.TryGetNode(stageId, out var node))
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(StageSelectController)}] StageId '{stageIdValue}' に対応するノードが見つかりませんでした。");
                return;
            }

            // データを詳細画面に反映してから表示する
            _detailPresenter.Push(node);
            _detailScreenView.Show(token);
        }

        private readonly StageTree _stageTree;
        private readonly StageDetailPresenter _detailPresenter;
        private readonly IStageDetailScreenShowable _detailScreenView;
    }
}