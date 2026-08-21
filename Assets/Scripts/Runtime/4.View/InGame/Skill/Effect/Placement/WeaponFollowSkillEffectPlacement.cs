using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     プレイヤーが構えている武器へ追従する配置ストラテジー。
    ///     武器を解決できない場合はプレイヤーへ追従する。
    /// </summary>
    public sealed class WeaponFollowSkillEffectPlacement : ISkillEffectPlacement
    {
        /// <summary> 追従型のため常にtrueです。 </summary>
        public bool IsFollow => true;

        /// <summary>
        ///     武器の現在の姿勢を解決する。毎フレーム呼ばれることで追従になる。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="pose"> 解決した配置結果です。 </param>
        /// <returns> 武器またはプレイヤーが存在する場合はtrue。 </returns>
        public bool TryResolve(in SkillEffectContext context, out SkillEffectPose pose)
        {
            // 武器が表示されていない場面でも破綻しないよう、プレイヤーへ退避する。
            Transform anchor = context.HasWeapon ? context.WeaponTransform : context.PlayerTransform;
            if (anchor == null)
            {
                pose = default;
                return false;
            }

            pose = new SkillEffectPose(anchor.position, anchor.rotation);
            return true;
        }
    }
}
