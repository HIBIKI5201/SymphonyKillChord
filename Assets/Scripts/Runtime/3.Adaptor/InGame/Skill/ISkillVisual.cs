using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキルのビジュアル演出を実装するインターフェース。
    /// </summary>
    public interface ISkillVisual
    {
        /// <summary> スキルID。 </summary>
        int Id { get; }
        
        /// <summary> 演出を実行する。 </summary>
        void Execute();
    }
}
