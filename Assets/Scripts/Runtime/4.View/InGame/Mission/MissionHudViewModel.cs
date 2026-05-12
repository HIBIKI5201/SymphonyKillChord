using KillChord.Runtime.Adaptor.InGame.Mission;
using R3;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View.InGame.Mission
{
    public class MissionHudViewModel : IMissionHudViewModel
    {
        public ReactiveProperty<string> MainMissionText { get; } = new(string.Empty);
        public ReactiveProperty<string> ResultText { get; } = new(string.Empty);

        public event Action<IReadOnlyList<MissionEvaluationItemViewModel>> OnEvaluationItemsUpdated;

        public void Apply(in MissionHudDTO dto)
        {
            MainMissionText.Value = dto.MainMissionText;
            ResultText.Value = dto.ResultText;

            _evaluationItems.Clear();

            for (int i = 0; i < dto.EvaluationItems.Length; i++)
            {
                MissionEvaluationItemDTO itemDTO = dto.EvaluationItems[i];

                _evaluationItems.Add(new MissionEvaluationItemViewModel(
                    itemDTO.Description,
                    itemDTO.IsAchieved
                ));
            }

            OnEvaluationItemsUpdated?.Invoke(_evaluationItems);
        }

        private readonly List<MissionEvaluationItemViewModel> _evaluationItems = new();
    }
}
