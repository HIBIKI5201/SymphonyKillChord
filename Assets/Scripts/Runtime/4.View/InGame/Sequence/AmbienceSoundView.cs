using KillChord.Runtime.View.Persistent.Music;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     ステージ開始演出における環境音（Ambience）の再生を管理するViewです。
    /// </summary>
    public class AmbienceSoundView : MonoBehaviour
    {
        /// <summary>
        ///     環境音の再生を開始します。ステージ開始演出（タイムライン）の再生開始と同時に呼び出します。
        /// </summary>
        public void PlayAmbience()
        {
            _ambienceSoundSource?.Play();
        }

        /// <summary>
        ///     環境音の再生を停止します。インゲームBGMの再生開始と同時に呼び出します。
        /// </summary>
        public void StopAmbience()
        {
            _ambienceSoundSource?.Stop();
        }

        [SerializeField, Tooltip("環境音（SE_Ambience_Ingame）用Source。")]
        private SoundEffectSource _ambienceSoundSource;
    }
}
