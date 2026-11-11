namespace KillChord.Runtime.Adaptor.Persistent.Session
{
    /// <summary>
    ///     前回のプレイの途中から再開する先（途中復帰先）を提供する。
    ///     <para>
    ///         起動時はタイトル画面から始め、タイトル画面でゲームを始めたときに、
    ///         ホーム画面以外（シナリオパート・バトルパート）にいた場合だけその再開先へ進むために使う。
    ///     </para>
    /// </summary>
    public interface IPlaySessionResumeProvider
    {
        /// <summary>
        ///     途中復帰先のシーンを取得します。
        ///     <para> 取得できた場合、再開に必要なシーン間の選択状態は用意済みです。 </para>
        /// </summary>
        /// <param name="sceneName"> 途中復帰先のシーン名です。 </param>
        /// <param name="isKeepLoading"> 遷移先の初期化がロード画面を引き継いで閉じる場合はtrueです。 </param>
        /// <returns> 途中復帰先がある場合はtrueです。 </returns>
        bool TryGetResumeDestination(out string sceneName, out bool isKeepLoading);

        /// <summary>
        ///     途中復帰先と前回のプレイの記録を破棄します。
        ///     <para> セーブデータを消したときなど、前回の続きから始めてはいけない場合に使います。 </para>
        /// </summary>
        void DiscardResume();
    }
}
