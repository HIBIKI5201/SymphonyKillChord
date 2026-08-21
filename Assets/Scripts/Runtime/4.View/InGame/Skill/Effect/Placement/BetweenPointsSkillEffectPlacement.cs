using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     プレイヤーと対象を結ぶ線上へ配置するストラテジー。
    ///     ビームや軌跡のように2点を必要とする演出で使用する。
    /// </summary>
    public sealed class BetweenPointsSkillEffectPlacement : ISkillEffectPlacement
    {
        /// <summary>
        ///     補間比率を指定して生成する。
        /// </summary>
        /// <param name="ratio"> プレイヤーを0、対象を1とした補間比率です。 </param>
        public BetweenPointsSkillEffectPlacement(float ratio)
        {
            _ratio = Mathf.Clamp01(ratio);
        }

        /// <summary> 両端が動くため常にtrueです。 </summary>
        public bool IsFollow => true;

        /// <summary>
        ///     プレイヤーと対象の座標から補間位置を計算する。
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

            // 対象が消滅している場合も、解決済みのワールド座標を終点として扱う。
            Vector3 startPosition = playerTransform.position;
            Vector3 endPosition = context.HasTarget ? context.TargetTransform.position : context.WorldPosition;

            Vector3 direction = endPosition - startPosition;
            pose = new SkillEffectPose(
                Vector3.Lerp(startPosition, endPosition, _ratio),
                SkillEffectRotationUtility.FromDirection(direction));
            return true;
        }

        private readonly float _ratio;
    }
}
