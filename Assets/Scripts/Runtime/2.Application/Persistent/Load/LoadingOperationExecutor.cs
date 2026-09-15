using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.Persistent.Load
{
    /// <summary>
    ///     ロード画面を伴う非同期処理を実行するクラス。
    /// </summary>
    public class LoadingOperationExecutor : ILoadingOperationExecutor
    {
        /// <summary>
        ///     ロードセッション生成元を指定して生成する。
        /// </summary>
        /// <param name="loadingSessionFactory"> ロードセッションを生成するファクトリー。 </param>
        /// <param name="minimumDisplayTime"> ロード画面の最低限表示する時間。 </param>
        public LoadingOperationExecutor(
            ILoadingSessionFactory loadingSessionFactory
            , float minimumDisplayTime)
        {
            _sessionFactory = loadingSessionFactory
                ?? throw new ArgumentNullException(nameof(loadingSessionFactory));

            if (!float.IsFinite(minimumDisplayTime) || minimumDisplayTime < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDisplayTime),
                    minimumDisplayTime,
                    "最低限表示時間は非負の有限の数でなければなりません。");
            }

            _minimumDisplayTime = minimumDisplayTime;
        }

        /// <summary>
        ///     アクティブなロードセッションが存在する場合はtrue。
        /// </summary>
        public bool IsSessionActive => _sessionFactory.IsLoading;

        public Task<bool> ExecuteAsync(Func<IProgress<float>, Task<bool>> operation, CancellationToken cancellationToken = default)
        {
            return ExecuteAsync(
                operation,
                LoadingExecutionOptions.Default,
                cancellationToken);
        }

        public async Task<bool> ExecuteAsync(Func<IProgress<float>, Task<bool>> operation, LoadingExecutionOptions options, CancellationToken cancellationToken = default)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            // ロードセッションの作成。
            ILoadingSession session = _sessionFactory.Begin(options.ReuseActiveSession);

            if (!options.ReuseActiveSession || _loadingTimeStamp == 0L)
            {
                _loadingTimeStamp = Stopwatch.GetTimestamp();
            }

            // 進捗範囲の変換。
            var progress = new LoadingProgressRange(
                session,
                options.StartProgress,
                options.EndProgress);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 開始地点の通知。
                progress.Report(0f);

                // 実際のロード処理の実行。
                bool success = await operation(progress);

                if (!success)
                {
                    session.Fail();
                    _loadingTimeStamp = 0L;
                    return false;
                }

                progress.Report(1f);

                if (options.CompleteOnSuccess)
                {
                    await WaitForMinimumDisplayTimeAsync(cancellationToken);

                    session.Complete();
                    _loadingTimeStamp = 0L;
                }

                return true;
            }
            catch
            {
                session.Fail();
                _loadingTimeStamp = 0L;
                throw;
            }
        }

        /// <summary>
        ///     ロード画面の最低限表示時間を待機する。
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task WaitForMinimumDisplayTimeAsync(CancellationToken cancellationToken)
        {
            if (_minimumDisplayTime <= 0f || _loadingTimeStamp == 0L)
            {
                return;
            }

            // 経過時間の計算
            // 純粋なC#のStopwatchを使って、ロード開始からの経過時間を計算する。
            double elapsedTime =
                (Stopwatch.GetTimestamp() - _loadingTimeStamp) / (double)Stopwatch.Frequency;

            double remainingTime = _minimumDisplayTime - elapsedTime;

            if (remainingTime <= 0d)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(remainingTime), cancellationToken);
        }

        private readonly ILoadingSessionFactory _sessionFactory;
        private readonly float _minimumDisplayTime;

        private long _loadingTimeStamp;
    }
}
