namespace KillChord.Runtime.View.InGame.Character
{
    /// <summary>
    ///     ワンショット演出の再生契約です。
    /// </summary>
    public interface IOneShotVisualEffect
    {
        /// <summary>
        ///     ワンショット演出を再生します。
        /// </summary>
        /// <param name="cueName"> 足音SEに利用するCueNameです。 </param>
        void Play(string cueName);

        /// <summary>
        ///     ワンショット演出を停止します。
        /// </summary>
        void Stop();
    }
}
