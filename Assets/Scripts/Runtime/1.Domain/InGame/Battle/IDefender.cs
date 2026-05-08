using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     防御者を表すインターフェース。
    /// </summary>
    public interface IDefender
    {
        /// <summary>
        ///     現在の体力を取得する。
        /// </summary>
        public Health CurrentHealth { get; }

        /// <summary>
        ///     体力の最大値を取得する。
        /// </summary>
        public Health MaxHealth { get; }

        /// <summary>
        ///     ダメージを受ける。
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(Damage damage);
    }
}
