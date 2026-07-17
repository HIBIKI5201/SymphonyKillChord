using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.Persistent.Load
{
    /// <summary>
    ///     ロード画面を伴う非同期処理を実行するインターフェイス。
    /// </summary>
    public interface ILoadingOperationExecutor
    {
        /// <summary>
        ///     アクティブなロードセッションが存在する場合はtrue。
        /// </summary>
        bool IsSessionActive { get; }

        /// <summary>
        ///     通常のロード画面付き処理として実行する。
        /// </summary>
        /// <param name="operation"> 進捗通知先を受け取る非同期処理。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 処理が成功した場合はTrue。 </returns>
        Task<bool> ExecuteAsync(
            Func<IProgress<float>, Task<bool>> operation,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     指定された設定でロード画面付き処理を実行する。
        /// </summary>
        /// <param name="operation"> 進捗通知先を受け取る非同期処理。 </param>
        /// <param name="options"> ロード実行設定。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 処理が成功した場合はtrue。 </returns>
        Task<bool> ExecuteAsync(
            Func<IProgress<float>, Task<bool>> operation,
            LoadingExecutionOptions options,
            CancellationToken cancellationToken = default);
    }
}
