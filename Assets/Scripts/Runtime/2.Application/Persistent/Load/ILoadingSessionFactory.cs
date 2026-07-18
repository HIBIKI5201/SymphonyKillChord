namespace KillChord.Runtime.Application.Persistent.Load
{
    /// <summary>
    ///   ロード画面を使用するロードセッションを開始するためのインターフェイス。
    /// </summary>
    public interface ILoadingSessionFactory
    {
        /// <summary>
        ///     アクティブなロードセッションが存在する場合はtrue。
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        ///    ロードセッションを開始する。
        /// </summary>
        /// <param name="reuseActiveSession"> 既存のアクティブなセッションを再利用するかどうか。 </param>
        /// <returns> 開始または引き継いだロードセッション。 </returns>
        ILoadingSession Begin(bool reuseActiveSession = false);
    }
}
