using KillChord.Runtime.Adaptor.InGame.Skill.Effect;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     Contextで指定されたワールド座標へ設置する配置ストラテジー。
    /// </summary>
    public sealed class WorldPointSkillEffectPlacement : ISkillEffectPlacement
    {
        /// <summary> 設置型のため常にfalseです。 </summary>
        public bool IsFollow => false;

        /// <summary>
        ///     Contextのワールド座標と向きを解決する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="pose"> 解決した配置結果です。 </param>
        /// <returns> 常にtrueです。 </returns>
        public bool TryResolve(in SkillEffectContext context, out SkillEffectPose pose)
        {
            pose = new SkillEffectPose(context.WorldPosition, SkillEffectRotationUtility.FromDirection(context.Direction));
            return true;
        }
    }
}
