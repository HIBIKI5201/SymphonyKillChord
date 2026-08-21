namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     UIから音量設定の変更を受け取るコマンドインターフェース。
    /// </summary>
    public interface IAudioSettingsCommand
    {
        /// <summary>
        ///     BGM音量を設定する。
        /// </summary>
        /// <param name="volume"> 設定する音量。 </param>
        void SetBgmVolume(int volume);

        /// <summary>
        ///     効果音音量を設定する。
        /// </summary>
        /// <param name="volume"> 設定する音量。 </param>
        void SetSoundEffectVolume(int volume);

        /// <summary>
        ///     ボイス音量を設定する。
        /// </summary>
        /// <param name="volume"> 設定する音量。 </param>
        void SetVoiceVolume(int volume);

        /// <summary>
        ///     すべての音量を既定値へ戻す。
        /// </summary>
        void ResetToDefaults();
    }
}
