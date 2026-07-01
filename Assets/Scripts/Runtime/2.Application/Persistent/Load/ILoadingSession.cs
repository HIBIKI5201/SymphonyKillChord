using System;

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
        ///     ロード処理を失敗として処理する。
        /// </summary>
        void Fail();
    }
}
