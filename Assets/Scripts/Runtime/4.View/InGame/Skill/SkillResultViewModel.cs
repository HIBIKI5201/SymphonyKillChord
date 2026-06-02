using KillChord.Runtime.Adaptor.InGame.Skill;
using System;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル結果表示用の状態を管理するViewModelクラス。
    /// </summary>
    public class SkillResultViewModel : ISkillResultViewModel
    {
        /// <summary> スキル結果の変更を通知するイベント。 </summary>
        public event Action<int, ReadOnlyMemory<int>> OnChanged;

        /// <summary> 現在のスキルID。 </summary>
        public int SkillId { get; private set; }

        /// <summary> 現在のスキルパターン（読み取り専用メモリ）。 </summary>
        public ReadOnlyMemory<int> SkillPattern { get; private set; }

        /// <summary>
        ///     DTOからスキル結果の状態を更新し、変更イベントを通知するメソッド。
        /// </summary>
        /// <param name="dto"></param>
        public void Push(in SkillResultDTO dto)
        {
            SkillId = dto.SkillId;
            SkillPattern = dto.SkillPattern;
            OnChanged?.Invoke(SkillId, SkillPattern);
        }
    }
}
