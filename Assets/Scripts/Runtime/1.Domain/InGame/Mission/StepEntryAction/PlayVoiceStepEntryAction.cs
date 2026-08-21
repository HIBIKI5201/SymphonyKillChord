using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.StepEntryAction
{
    /// <summary>
    ///     再生するボイスを表す。
    /// </summary>
    public class PlayVoiceStepEntryAction : IMissionStepEntryAction
    {
        public PlayVoiceStepEntryAction(string voiceCueName)
        {
            if (string.IsNullOrWhiteSpace(voiceCueName))
            {
                throw new ArgumentException(nameof(voiceCueName), $"[PlayVoiceStepEntryAction] 再生ボイスのCueNameが未設定です。");
            }
            _voiceCueName = voiceCueName;
        }
        /// <summary> 再生するボイスのCueName </summary>
        public string VoiceCueName => _voiceCueName;
        private readonly string _voiceCueName;
    }
}
