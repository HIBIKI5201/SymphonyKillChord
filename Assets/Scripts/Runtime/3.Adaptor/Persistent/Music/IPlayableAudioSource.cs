namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     再生可能なAudio Sourceの共通インターフェース。
    /// </summary>
    public interface IPlayableAudioSource
    {
        /// <summary>
        ///     Source側に設定されているCueを再生します。
        /// </summary>
        void Play();

        /// <summary>
        ///     CueNameを指定して再生します。
        /// </summary>
        void Play(string cueName);
    }
}
