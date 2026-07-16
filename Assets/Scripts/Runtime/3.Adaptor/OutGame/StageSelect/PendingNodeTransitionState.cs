namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ノード再生後に実行する後続処理の予約状態を保持します。
    /// </summary>
    public sealed class PendingNodeTransitionState
    {
        /// <summary> 予約済み遷移が存在する場合はtrueです。 </summary>
        public bool HasPending => _pendingNodeTransition != null;

        /// <summary>
        ///     後続処理を予約します。
        /// </summary>
        /// <param name="pendingNodeTransition"> 予約する遷移情報です。 </param>
        public void Reserve(PendingNodeTransition pendingNodeTransition)
        {
            _pendingNodeTransition = pendingNodeTransition;
        }

        /// <summary>
        ///     予約済み遷移を一度だけ取り出します。
        /// </summary>
        /// <param name="pendingNodeTransition"> 取得した遷移情報です。 </param>
        /// <returns> 取得できた場合はtrueです。 </returns>
        public bool TryConsume(out PendingNodeTransition pendingNodeTransition)
        {
            pendingNodeTransition = _pendingNodeTransition;
            if (pendingNodeTransition == null)
            {
                return false;
            }

            _pendingNodeTransition = null;
            return true;
        }

        /// <summary>
        ///     予約状態を破棄します。
        /// </summary>
        public void Clear()
        {
            _pendingNodeTransition = null;
        }

        private PendingNodeTransition _pendingNodeTransition;
    }
}
