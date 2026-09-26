namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     音量設定の表示値をViewModelへ渡すDTO。
    /// </summary>
    public readonly ref struct AudioSettingsViewDTO
    {
        /// <summary>
        ///     音量設定の表示値を初期化する。
        /// </summary>
        public AudioSettingsViewDTO(
            int bgmVolume,
            int soundEffectVolume,
            int voiceVolume)
        {
            BgmVolume = bgmVolume;
            SoundEffectVolume = soundEffectVolume;
            VoiceVolume = voiceVolume;
        }

        /// <summary> BGM音量。 </summary>
        public int BgmVolume { get; }

        /// <summary> 効果音音量。 </summary>
        public int SoundEffectVolume { get; }

        /// <summary> ボイス音量。 </summary>
        public int VoiceVolume { get; }
    }
}
