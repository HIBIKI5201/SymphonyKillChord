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
        /// <returns> 対応するストラテジーです。 </returns>
        public static ISkillEffectPlacement Resolve(SkillEffectAttachMode attachMode)
        {
            // ストラテジーは状態を持たないため、GC削減のためインスタンスを使い回す。
            return attachMode switch
            {
                SkillEffectAttachMode.PlayerFollow => PLAYER_FOLLOW,
                SkillEffectAttachMode.PlayerPoint => PLAYER_POINT,
                SkillEffectAttachMode.TargetFollow => TARGET_FOLLOW,
                SkillEffectAttachMode.TargetPoint => TARGET_POINT,
                SkillEffectAttachMode.WorldPoint => WORLD_POINT,
                _ => PLAYER_POINT,
            };
        }

        private static readonly ISkillEffectPlacement PLAYER_FOLLOW = new PlayerFollowSkillEffectPlacement();
        private static readonly ISkillEffectPlacement PLAYER_POINT = new PlayerPointSkillEffectPlacement();
        private static readonly ISkillEffectPlacement TARGET_FOLLOW = new TargetFollowSkillEffectPlacement();
        private static readonly ISkillEffectPlacement TARGET_POINT = new TargetPointSkillEffectPlacement();
        private static readonly ISkillEffectPlacement WORLD_POINT = new WorldPointSkillEffectPlacement();
    }
}
