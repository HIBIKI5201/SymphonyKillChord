using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// ViewModelへスキル結果DTOを渡すためのインターフェース。
    /// </summary>
    public interface ISkillResultViewModel
    {
        /// <summary>
        /// スキル結果DTOを受け取り、ViewModel側の状態を更新する。
        /// </summary>
        /// <param name="dto">スキル結果DTO（読み取り専用）</param>
        void Push(in SkillResultDTO dto);
    }
}
