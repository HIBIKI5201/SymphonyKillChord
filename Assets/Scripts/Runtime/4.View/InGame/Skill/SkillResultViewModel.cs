using KillChord.Runtime.Adaptor;
using System;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     スキル結果表示用の状態を管理するViewModelクラス。
    /// </summary>
    public class SkillResultViewModel : ISkillResultViewModel
    {
        public event Action<int, ReadOnlyMemory<int>> OnChanged;

        public int SkillId { get; private set; }

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
