using R3;

namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     UIへ共通音量設定を公開するViewModelインターフェース。
    /// </summary>
    public interface IAudioSettingsViewModel
    {
        /// <summary> BGM音量。 </summary>
        ReadOnlyReactiveProperty<int> BgmVolume { get; }

        /// <summary> 効果音音量。 </summary>
        ReadOnlyReactiveProperty<int> SoundEffectVolume { get; }

        /// <summary> ボイス音量。 </summary>
        ReadOnlyReactiveProperty<int> VoiceVolume { get; }

        /// <summary>
        ///     表示用DTOを音量設定へ反映する。
        /// </summary>
        /// <param name="dto"> 反映する音量設定の表示用DTO。 </param>
        void Apply(in AudioSettingsViewDTO dto);
    }
}
