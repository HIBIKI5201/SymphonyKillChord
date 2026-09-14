using KillChord.Runtime.View.Persistent.Music;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     ステージシーケンスにおけるSE再生を管理するViewです。
    /// </summary>
    public class StageSequenceSoundEffectView : MonoBehaviour
    {
        /// <summary>
        ///     ステージクリア時のSEを再生します。
        /// </summary>
        public void PlayStageClearSe()
        {
            _stageClearSoundSource?.Play();
        }

        /// <summary>
        ///     ゲームオーバー時のSEを再生します。
        /// </summary>
        public void PlayGameOverSe()
        {
            _gameOverSoundSource?.Play();
        }

        [SerializeField, Tooltip("ステージクリア時に再生するSE（SE_Gameclear）用Source。")]
        private SoundEffectSource _stageClearSoundSource;

        [SerializeField, Tooltip("ゲームオーバー時に再生するSE（SE_Gameover）用Source。")]
        private SoundEffectSource _gameOverSoundSource;
    }
}
