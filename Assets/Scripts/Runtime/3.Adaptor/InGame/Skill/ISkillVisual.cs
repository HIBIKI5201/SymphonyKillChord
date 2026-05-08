using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    /// スキルのビジュアル演出を実装するインターフェース。
    /// </summary>
    public interface ISkillVisual
    {
        /// <summary>
        /// スキルに対応する識別子。
        /// </summary>
        int Id { get; }

        /// <summary>
        /// ビジュアル演出を実行する。
        /// </summary>
        void Execute();
    }
}
