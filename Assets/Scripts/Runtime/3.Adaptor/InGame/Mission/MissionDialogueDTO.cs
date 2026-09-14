using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     会話表示関連のDTO。
    /// </summary>
    public readonly ref struct MissionDialogueDTO
    {
        public MissionDialogueDTO(string text, Sprite portrait, bool isVisible, bool isPaused, int version, bool isImmediate)
        {
            Text = text;
            Portrait = portrait;
            IsVisible = isVisible;
            IsPaused = isPaused;
            Version = version;
            IsImmediate = isImmediate;
        }

        /// <summary> 会話テキスト </summary>
        public string Text { get; }
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
