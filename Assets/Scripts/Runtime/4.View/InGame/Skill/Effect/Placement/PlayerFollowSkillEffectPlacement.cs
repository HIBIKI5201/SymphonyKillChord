using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     プレイヤーへ追従する配置ストラテジー。
    /// </summary>
    public sealed class PlayerFollowSkillEffectPlacement : ISkillEffectPlacement
    {
        /// <summary> 追従型のため常にtrueです。 </summary>
        public bool IsFollow => true;

        /// <summary>
        ///     プレイヤーTransformへの追従情報を解決する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="pose"> 解決した配置結果です。 </param>
        /// <returns> プレイヤーが存在する場合はtrue。 </returns>
        public bool TryResolve(in SkillEffectContext context, out SkillEffectPose pose)
        {
            Transform playerTransform = context.PlayerTransform;
            if (playerTransform == null)
            {
                pose = default;
                return false;
            }

            pose = new SkillEffectPose(playerTransform.position, playerTransform.rotation, playerTransform);
            return true;
        }
    }
}
