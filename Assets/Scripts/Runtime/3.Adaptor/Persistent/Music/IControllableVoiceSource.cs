namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     会話音声の再生を制御するインタフェース。
    /// </summary>
    public interface IControllableVoiceSource
    {
        /// <summary> 音声が再生中か（一時停止の場合も再生中と見なす） </summary>
        public bool IsVoiceActive { get; }
        /// <summary> 再生失敗したか </summary>
        public bool HasVoiceError { get; }

        /// <summary>
        ///     音声を再生する。
        /// </summary>
        public bool TryPlayVoice(string cueName);

        /// <summary>
        ///     音声再生を停止する。
        /// </summary>
        void StopVoice();

        /// <summary>
        ///     音声再生を一時停止する。
        /// </summary>
        void SetVoicePaused(bool isPaused);
    }
}
