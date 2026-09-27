using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     会話表示関連のDTO。
    /// </summary>
    public readonly ref struct MissionDialogueDTO
    {
        /// <summary>
        ///     台詞表示の内容と状態を指定して生成する。
        /// </summary>
        public MissionDialogueDTO(
            string textEntryKey,
            string fallbackText,
            Sprite portrait,
            bool isVisible,
            bool isPaused,
            int version,
            bool isImmediate)
        {
            TextEntryKey = textEntryKey;
            FallbackText = fallbackText;
            Portrait = portrait;
            IsVisible = isVisible;
            IsPaused = isPaused;
            Version = version;
            IsImmediate = isImmediate;
        }

        /// <summary> 会話テキストのローカライズキー </summary>
        public string TextEntryKey { get; }
        /// <summary> 翻訳未登録時に表示する会話テキスト </summary>
        public string FallbackText { get; }
        /// <summary> 顔画像 </summary>
        public Sprite Portrait { get; }
        /// <summary> 表示するか </summary>
        public bool IsVisible { get; }
        /// <summary> ポーズ状態 </summary>
        public bool IsPaused { get; }
        /// <summary> 演出要求の世代番号 </summary>
        public int Version { get; }
        /// <summary> 即時反映するか </summary>
        public bool IsImmediate { get; }
    }
}
