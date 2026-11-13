using KillChord.Runtime.View.Persistent.Audio;

namespace KillChord.Runtime.View.Persistent.Voice
{
    /// <summary>
    ///     登録されたVoice Sourceの音量を一括管理するクラス。
    /// </summary>
    public class VoiceVolumeManager : AudioSourceVolumeManager<VoiceSource>
    {
        /// <summary>
        ///     暫定のボイス音量で初期化する。
        /// </summary>
        public VoiceVolumeManager() : base(INITIAL_VOLUME)
        {
        }

        /// <summary> 音量設定の読み込み前に登録されたSourceへ適用する暫定音量です。 </summary>
        private const float INITIAL_VOLUME = 0.8f;
    }
}
