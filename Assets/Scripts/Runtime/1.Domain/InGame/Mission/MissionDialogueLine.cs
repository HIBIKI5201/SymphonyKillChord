using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     Ingame会話の情報を保持する構造体。
    /// </summary>
    public readonly struct MissionDialogueLine
    {
        /// <summary>
        ///     台詞1行分のデータを生成する。
        /// </summary>
        public MissionDialogueLine(
            string textEntryKey,
            string fallbackText,
            Sprite portrait,
            string voiceCueName,
            float silentDuration)
        {
            if (string.IsNullOrWhiteSpace(textEntryKey) && string.IsNullOrWhiteSpace(fallbackText))
            {
                throw new ArgumentException("会話テキストとローカライズキーが未設定です。", nameof(fallbackText));
            }
            if (float.IsNaN(silentDuration) || float.IsInfinity(silentDuration) || silentDuration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(silentDuration));
            }
            TextEntryKey = textEntryKey ?? string.Empty;
            FallbackText = fallbackText ?? string.Empty;
            Portrait = portrait;
            VoiceCueName = voiceCueName ?? string.Empty;
            SilentDuration = silentDuration;
        }

        /// <summary> 会話テキストのローカライズキー </summary>
        public string TextEntryKey { get; }
        /// <summary> 翻訳未登録時に表示する会話テキスト </summary>
        public string FallbackText { get; }
        /// <summary> キャラクターアイコン </summary>
        public Sprite Portrait { get; }
        /// <summary> 音声のCue名 </summary>
        public string VoiceCueName { get; }
        /// <summary> 音声Cue未設定時の表示時間 </summary>
        public float SilentDuration { get; }
    }
}
