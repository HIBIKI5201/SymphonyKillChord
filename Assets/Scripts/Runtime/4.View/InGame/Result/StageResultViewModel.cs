using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.View.InGame.Result;
using R3;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View
{
    public class StageResultViewModel : IStageResultViewModel
    {
        public ReactiveProperty<StageResultType> ResultType { get; }
            = new(StageResultType.Victory);

        public ReactiveProperty<string> StageNameText { get; }
            = new(string.Empty);

        public ReactiveProperty<string> MainMissionStateText { get; }
            = new(string.Empty);

        public ReactiveProperty<string> MainMissionText { get; }
            = new(string.Empty);

        public ReactiveProperty<string> BattleTimeText { get; }
            = new(string.Empty);

        public ReactiveProperty<string> MaxComboText { get; }
            = new(string.Empty);

        public ReactiveProperty<string> RankText { get; }
            = new(string.Empty);

        public ReactiveProperty<string> TipsText { get; }
            = new(string.Empty);

        public event Action<IReadOnlyList<StageResultMissionItemViewModel>> OnSubMissionItemsUpdated;
        public void Apply(in StageResultDTO dto)
        {
            ResultType.Value = dto.ResultType;
            StageNameText.Value = dto.StageNameText;
            MainMissionText.Value = dto.MainMissionText;
            MainMissionStateText.Value = dto.MainMissionStateText;
            BattleTimeText.Value = dto.BattleTimeText;
            MaxComboText.Value = dto.MaxComboText;
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

            OnSubMissionItemsUpdated?.Invoke(_subMissionItems.AsReadOnly());
        }

        private readonly List<StageResultMissionItemViewModel> _subMissionItems = new();
    }
}
