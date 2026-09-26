namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     ノード解放時のステータスボーナス効果を表すインターフェース。
    /// </summary>
    public interface IStatusBonusEffect
    {
        /// <summary> この効果の種別。 </summary>
        public StatusBonusEffectKind Kind { get; }

        /// <summary>
        ///     プレイヤーステータスのボーナスへ効果を適用する。
        /// </summary>
        /// <param name="builder"> ボーナスの集計先。 </param>
        public void Apply(PlayerStatusBonusBuilder builder);
    }
}
