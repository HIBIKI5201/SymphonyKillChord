using System;

namespace KillChord.Runtime.Application.Persistent.Load
{
    /// <summary>
    ///     ロード画面付き処理の実行条件をまとめた不変の設定値。
    ///     ユースケースの実行条件をまとめた値なのでApplicationにおいてます。
    /// </summary>
    public readonly struct LoadingExecutionOptions : IEquatable<LoadingExecutionOptions>
    {
        /// <summary>
        ///     ロード実行設定を生成する。
        /// </summary>
        /// <param name="startProgress"> 全体進捗上の開始位置。 </param>
        /// <param name="endProgress"> 全体進捗上の終了位置。 </param>
        /// <param name="reuseActiveSession"> 実行中のロードセッションを再利用するかどうか。 </param>
        /// <param name="completeOnSuccess"> 処理成功時にロードセッションを終了するかどうか。 </param>
        public LoadingExecutionOptions(
            float startProgress,
            float endProgress,
            bool reuseActiveSession,
            bool completeOnSuccess)
        {
            // 例外処理。
            if (startProgress < 0f || startProgress > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startProgress),
                    startProgress,
                    "開始進捗は0から1の範囲で指定してください。");
            }

            if (endProgress < 0f || endProgress > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(endProgress),
                    endProgress,
                    "終了進捗は0から1の範囲で指定してください。");
            }

            if (startProgress > endProgress)
            {
                throw new ArgumentException(
                    "開始進捗は終了進捗以下である必要があります。");
            }

            StartProgress = startProgress;
            EndProgress = endProgress;
            ReuseActiveSession = reuseActiveSession;
            CompleteOnSuccess = completeOnSuccess;
        }

        /// <summary>
        ///     全体進捗上の開始位置。
        /// </summary>
        public float StartProgress { get; }

        /// <summary>
        ///     全体進捗上の終了位置。
        /// </summary>
        public float EndProgress { get; }

        /// <summary>
        ///     実行中のロードセッションを引き継ぐかどうか。
        /// </summary>
        public bool ReuseActiveSession { get; }

        /// <summary>
        ///     処理成功時にロード画面を閉じるかどうか。
        /// </summary>
        public bool CompleteOnSuccess { get; }

        /// <summary>
        ///     単独で完結するロード設定を生成する。
        /// </summary>
        public static LoadingExecutionOptions Default =>
            new LoadingExecutionOptions(
                0f,
                1f,
                false,
                true);

        /// <summary>
        ///     ロード画面を閉じずに、皇族処理へ引き継ぎ設定を生成する。
        /// </summary>
        /// <param name="startProgress"> 開始進捗。 </param>
        /// <param name="endprogress"> 終了進捗。 </param>
        /// <returns> ロード実行設定。 </returns>
        public static LoadingExecutionOptions KeepOpen(
            float startProgress,
            float endprogress)
        {
            return new LoadingExecutionOptions(
                startProgress,
                endprogress,
                false,
                false);
        }

        /// <summary>
        ///     実行中のロード画面を引き継ぎ、成功時に閉じる設定を生成する。
        /// </summary>
        /// <param name="startProgress"> 開始進捗。 </param>
        /// <param name="endProgress"> 終了進捗。 </param>
        /// <returns> ロード実行設定。 </returns>
        public static LoadingExecutionOptions ContinueAndComplete(
            float startProgress,
            float endProgress)
        {
            return new LoadingExecutionOptions(
                startProgress,
                endProgress,
                true,
                true);
        }

        /// <summary>
        ///     他の値と等しいか判定する。
        /// </summary>
        /// <param name="other"> 比較対象。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public bool Equals(LoadingExecutionOptions other)
        {
            return StartProgress.Equals(other.StartProgress)
                && EndProgress.Equals(other.EndProgress)
                && ReuseActiveSession == other.ReuseActiveSession
                && CompleteOnSuccess == other.CompleteOnSuccess;
        }

        /// <summary>
        ///     他のオブジェクトと等しいか判定する。
        /// </summary>
        /// <param name="obj"> 比較対象。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public override bool Equals(object obj)
        {
            return obj is LoadingExecutionOptions other
                && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得する。
        /// </summary>
        /// <returns> ハッシュコード。 </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                StartProgress,
                EndProgress,
                ReuseActiveSession,
                CompleteOnSuccess);
        }
    }
}
