using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     会話UIを表示し、ボイスを再生するStep開始時処理の定義。
    /// </summary>
    [Serializable]
    public sealed class PlayDialogueStepEntryActionAsset : MissionStepEntryActionAssetBase
    {
        /// <inheritdoc />
        public override IMissionStepEntryAction Create()
        {
            List<MissionDialogueLine> lines = new();
            if (_lines != null)
            {
                for (int i = 0; i < _lines.Count; i++)
                {
                    if (_lines[i] == null)
                    {
                        throw new InvalidOperationException($"台詞[{i}]が未設定です。");
                    }
                    lines.Add(_lines[i].Create());
                }
            }
            return new PlayDialogueStepEntryAction(lines, !_continueAfterStepChange);
        }

        [SerializeField, Tooltip("再生する会話のリスト")]
        private List<MissionDialogueLineAsset> _lines = new() { new MissionDialogueLineAsset() };

        // 未設定の既存アセットはfalseとなり、従来のステップ変更時キャンセルを維持する。
        [SerializeField, Tooltip("ステップが変わっても会話全体を最後まで続けます。次の会話やゲームプレイ終了時は停止します。")]
        private bool _continueAfterStepChange;

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            return $"会話を再生する：{_lines?.Count ?? 0}件";
        }
    }
}
