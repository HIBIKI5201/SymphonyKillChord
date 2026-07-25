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

            if (!HasEvaluationItemsChanged(dto.EvaluationItems))
            {
                return;
            }

            _evaluationItems.Clear();

            for (int i = 0; i < dto.EvaluationItems.Length; i++)
            {
                MissionEvaluationItemDTO itemDTO = dto.EvaluationItems[i];

                _evaluationItems.Add(new MissionEvaluationItemViewModel(
                    itemDTO.Description,
                    itemDTO.DisplayState
                ));
            }

            OnEvaluationItemsUpdated?.Invoke((IReadOnlyList<MissionEvaluationItemViewModel>)_evaluationItems);
        }

        /// <summary>
        ///     評価項目に前回適用時からの変化があるかどうかを判定します。
        /// </summary>
        /// <param name="newItems">今回のDTOに含まれる評価項目。</param>
        /// <returns>内容が変化している場合はtrue。</returns>
        private bool HasEvaluationItemsChanged(ReadOnlySpan<MissionEvaluationItemDTO> newItems)
        {
            if (newItems.Length != _evaluationItems.Count)
            {
                return true;
            }

            for (int i = 0; i < newItems.Length; i++)
            {
                MissionEvaluationItemViewModel current = _evaluationItems[i];
                MissionEvaluationItemDTO next = newItems[i];

                if (current.DisplayState != next.DisplayState ||
                    current.Description != next.Description)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> 評価項目のリスト。 </summary>
        private readonly List<MissionEvaluationItemViewModel> _evaluationItems = new();
    }
}
