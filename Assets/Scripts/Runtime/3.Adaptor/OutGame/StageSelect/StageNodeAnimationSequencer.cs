using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     複数の StageNodePresenter をまたいだアニメーション処理を
    ///     先入れ先出し順（FIFO）で直列実行するシーケンサー。
    /// </summary>
    public sealed class StageNodeAnimationSequencer
    {
        /// <summary>
        ///    StageNodeAnimationSequencer を初期化します。
        /// </summary>
        public StageNodeAnimationSequencer()
        {
            _tail = Task.CompletedTask;
        }

        /// <summary>
        ///     処理をキューの末尾に追加します。
        ///     前の処理が完了してから <paramref name="action"/> が実行されます。
        /// </summary>
        /// <param name="action"> 実行する非同期処理。</param>
        /// <param name="token"> キャンセルトークン。</param>
        /// <returns> この処理が完了したタスク。</returns>
        public Task Enqueue(Func<CancellationToken, Task> action, CancellationToken token)
        {
            // 前のタスクを捕捉してからチェーンする
            // lock でチェーンの登録順序を保証する（FIFO）
            lock (_lock)
            {
                var next = ChainAsync(_tail, action, token);
                _tail = next;
                return next;
            }
        }

        /// <summary>
        ///     前のタスク完了後に <paramref name="action"/> を実行するタスクを返します。
        /// </summary>
        private static async Task ChainAsync(
            Task previous,
            Func<CancellationToken, Task> action,
            CancellationToken token)
        {
            try
            {
                await previous;
            }
            catch (Exception) { /* 前のタスクの例外は伝播させない */ }

            if (token.IsCancellationRequested) { return; }

            await action(token);
        }

        private readonly object _lock = new object();
        private Task _tail;
    }
}