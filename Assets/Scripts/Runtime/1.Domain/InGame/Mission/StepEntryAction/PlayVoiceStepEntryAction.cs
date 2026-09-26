using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.StepEntryAction
{
    /// <summary>
    ///     再生するボイスを表す。
    /// </summary>
    public class PlayVoiceStepEntryAction : IMissionStepEntryAction
    {
        /// <summary>
        ///     再生するボイスの CueName を指定して生成する。
        ///     未設定の場合は例外を投げる。
        /// </summary>
        public PlayVoiceStepEntryAction(string voiceCueName)
        {
            if (string.IsNullOrWhiteSpace(voiceCueName))
            {
                throw new ArgumentException($"[PlayVoiceStepEntryAction] 再生ボイスのCueNameが未設定です。", nameof(voiceCueName));
            }
            _voiceCueName = voiceCueName;
        }
        /// <summary> 再生するボイスのCueName </summary>
        public string VoiceCueName => _voiceCueName;
        private readonly string _voiceCueName;
    }
}
