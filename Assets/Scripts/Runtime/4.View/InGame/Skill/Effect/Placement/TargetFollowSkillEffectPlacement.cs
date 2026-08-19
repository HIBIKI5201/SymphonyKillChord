using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     対象へ追従する配置ストラテジー。
    /// </summary>
    public sealed class TargetFollowSkillEffectPlacement : ISkillEffectPlacement
    {
        /// <summary> 追従型のため常にtrueです。 </summary>
        public bool IsFollow => true;

        /// <summary>
        ///     対象Transformへの追従情報を解決する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="pose"> 解決した配置結果です。 </param>
        /// <returns> 対象が存在する場合はtrue。 </returns>
        public bool TryResolve(in SkillEffectContext context, out SkillEffectPose pose)
        {
            Transform targetTransform = context.TargetTransform;
            if (targetTransform == null)
            {
                pose = default;
                return false;
            }

            pose = new SkillEffectPose(targetTransform.position, targetTransform.rotation, targetTransform);
            return true;
        }
    }
}
