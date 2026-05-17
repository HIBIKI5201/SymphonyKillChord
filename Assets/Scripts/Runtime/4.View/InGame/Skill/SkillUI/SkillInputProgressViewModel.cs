using KillChord.Runtime.Adaptor.InGame.Skill;
using R3;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力の進捗を管理するViewModelクラス。
    /// </summary>
    public class SkillInputProgressViewModel : ISkillInputProgressViewModel
    {
        /// <summary>
        ///     スキル入力の進捗を管理するViewModelを生成する。
        /// </summary>
        /// <param name="visualConfig"> スキル入力進行UIの表示設定。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillInputProgressViewModel(SkillInputProgressViewconfig visualConfig)
        {
            _visualConfig = visualConfig ?? throw new ArgumentNullException(nameof(visualConfig));
        }

        /// <summary> スキル入力進行UIの行データのリスト。 </summary>
        public ReadOnlyReactiveProperty<IReadOnlyList<SkillInputProgressRowData>> Rows => _rows;

        public void Apply(in SkillInputProgressRowDTO dto)
        {
            List<SkillInputProgressStepData> steps = new();

            for (int i = 0; i < dto.Steps.Length; i++)
            {
                SkillInputProgressStepDTO stepDTO = dto.Steps[i];
                SkillBeatVisualSetting setting = _visualConfig.GetSetting(stepDTO.BeatType);

                steps.Add(new SkillInputProgressStepData(
                    stepDTO.BeatType,
                    stepDTO.IsActive,
                    stepDTO.IsActive ? setting.ActiveColor : setting.NormalColor,
                    setting.Icon));
            }

            _rowBuffer.Add(new SkillInputProgressRowData(dto.SkillId, steps));
        }

        public void Clear()
        {
            _rowBuffer.Clear();
            _rows.Value = Array.Empty<SkillInputProgressRowData>();
        }

        public void Commit()
        {
            _rows.Value = _rowBuffer.ToArray();
        }

        private readonly SkillInputProgressViewconfig _visualConfig;
        private readonly ReactiveProperty<IReadOnlyList<SkillInputProgressRowData>> _rows = new(Array.Empty<SkillInputProgressRowData>());
        private readonly List<SkillInputProgressRowData> _rowBuffer = new();
    }
}
