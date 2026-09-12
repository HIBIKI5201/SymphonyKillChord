using KillChord.Runtime.Adaptor.InGame.Skill.Effect;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     配置方式に対応するストラテジーを解決する静的クラス。
    /// </summary>
    public static class SkillEffectPlacementResolver
    {
        /// <summary>
        ///     配置方式に対応するストラテジーを取得する。
        /// </summary>
        /// <param name="attachMode"> 配置方式です。 </param>
        /// <param name="betweenRatio"> 2点間配置で使用する補間比率です。 </param>
        /// <returns> 対応するストラテジーです。 </returns>
        public static ISkillEffectPlacement Resolve(SkillEffectAttachMode attachMode, float betweenRatio = DEFAULT_BETWEEN_RATIO)
        {
            // 補間比率を持つ2点間配置のみ、定義ごとに生成する。
            if (attachMode == SkillEffectAttachMode.BetweenPlayerAndTarget)
            {
                return new BetweenPointsSkillEffectPlacement(betweenRatio);
            }

            // それ以外のストラテジーは状態を持たないため、GC削減のためインスタンスを使い回す。
            return attachMode switch
            {
                SkillEffectAttachMode.PlayerFollow => PLAYER_FOLLOW,
                SkillEffectAttachMode.PlayerPoint => PLAYER_POINT,
                SkillEffectAttachMode.TargetFollow => TARGET_FOLLOW,
                SkillEffectAttachMode.TargetPoint => TARGET_POINT,
                SkillEffectAttachMode.WorldPoint => WORLD_POINT,
                SkillEffectAttachMode.WeaponFollow => WEAPON_FOLLOW,
                _ => PLAYER_POINT,
            };
        }

        private const float DEFAULT_BETWEEN_RATIO = 0.5f;

        private static readonly ISkillEffectPlacement PLAYER_FOLLOW = new PlayerFollowSkillEffectPlacement();
        private static readonly ISkillEffectPlacement PLAYER_POINT = new PlayerPointSkillEffectPlacement();
        private static readonly ISkillEffectPlacement TARGET_FOLLOW = new TargetFollowSkillEffectPlacement();
        private static readonly ISkillEffectPlacement TARGET_POINT = new TargetPointSkillEffectPlacement();
        private static readonly ISkillEffectPlacement WORLD_POINT = new WorldPointSkillEffectPlacement();
        private static readonly ISkillEffectPlacement WEAPON_FOLLOW = new WeaponFollowSkillEffectPlacement();
    }
}
