using KillChord.Runtime.View.InGame.Player;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     ステージシーケンスにおけるVoice再生を管理するView。
    ///     Playerが動的に生成されるため、Bindingsでの参照ができないため、StageSequenceViewからPlayerViewを受け取る形でVoice再生を行う。
    /// </summary>
    public class StageSequenceVoiceView : MonoBehaviour
    {
        /// <summary>
        ///     Voiceを再生するPlayerViewを設定します。
        /// </summary>
        /// <param name="playerView"> Voiceを再生するPlayerView。 </param>
        public void Initialize(PlayerView playerView)
        {
            _playerView = playerView
                ?? throw new ArgumentNullException(nameof(playerView));
        }

        /// <summary>
        ///     ステージ開始時のPlayer Voiceを再生します。
        /// </summary>
        public void PlayStageStartVoice()
        {
            _playerView?.PlayStageStartVoice();
        }

        /// <summary>
        ///     ステージクリア時のPlayer Voiceを再生します。
        /// </summary>
        public void PlayStageClearVoice()
        {
            _playerView?.PlayStageClearVoice();
        }

        /// <summary>
        ///     ゲームオーバー時のPlayer Voiceを再生します。
        /// </summary>
        public void PlayGameOverVoice()
        {
            _playerView?.PlayGameOverVoice();
        }

        private PlayerView _playerView;
    }
}
