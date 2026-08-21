using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     目標ステップ進入時アクションの実行クラス：<br/>
    ///     ボイスを再生する。
    /// </summary>
    public class PlayVoiceStepEntryActionExecutor : IMissionStepEntryActionExecutor
    {
        public PlayVoiceStepEntryActionExecutor(IPlayableAudioSource voiceSource)
        {
            _voiceSource = voiceSource;
        }
        /// <inheritdoc />
        public Type EntryActionType => typeof(PlayVoiceStepEntryAction);

        /// <inheritdoc />
        public void Execute(IMissionStepEntryAction entryAction)
        {
            if (entryAction is not PlayVoiceStepEntryAction)
            {
                throw new ArgumentException(
                    $"{nameof(entryAction)}の型が{nameof(PlayVoiceStepEntryAction)}ではない。", nameof(entryAction));
            }
            if (_voiceSource == null)
            {
                Debug.LogWarning($"[PlayVoiceStepEntryActionExecutor] ボイス再生用のVoiceSourceがNULLのため、ボイスを再生しません。");
                return;
            }
            _voiceSource.Play(((PlayVoiceStepEntryAction)entryAction).VoiceCueName);
        }

        private IPlayableAudioSource _voiceSource;
    }
}
