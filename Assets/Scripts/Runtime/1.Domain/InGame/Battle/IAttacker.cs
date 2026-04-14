using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃者を表すインターフェース。
    /// </summary>
    public interface IAttacker
    {
        /// <summary>
        ///     攻撃力を表すプロパティ。
        /// </summary>
        public AttackPower AttackPower { get; }
    }
}
