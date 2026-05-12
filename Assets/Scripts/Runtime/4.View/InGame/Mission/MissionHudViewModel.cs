using KillChord.Runtime.Adaptor.InGame.Mission;
using R3;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     ミッションHUDの表示内容を管理するViewModelクラス。
    /// </summary>
    public class MissionHudViewModel : IMissionHudViewModel
    {
        /// <summary> メインミッションのテキスト。 </summary>
        public ReactiveProperty<string> MainMissionText { get; } = new(string.Empty);
        /// <summary> ミッション結果のテキスト。 </summary>
        public ReactiveProperty<string> ResultText { get; } = new(string.Empty);

        /// <summary> 評価項目のリストが更新された際のイベント。 </summary>
        public event Action<IReadOnlyList<MissionEvaluationItemViewModel>> OnEvaluationItemsUpdated;

        /// <summary>
        ///     DTOを元にViewModelの状態を更新します。
        /// </summary>
        /// <param name="dto">ミッションHUDのDTO。</param>
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

        /// <summary> 評価項目のリスト。 </summary>
        private readonly List<MissionEvaluationItemViewModel> _evaluationItems = new();
    }
}
