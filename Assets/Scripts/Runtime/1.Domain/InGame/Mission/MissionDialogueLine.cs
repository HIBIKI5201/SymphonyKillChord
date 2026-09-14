using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     Ingame会話の情報を保持する構造体。
    /// </summary>
    public readonly struct MissionDialogueLine
    {
        public MissionDialogueLine(string text, Sprite portrait, string voiceCueName, float silentDuration)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("会話テキストが未設定です。", nameof(text));
            }
            if (float.IsNaN(silentDuration) || float.IsInfinity(silentDuration) || silentDuration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(silentDuration));
            }
            Text = text;
            Portrait = portrait;
            VoiceCueName = voiceCueName ?? string.Empty;
            SilentDuration = silentDuration;
        }

        /// <summary> 会話テキスト </summary>
        public string Text { get; }
        /// <summary> キャラクターアイコン </summary>
        public Sprite Portrait { get; }
        /// <summary> 音声のCue名 </summary>
        public string VoiceCueName { get; }
        /// <summary> 音声Cue未設定時の表示時間 </summary>
        public float SilentDuration { get; }
    }
}
