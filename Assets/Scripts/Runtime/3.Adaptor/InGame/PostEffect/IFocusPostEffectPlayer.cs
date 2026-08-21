namespace KillChord.Runtime.Adaptor.InGame.PostEffect
{
    /// <summary>
    ///     注目対象を除いた画面へ、一定時間だけ全画面Volumeを掛ける再生契約。
    ///     どのレイヤーを注目対象とするかは実装側の描画設定が持つ。
    /// </summary>
    public interface IFocusPostEffectPlayer
    {
        /// <summary> Volumeを適用中かどうかです。 </summary>
        bool IsPlaying { get; }

        /// <summary>
        ///     待機したのち、指定秒数だけVolumeの適用を行う。
        ///     待機中や適用中に再度呼ばれた場合は、終了が遅い方の予約を採用する。
        /// </summary>
        /// <param name="delaySeconds"> 適用を開始するまでの待機秒数です。0以下なら即座に開始します。 </param>
        /// <param name="durationSeconds"> 適用を継続する秒数です。0以下なら何もしません。 </param>
        void Play(float delaySeconds, float durationSeconds);

        /// <summary>
        ///     Volumeの適用と待機中の予約を即座に終了する。
        /// </summary>
        void Stop();
    }
}
