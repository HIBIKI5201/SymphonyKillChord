using KillChord.Runtime.Adaptor.OutGame.Audio;
using KillChord.Runtime.Adaptor.Persistent.Music;
using System;

namespace KillChord.Runtime.View.OutGame.Audio
{
    /// <summary>
    ///     UI操作の意味をCueへ解決して操作音を再生するコマンド実装。
    /// </summary>
    public sealed class UISoundEffectCommand : IUISoundEffectCommand
    {
        /// <summary>
        ///     UI操作音の設定と再生ポートを設定する。
        /// </summary>
        /// <param name="config"> UI操作音の解決設定。 </param>
        /// <param name="player"> UI操作音の再生ポート。 </param>
        public UISoundEffectCommand(
            UISoundEffectConfig config,
            IPlayableAudioSource player)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        /// <summary>
        ///     指定したUI操作に対応するCueを解決して再生する。
        /// </summary>
        /// <param name="kind"> UI操作音の種類。 </param>
        public void Play(UISoundEffectKind kind)
        {
            if (_config.TryGetCue(kind, out string cueName))
            {
                _player.Play(cueName);
            }
        }

        private readonly UISoundEffectConfig _config;
        private readonly IPlayableAudioSource _player;
    }
}
