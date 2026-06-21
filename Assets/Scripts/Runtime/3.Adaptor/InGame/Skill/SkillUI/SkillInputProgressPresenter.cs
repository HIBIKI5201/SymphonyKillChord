using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行状態をViewに反映するプレゼンタークラス。
    /// </summary>
    public class SkillInputProgressPresenter
    {
        public SkillInputProgressPresenter(ISkillInputProgressRowView rowView)
        {
            _rowView = rowView ?? throw new ArgumentNullException(nameof(rowView));
        }

        /// <summary>
        ///     スキル入力進行状態を更新する。
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateRow(SkillInputProgressUpdateDTO dto)
        {
            _rowView.UpdateSteps(dto);
        }
        
        private readonly ISkillInputProgressRowView _rowView;
    }
}
