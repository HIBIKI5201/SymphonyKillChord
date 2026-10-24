using KillChord.Runtime.View.Persistent.Audio;

namespace KillChord.Runtime.View.Persistent.Music
{
    /// <summary>
    ///     登録されたSE Sourceの音量を一括管理するクラス。
    /// </summary>
    public class SoundEffectVolumeManager : AudioSourceVolumeManager<SoundEffectSource>
    {
        /// <summary>
        ///     暫定のSE音量で初期化する。
        /// </summary>
        public SoundEffectVolumeManager() : base(INITIAL_VOLUME)
        {
        }

        /// <summary> 音量設定の読み込み前に登録されたSourceへ適用する暫定音量です。 </summary>
        private const float INITIAL_VOLUME = 0.4f;
    }
}
