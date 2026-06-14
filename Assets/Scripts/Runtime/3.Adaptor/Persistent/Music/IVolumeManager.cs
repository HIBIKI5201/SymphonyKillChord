namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     音量管理の共通インターフェース。
    /// </summary>
    public interface IVolumeManager
    {
        /// <summary>
        ///     音量を設定する。
        /// </summary>
        void SetVolume(float volume);
    }
}
