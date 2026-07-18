using KillChord.Runtime.Domain.OutGame.StageSelect;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     後続実行待ちの自動遷移チェーンを保持するクラス。
    /// </summary>
    public sealed class PendingNodeTransitionState
    {
        /// <summary> 予約済み遷移が存在する場合はtrue。 </summary>
        public bool HasPending => _pendingNodeTransitions.Count > 0;

        /// <summary>
        ///     後続処理を末尾へ予約する。
        /// </summary>
        /// <param name="pendingNodeTransition"> 予約する遷移情報。</param>
        public void Reserve(PendingNodeTransition pendingNodeTransition)
        {
            if (pendingNodeTransition == null)
            {
                throw new System.ArgumentNullException(nameof(pendingNodeTransition));
            }

            _pendingNodeTransitions.Enqueue(pendingNodeTransition);
        }

        /// <summary>
        ///     指定ステージが今回のプレイで完了したことを記録する。
        /// </summary>
        /// <param name="stageId"> 完了したステージID。</param>
        public void MarkCompleted(StageId stageId)
        {
            if (stageId.Value == 0)
            {
                return;
            }

            _completedStageIds.Add(stageId);
        }

        /// <summary>
        ///     接続元が完了済みの先頭遷移を一度だけ取り出す。
        /// </summary>
        /// <param name="pendingNodeTransition"> 取得した遷移情報。</param>
        /// <returns> 取得できた場合はtrue。</returns>
        public bool TryConsumeCompleted(out PendingNodeTransition pendingNodeTransition)
        {
            pendingNodeTransition = null;
            if (_pendingNodeTransitions.Count == 0)
            {
                return false;
            }

            PendingNodeTransition candidate = _pendingNodeTransitions.Peek();
            if (!_completedStageIds.Remove(candidate.TriggerStageId))
            {
                return false;
            }

            pendingNodeTransition = _pendingNodeTransitions.Dequeue();
            return true;
        }

        /// <summary>
        ///     予約状態と完了状態を破棄する。
        /// </summary>
        public void Clear()
        {
            _pendingNodeTransitions.Clear();
            _completedStageIds.Clear();
        }

        private readonly Queue<PendingNodeTransition> _pendingNodeTransitions = new();
        private readonly HashSet<StageId> _completedStageIds = new();
    }
}
