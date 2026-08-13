using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.StatusEffect;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃者を表すインターフェース。
    /// </summary>
    public interface IAttacker
    {
        /// <summary>
        ///     攻撃者側のバフシステムを取得する。
        /// </summary>
        IBuffSystem BuffSystem { get; }

        /// <summary> 状態効果システムを取得する。 </summary>
        IStatusEffectSystem StatusEffectSystem { get; }

        /// <summary>
        ///     攻撃者の会心率を取得する。会心率は武器ではなく攻撃者が持つ。
        /// </summary>
        CriticalChance CriticalChance { get; }
    }
}
