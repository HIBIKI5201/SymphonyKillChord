using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        /// <param name="waitTimeResolver"> 遷移元と遷移先からロード画面の最低表示時間を決める機能。 </param>
        public LoadingOperationExecutor(
            ILoadingSessionFactory loadingSessionFactory,
            ILoadingWaitTimeResolver waitTimeResolver)
        {
            _sessionFactory = loadingSessionFactory
                ?? throw new ArgumentNullException(nameof(loadingSessionFactory));
            _waitTimeResolver = waitTimeResolver
                ?? throw new ArgumentNullException(nameof(waitTimeResolver));
        }

        /// <summary>
        ///     アクティブなロードセッションが存在する場合はtrue。
        /// </summary>
        public bool IsSessionActive => _sessionFactory.IsLoading;

        /// <summary>
        ///     既定の設定でロード処理を実行する。
        /// </summary>
        public Task<bool> ExecuteAsync(Func<IProgress<float>, Task<bool>> operation, CancellationToken cancellationToken = default)
        {
            return ExecuteAsync(
                operation,
                LoadingExecutionOptions.Default,
                cancellationToken);
        }

        /// <summary>
        ///     設定を指定してロード処理を実行する。
        /// </summary>
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
                _fromSceneName = options.FromSceneName;
                _toSceneName = options.ToSceneName;
            }
            else
            {
                // 引き継いだセッションでは、最初の遷移元を保ったまま遷移先だけを更新する。
                _fromSceneName ??= options.FromSceneName;
                _toSceneName = options.ToSceneName ?? _toSceneName;
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

                // 画面が暗転し切ってからシーンを触り、シーンの無い画面を見せない。
                await session.WaitUntilScreenCoveredAsync(cancellationToken);

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

                    // ロード画面の上を暗転させてから閉じ、遷移先を暗転から表示する。
                    await session.CompleteAsync(cancellationToken);
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
            if (_loadingTimeStamp == 0L)
            {
                return;
            }

            // 遷移の組み合わせごとの設定から最低表示時間を決める。
            float minimumDisplayTime = _waitTimeResolver.ResolveMinimumDisplayTime(
                _fromSceneName,
                _toSceneName);
            if (!float.IsFinite(minimumDisplayTime) || minimumDisplayTime <= 0f)
            {
                return;
            }

            // 経過時間の計算
            // 純粋なC#のStopwatchを使って、ロード開始からの経過時間を計算する。
            double elapsedTime =
                (Stopwatch.GetTimestamp() - _loadingTimeStamp) / (double)Stopwatch.Frequency;

            double remainingTime = minimumDisplayTime - elapsedTime;

            if (remainingTime <= 0d)
            {
                return;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(remainingTime), DelayType.Realtime, cancellationToken: cancellationToken);
        }

        private readonly ILoadingSessionFactory _sessionFactory;
        private readonly ILoadingWaitTimeResolver _waitTimeResolver;

        private long _loadingTimeStamp;
        private string _fromSceneName;
        private string _toSceneName;
    }
}
