using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
        public LoadingOperationExecutor(ILoadingSessionFactory loadingSessionFactory)
        {
            _sessionFactory = loadingSessionFactory
                ?? throw new ArgumentNullException(nameof(loadingSessionFactory));
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
            if(operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            // ロードセッションの作成。
            ILoadingSession session = _sessionFactory.Begin(options.ReuseActiveSession);

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

                if(!success)
                {
                    session.Fail();
                    return false;
                }

                progress.Report(1f);

                if(options.CompleteOnSuccess)
                {
                    session.Complete();
                }

                return true;
            }
            catch
            {
                session.Fail();
                throw;
            }
        }

        private readonly ILoadingSessionFactory _sessionFactory;
    }
}
