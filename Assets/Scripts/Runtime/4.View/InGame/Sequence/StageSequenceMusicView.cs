using KillChord.Runtime.Adaptor.Persistent.Music;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     ステージシーケンスにおけるBGM再生を管理するViewです。
    /// </summary>
    public class StageSequenceMusicView : MonoBehaviour
    {
        /// <summary>
        ///     BGMを再生するPlayerを設定します。
        /// </summary>
        /// <param name="bgmCuePlayer"> BGM Cueの再生を行うPlayer。 </param>
        public void Initialize(IBgmCuePlayer bgmCuePlayer)
        {
            _bgmCuePlayer = bgmCuePlayer ?? throw new ArgumentNullException(nameof(bgmCuePlayer));
        }

        /// <summary>
        ///     ステージクリア時のBGMを再生します。
        /// </summary>
        public void PlayStageClearBgm()
        {
            PlayBgm(_stageClearCueName);
        }

        /// <summary>
        ///     ゲームオーバー時のBGMを再生します。
        /// </summary>
        public void PlayGameOverBgm()
        {
            PlayBgm(_gameOverCueName);
        }

        [SerializeField, Tooltip("ステージクリア時に再生するBGMのCueName。")]
        private string _stageClearCueName;

        [SerializeField, Tooltip("ゲームオーバー時に再生するBGMのCueName。")]
        private string _gameOverCueName;

        private IBgmCuePlayer _bgmCuePlayer;

        /// <summary>
        ///     指定したBGMを再生します。
        /// </summary>
        /// <param name="cueName"> 再生するCueName。 </param>
        private void PlayBgm(string cueName)
        {
            if (string.IsNullOrWhiteSpace(cueName))
            {
                Debug.LogWarning(
                    $"[{nameof(StageSequenceMusicView)}] BGMのCueNameが設定されていません。",
                    this);
                return;
            }

            _bgmCuePlayer?.SetCue(cueName);
        }
    }
}