using UnityEngine;

namespace KillChord.Runtime.Domain
{
    /// <summary>
    /// スキル効果を表す抽象インターフェース。
    /// </summary>
    public interface ISkillEffect
    {
        /// <summary>
        /// スキル効果を実行する。
        /// </summary>
        void Execute();
    }
}
