using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     防御者を表すインターフェース。
    /// </summary>
    public interface IDefender
    {
        /// <summary>
        ///     現在の体力を表すプロパティ。
        /// </summary>
        public Health CurrentHelth { get; }
        /// <summary>
        ///     体力の最大値を表すプロパティ。
        /// </summary>
        public Health MaxHealth { get; }

        /// <summary>
        ///     ダメージを受け取るメソッド。
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(Damage damage);
    }
}
