using KillChord.Runtime.Adaptor.InGame.Mission;
using R3;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     会話表示用の情報を保持するViewModel。
    /// </summary>
    public sealed class MissionDialogueViewModel : IMissionDialogueViewModel, IDisposable
    {
        /// <summary> 表示内容変更の通知を担当するReactiveProperty </summary>
        public ReadOnlyReactiveProperty<int> Revision => _revision;
        /// <summary> 会話テキストのローカライズキー </summary>
        public string TextEntryKey { get; private set; }
        /// <summary> 翻訳未登録時に表示する会話テキスト </summary>
        public string FallbackText { get; private set; }
        /// <summary> 顔画像 </summary>
        public Sprite Portrait { get; private set; }
        /// <summary> 表示状態 </summary>
        public bool IsVisible { get; private set; }
        /// <summary> ポーズ状態 </summary>
        public bool IsPaused { get; private set; }
        /// <summary> 演出要求の世代番号 </summary>
        public int Version { get; private set; }
        /// <summary> 即時反映する </summary>
        public bool IsImmediate { get; private set; }

        /// <inheritdoc />
        public void Apply(in MissionDialogueDTO dto)
        {
            TextEntryKey = dto.TextEntryKey;
            FallbackText = dto.FallbackText;
            Portrait = dto.Portrait;
            IsVisible = dto.IsVisible;
            IsPaused = dto.IsPaused;
            Version = dto.Version;
            IsImmediate = dto.IsImmediate;
            _revision.Value++;
        }

        /// <inheritdoc />
        public void Dispose() => _revision.Dispose();

        private readonly ReactiveProperty<int> _revision = new(0);
    }
}
