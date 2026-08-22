using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill.Effect
{
    /// <summary>
    ///     エフェクトの取り付け先となる武器を提供する契約。
    /// </summary>
    public interface ISkillEffectWeaponSource
    {
        /// <summary> 現在構えている武器のTransformです。武器が無い場合はnull。 </summary>
        Transform WeaponTransform { get; }
    }
}
