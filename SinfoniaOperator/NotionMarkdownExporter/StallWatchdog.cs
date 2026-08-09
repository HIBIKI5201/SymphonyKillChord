using System;
using System.Threading;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     処理が進まなくなったことを検出して警告を出力するクラス。
    /// </summary>
    internal sealed class StallWatchdog : IDisposable
    {
        private const int POLL_INTERVAL_SECONDS = 5;
        private const int STALL_THRESHOLD_SECONDS = 60;
        private const int REPEAT_WARNING_SECONDS = 300;
        private const int SUSPEND_MARGIN_SECONDS = 30;
        private const int DISPOSE_WAIT_SECONDS = 5;

        private readonly Action<string> _warning;
        private readonly CancellationTokenSource _cancellation = new();
        private readonly object _stateLock = new();
        private readonly Task _monitorTask;
        private DateTime _lastProgressUtc;
        private string _currentOperation;
        private bool _isDisposed;

        /// <summary>
        ///     停止監視を開始する。
        /// </summary>
        /// <param name="warning">警告の出力先。</param>
        internal StallWatchdog(Action<string> warning)
        {
            _warning = warning;
            _lastProgressUtc = DateTime.UtcNow;
            _currentOperation = "エクスポートの準備";
            _monitorTask = Task.Run(MonitorAsync);
        }

        /// <summary>
        ///     これから実行する処理を記録し、停止判定の基準時刻を更新する。
        /// </summary>
        /// <param name="operation">これから実行する処理の説明。</param>
        internal void ReportProgress(string operation)
        {
            lock (_stateLock)
            {
                _lastProgressUtc = DateTime.UtcNow;
                _currentOperation = operation;
            }
        }

        /// <summary>
        ///     監視を停止する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) { return; }

            _isDisposed = true;
            _cancellation.Cancel();
            try
            {
                _monitorTask.Wait(TimeSpan.FromSeconds(DISPOSE_WAIT_SECONDS));
            }
            catch (AggregateException)
            {
                // 監視タスク終了時の例外はエクスポート結果に影響しないため無視する。
            }

            _cancellation.Dispose();
        }

        /// <summary>
        ///     一定間隔で進捗を確認し、停止の開始と解消を警告する。
        /// </summary>
        private async Task MonitorAsync()
        {
            TimeSpan pollInterval = TimeSpan.FromSeconds(POLL_INTERVAL_SECONDS);
            TimeSpan stallThreshold = TimeSpan.FromSeconds(STALL_THRESHOLD_SECONDS);
            TimeSpan repeatInterval = TimeSpan.FromSeconds(REPEAT_WARNING_SECONDS);
            TimeSpan suspendThreshold = pollInterval + TimeSpan.FromSeconds(SUSPEND_MARGIN_SECONDS);
            DateTime previousPollUtc = DateTime.UtcNow;
            DateTime? stalledSinceUtc = null;
            DateTime lastWarnedUtc = DateTime.MinValue;

            while (true)
            {
                try
                {
                    await Task.Delay(pollInterval, _cancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                DateTime nowUtc = DateTime.UtcNow;
                TimeSpan pollDelay = nowUtc - previousPollUtc;
                previousPollUtc = nowUtc;

                // 監視タスク自体の待機が伸びている場合は、プロセス全体が中断されていたとみなす。
                if (pollDelay > suspendThreshold)
                {
                    _warning(
                        $"プロセスが{FormatDuration(pollDelay)}中断されていました。" +
                        "PCのスリープや休止状態が原因の可能性があります。");
                    lock (_stateLock) { _lastProgressUtc = nowUtc; }
                    stalledSinceUtc = null;
                    lastWarnedUtc = DateTime.MinValue;
                    continue;
                }

                DateTime lastProgressUtc;
                string currentOperation;
                lock (_stateLock)
                {
                    lastProgressUtc = _lastProgressUtc;
                    currentOperation = _currentOperation;
                }

                TimeSpan idleTime = nowUtc - lastProgressUtc;
                if (idleTime < stallThreshold)
                {
                    if (stalledSinceUtc != null)
                    {
                        _warning($"処理が{FormatDuration(lastProgressUtc - stalledSinceUtc.Value)}停止していましたが再開しました。");
                        stalledSinceUtc = null;
                    }

                    continue;
                }

                // 初回検出時は即座に、それ以降は一定間隔で警告を繰り返す。
                if (stalledSinceUtc == null)
                {
                    stalledSinceUtc = lastProgressUtc;
                }
                else if (nowUtc - lastWarnedUtc < repeatInterval)
                {
                    continue;
                }

                lastWarnedUtc = nowUtc;
                _warning(
                    $"処理が{FormatDuration(idleTime)}進んでいません。（実行中の処理: {currentOperation}）" +
                    "コンソールでテキストを選択している場合はEscキーを押すと再開します。");
            }
        }

        /// <summary>
        ///     経過時間を日本語の表記へ変換する。
        /// </summary>
        /// <param name="duration">経過時間。</param>
        /// <returns>日本語表記の経過時間。</returns>
        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours}時間{duration.Minutes}分{duration.Seconds}秒";
            }

            if (duration.TotalMinutes >= 1)
            {
                return $"{duration.Minutes}分{duration.Seconds}秒";
            }

            return $"{duration.Seconds}秒";
        }
    }
}
