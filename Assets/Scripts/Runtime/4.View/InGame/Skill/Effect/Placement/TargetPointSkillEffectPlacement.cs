using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     再生時点の対象位置へ設置する配置ストラテジー。
    /// </summary>
    public sealed class TargetPointSkillEffectPlacement : ISkillEffectPlacement
    {
        /// <summary> 設置型のため常にfalseです。 </summary>
        public bool IsFollow => false;

        /// <summary>
        ///     再生時点の対象位置を解決する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="pose"> 解決した配置結果です。 </param>
        /// <returns> 常にtrueです。 </returns>
        public bool TryResolve(in SkillEffectContext context, out SkillEffectPose pose)
        {
            Transform targetTransform = context.TargetTransform;
            if (targetTransform != null)
            {
                pose = new SkillEffectPose(targetTransform.position, targetTransform.rotation);
                return true;
            }

            // 対象が既に消滅している場合は、解決済みのワールド座標へフォールバックする。
            pose = new SkillEffectPose(context.WorldPosition, SkillEffectRotationUtility.FromDirection(context.Direction));
            return true;
        }
    }
}
