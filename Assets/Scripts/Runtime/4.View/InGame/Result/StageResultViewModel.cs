using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.View.InGame.Result;
using R3;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///    ステージ結果の表示に必要なデータを保持するビューモデル。
    /// </summary>
    public class StageResultViewModel : IStageResultViewModel
    {
        /// <summary> 勝敗などのリザルトの種類。 </summary>
        public ReactiveProperty<StageResultType> ResultType { get; }
            = new(StageResultType.Victory);

        /// <summary> ステージ名。 </summary>
        public ReactiveProperty<string> StageNameText { get; }
            = new(string.Empty);

        /// <summary> メインミッションの達成状態の文言。 </summary>
        public ReactiveProperty<string> MainMissionStateText { get; }
            = new(string.Empty);

        /// <summary> メインミッションの文言。 </summary>
        public ReactiveProperty<string> MainMissionText { get; }
            = new(string.Empty);

        /// <summary> バトルにかかった秒数。 </summary>
        public ReactiveProperty<float> BattleTimeSeconds { get; }
            = new(0f);

        /// <summary> 最大コンボ数。 </summary>
        public ReactiveProperty<int> MaxCombo { get; }
            = new(0);

        /// <summary> ランクの文言。 </summary>
        public ReactiveProperty<string> RankText { get; }
            = new(string.Empty);

        /// <summary> Tips の文言。 </summary>
        public ReactiveProperty<string> TipsText { get; }
            = new(string.Empty);

        /// <summary> サブミッションの表示項目が更新されたときに発火するイベント。 </summary>
        public event Action<IReadOnlyList<StageResultMissionItemViewModel>> OnSubMissionItemsUpdated;
        /// <summary>
        ///     リザルトの内容を各プロパティへ反映する。
        /// </summary>
        public void Apply(in StageResultDTO dto)
        {
            ResultType.Value = dto.ResultType;
            StageNameText.Value = dto.StageNameText;
            MainMissionText.Value = dto.MainMissionText;
            MainMissionStateText.Value = dto.MainMissionStateText;
            BattleTimeSeconds.Value = dto.BattleTimeSeconds;
            MaxCombo.Value = dto.MaxCombo;
            RankText.Value = dto.RankText;
            TipsText.Value = dto.TipsText;

            _subMissionItems.Clear();

            for (int i = 0; i < dto.SubMissionItems.Length; i++)
            {
                StageResultMissionItemDTO item = dto.SubMissionItems[i];

                _subMissionItems.Add(new StageResultMissionItemViewModel
                    (item.Description,
                    item.IsCompleted));
            }

            OnSubMissionItemsUpdated?.Invoke(_subMissionItems.ToArray());
        }

        private readonly List<StageResultMissionItemViewModel> _subMissionItems = new();
    }
}
