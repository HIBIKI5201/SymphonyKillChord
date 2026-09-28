using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission.StepEntryAction
{
    /// <summary>
    ///     ステップ開始時に順番に再生する会話を保持する。
    /// </summary>
    public sealed class PlayDialogueStepEntryAction : IMissionStepEntryAction
    {
        /// <summary>
        ///     会話とステップ変更時のキャンセル方針を設定する。
        /// </summary>
        public PlayDialogueStepEntryAction(IReadOnlyList<MissionDialogueLine> lines,
            bool isStepChangeCancellationEnabled = true)
        {
            if (lines == null || lines.Count == 0)
            {
                throw new ArgumentException("会話情報がありません。", nameof(lines));
            }
            List<MissionDialogueLine> copy = new(lines.Count);
            for (int i = 0; i < lines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i].TextEntryKey)
                    && string.IsNullOrWhiteSpace(lines[i].FallbackText))
                {
                    throw new ArgumentException($"台詞[{i}]が未設定です。", nameof(lines));
                }
                copy.Add(lines[i]);
            }
            Lines = copy.AsReadOnly();
            IsStepChangeCancellationEnabled = isStepChangeCancellationEnabled;
        }

        /// <summary> 順番に再生する台詞 </summary>
        public IReadOnlyList<MissionDialogueLine> Lines { get; }

        /// <summary> ステップ変更時に音声と字幕をキャンセルするか。 </summary>
        public bool IsStepChangeCancellationEnabled { get; }
    }
}
