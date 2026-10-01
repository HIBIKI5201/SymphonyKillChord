namespace KillChord.Runtime.Composition.Persistent.Session
{
    /// <summary>
    ///     常駐シーンの起動後に最初に開くシーンを決める初期化モジュールのインターフェースです。
    /// </summary>
    public interface IPersistentFirstSceneProvider
    {
        /// <summary>
        ///     最初に開くシーンを取得します。
        /// </summary>
        /// <param name="sceneName"> 最初に開くシーン名です。 </param>
        /// <param name="isKeepLoading">
        ///     読み込んだシーンがロード画面を引き継いで閉じる場合はtrueです。
        /// </param>
        /// <returns> 既定のシーンの代わりに開くシーンがある場合はtrueです。 </returns>
        bool TryGetFirstScene(out string sceneName, out bool isKeepLoading);
    }
}
