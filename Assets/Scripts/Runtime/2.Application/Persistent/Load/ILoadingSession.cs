using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.Persistent.Load
{
    /// <summary>
    ///     1回のロード表示期間における進捗と終了状態を管理するインターフェイス。
    /// </summary>
    public interface ILoadingSession : IProgress<float>
    {
        /// <summary>
        ///     ロード処理が終了済みかどうか。
        /// </summary>
        bool IsEnded { get; }

        /// <summary>
        ///     ロード処理を正常終了する。
        /// </summary>
        void Complete();

        /// <summary>
        ///     画面の暗転を待ってからロード処理を正常終了する。
        ///     ロード画面の上を暗転させてから閉じ、遷移先の画面を暗転から表示させるために使う。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 終了までを表すタスク。 </returns>
        Task CompleteAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     ロード開始時の暗転が終わり、画面が覆われるまで待つ。
        ///     暗転の前にシーンを破棄すると、シーンの無い画面が見えてしまうため。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 画面が覆われるまでを表すタスク。 </returns>
        Task WaitUntilScreenCoveredAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     ロード処理を失敗として処理する。
        /// </summary>
        void Fail();
    }
}
