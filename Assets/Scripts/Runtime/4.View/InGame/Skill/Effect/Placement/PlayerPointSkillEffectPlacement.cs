using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     再生時点のプレイヤー位置へ設置する配置ストラテジー。
    /// </summary>
    public sealed class PlayerPointSkillEffectPlacement : ISkillEffectPlacement
    {
        /// <summary> 設置型のため常にfalseです。 </summary>
        public bool IsFollow => false;

        /// <summary>
        ///     再生時点のプレイヤー位置を解決する。
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

            pose = new SkillEffectPose(playerTransform.position, playerTransform.rotation);
            return true;
        }
    }
}
