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
        ///     評価項目の達成度に応じたステージクリア時のPlayer Voiceを再生します。
        /// </summary>
        /// <param name="achievedCount"> 達成した評価項目数です。 </param>
        /// <param name="totalCount"> 評価項目の合計数です。 </param>
        public void PlayStageClearVoice(int achievedCount, int totalCount)
        {
            _playerView?.PlayStageClearVoice(achievedCount, totalCount);
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
