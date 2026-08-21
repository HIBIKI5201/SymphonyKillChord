using KillChord.Runtime.Adaptor.InGame.Skill.Effect;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     スキルエフェクトの配置方式を表すストラテジーの契約。
    /// </summary>
    public interface ISkillEffectPlacement
    {
        /// <summary> 毎フレーム追従の更新が必要かどうかです。 </summary>
        bool IsFollow { get; }

        /// <summary>
        ///     Contextから配置結果を解決する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="pose"> 解決した配置結果です。 </param>
        /// <returns> 解決に成功した場合はtrue。 </returns>
        bool TryResolve(in SkillEffectContext context, out SkillEffectPose pose);
    }
}
