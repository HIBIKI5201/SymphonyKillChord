namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     音量適用可能なAudio Sourceの共通インターフェース。
    /// </summary>
    public interface IVolumeApplicable
    {
        /// <summary>
        ///     音量を適用します。
        /// </summary>
        void ApplyVolume(float volume);
    }
}
