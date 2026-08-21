namespace KillChord.Runtime.View.Persistent.PostEffect
{
    /// <summary>
    ///     Config単位でポストプロセス用Overlayカメラの起動と停止を受け付ける契約。
    ///     同じConfigの多重要求は参照数で数え、すべて取り下げられた時点で停止する。
    /// </summary>
    public interface IPostEffectOverlayPlayer
    {
        /// <summary>
        ///     指定Configのポストプロセスを開始する。
        /// </summary>
        /// <param name="config"> 開始する対象のConfigです。 </param>
        void Add(PostEffectOverlayConfig config);

        /// <summary>
        ///     指定Configのポストプロセスを取り下げる。
        /// </summary>
        /// <param name="config"> 取り下げる対象のConfigです。 </param>
        void Remove(PostEffectOverlayConfig config);

        /// <summary>
        ///     待機したのち、指定秒数だけポストプロセスを開始する。
        ///     <see cref="Add"/> と <see cref="Remove"/> を時間で自動的に呼ぶ簡易版。
        /// </summary>
        /// <param name="config"> 開始する対象のConfigです。 </param>
        /// <param name="delaySeconds"> 開始までの待機秒数です。0以下なら即座に開始します。 </param>
        /// <param name="durationSeconds"> 継続する秒数です。0以下なら何もしません。 </param>
        void AddForSeconds(PostEffectOverlayConfig config, float delaySeconds, float durationSeconds);

        /// <summary>
        ///     指定Configが適用中かどうかを取得する。
        /// </summary>
        /// <param name="config"> 判定する対象のConfigです。 </param>
        /// <returns> 適用中の場合はtrue。 </returns>
        bool IsActive(PostEffectOverlayConfig config);

        /// <summary>
        ///     すべてのポストプロセスを取り下げる。
        /// </summary>
        void RemoveAll();
    }
}
