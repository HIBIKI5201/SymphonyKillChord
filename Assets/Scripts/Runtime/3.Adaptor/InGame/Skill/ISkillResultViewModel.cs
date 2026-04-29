using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     スキルの結果を表示するためのインターフェース。
    /// </summary>
    public interface ISkillResultViewModel
    {
        /// <summary>
        ///     スキルの結果を表示するためのメソッド。
        /// </summary>
        /// <param name="dto"></param>
        void Push(in SkillResultDTO dto);
    }
}
