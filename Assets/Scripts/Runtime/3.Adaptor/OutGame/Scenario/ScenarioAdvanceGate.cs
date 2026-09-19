using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// テキスト送り待ちを制御する同期ゲート。
    /// </summary>
    public class ScenarioAdvanceGate : ITextAdvanceWaiter
    {
        /// <summary>
        /// 次送り入力が来るまで待機する。
        /// </summary>
        public async ValueTask WaitNextAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var source = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_sync)
            {
                if (_currentWait != null && !_currentWait.Task.IsCompleted)
                {
                    throw new InvalidOperationException("次送り待機は既に開始されています。");
                }
                _currentWait = source;
            }

            // 現在のテキストに対する一入力だけ受け付け、次のテキストへ持ち越さない。
            using CancellationTokenRegistration registration = ct.Register(() => source.TrySetCanceled(ct));
            try
            {
                await source.Task;
                ct.ThrowIfCancellationRequested();
            }
            finally
            {
                lock (_sync)
                {
                    if (ReferenceEquals(_currentWait, source)) { _currentWait = null; }
                }
            }
        }

        /// <summary>
        /// 待機中の次送りを解放する。
        /// </summary>
        public void NotifyNext()
        {
            lock (_sync)
            {
                _currentWait?.TrySetResult(true);
            }
        }

        private TaskCompletionSource<bool> _currentWait;
        private readonly object _sync = new();
    }
}